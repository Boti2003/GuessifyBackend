using GuessifyBackend.DTO.GameModel;
using GuessifyBackend.DTO.LobbyModel;
using GuessifyBackend.Entities;
using GuessifyBackend.Hubs;
using GuessifyBackend.Models.Enum;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GuessifyBackend.Service
{
    public class GameService
    {
        private readonly GameDbContext _dbContext;

        private readonly CategoryService _categoryService;

        private readonly QuestionService _questionService;

        private readonly DeezerApiService _deezerApiService;

        private readonly GameEventManager _eventManager;

        private readonly VotingService _votingService;

        private readonly UserService _userService;

        private IHubContext<GameHub, IGameClient> _gameHubContext { get; }

        //private List<Game> games = new List<Game>();

        public GameService(GameDbContext dbContext, CategoryService categoryService, QuestionService questionService, DeezerApiService deezerApiService, IHubContext<GameHub, IGameClient> hubContext, GameEventManager gameEventManager, VotingService votingService, UserService userService)
        {
            _categoryService = categoryService;
            _dbContext = dbContext;
            _questionService = questionService;
            _deezerApiService = deezerApiService;
            _gameHubContext = hubContext;
            _eventManager = gameEventManager;
            _votingService = votingService;
            _userService = userService;
        }

        public async Task<GameDto> StartNewGame(string name, DateTime startTime, GameMode gameMode, int totalRoundCount, string? hostConnectionId = null)
        {
            var newGame = new Game
            {
                Name = name,
                StartTime = startTime,
                Status = GameStatus.IN_GAME,
                Mode = gameMode,
                HostConnectionId = hostConnectionId,
                GameRounds = new List<GameRound>(),
                Players = new List<DbPlayer>(),
                TotalRoundCount = totalRoundCount,
            };
            await _dbContext.Games.AddAsync(newGame);
            await _dbContext.SaveChangesAsync();
            _votingService.AddVoteSummaryForGame(newGame.Id.ToString());
            _eventManager.RegisterNewGameEventState(newGame.Id.ToString());
            return new GameDto(newGame.Id.ToString(), newGame.Name, newGame.Mode, totalRoundCount);

        }

        public async Task<PlayerDto> AddPlayerToGame(string gameId, string playerName, string connectionId, string? userId)
        {

            var game = await _dbContext.Games.Include(g => g.Players).Where(g => g.Id == Guid.Parse(gameId)).SingleAsync();
            var isGuest = string.IsNullOrEmpty(userId);
            var player = new DbPlayer
            {
                Name = playerName,
                Score = 0,
                ConnectionId = connectionId,
                UserId = userId,
                IsGuest = isGuest,

            };

            game.Players.Add(player);
            await _dbContext.SaveChangesAsync();
            await _gameHubContext.Groups.AddToGroupAsync(connectionId, gameId);
            var players = await this.GetPlayersInGame(gameId);
            await _gameHubContext.Clients.Group(gameId).ReceivePlayersInGame(players);
            return new PlayerDto(player.Id.ToString(), player.Name, player.Score);

        }

        public async Task<List<PlayerDto>> GetPlayersInGame(string gameId)
        {
            var games = await _dbContext.Games.Include(g => g.Players).ToListAsync();
            var game = games.FirstOrDefault(g => g.Id == Guid.Parse(gameId));
            Console.WriteLine(game.Mode);
            Console.WriteLine(game.Status);
            if (game == null)
            {
                throw new ArgumentException("Game does not exists");
            }
            List<PlayerDto> players = new List<PlayerDto>();
            foreach (var player in game.Players)
            {
                players.Add(new PlayerDto(player.Id.ToString(), player.Name, player.Score));

            }
            return players;
        }

        public async Task<GameStatus> StartNewRound(string gameId, string categoryId)
        {
            var games = await _dbContext.Games.Include(g => g.GameRounds).ToListAsync();
            var game = games.FirstOrDefault(g => g.Id == Guid.Parse(gameId));
            if (game == null)
            {
                throw new ArgumentException("Game does not exists");
            }

            var category = await _categoryService.GetCategory(categoryId);
            var questions = await _questionService.CreateQuestions(categoryId, 5);
            var newRound = new GameRound
            {
                StartTime = DateTime.Now,
                GameCategoryId = category.Id,
                Questions = questions,
                Answers = new List<PlayerAnswer>(),

            };
            game.GameRounds.Add(newRound);
            await _dbContext.SaveChangesAsync();


            var status = await this.PlayGameRound(gameId, new GameRoundDto(newRound.Id.ToString(), newRound.GameCategoryId, category.Name));
            if (status == GameStatus.ABORTED)
            {
                await this.EndGame(gameId);
                return status;
            }
            if (game.GameRounds.Count >= game.TotalRoundCount)
            {
                Console.WriteLine(game.GameRounds.Count);
                game.Status = GameStatus.FINISHED;
                await this.EndGame(gameId);
                await _dbContext.SaveChangesAsync();
                await _gameHubContext.Clients.Group(gameId).ReceiveGameEnd(new GameEndDto(GameEndReason.ALL_ROUNDS_COMPLETED));
                return GameStatus.FINISHED;
            }
            int roundCount = game.GameRounds.Count;

            await _gameHubContext.Clients.Group(gameId).ReceiveEndGameRound(roundCount + 1);
            return status;

        }

        public async Task ManageRemoteGamePlay(string gameId)
        {

            Console.WriteLine("Managing remote game play for game: " + gameId);
            GameStatus gameStatus = GameStatus.IN_GAME;
            while (gameStatus == GameStatus.IN_GAME)
            {
                await Task.Delay(1000);
                Console.WriteLine("1000 " + gameId);

                //Console.WriteLine("Managing remote game play for game: " + categoryGroups);
                int voteTime = 10000;
                await _gameHubContext.Clients.Group(gameId).ReceiveVotingStarted(new VotingTime(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), voteTime));

                var tcs = new TaskCompletionSource<object>();

                EventHandler handler = (sender, args) =>
                {
                    tcs.TrySetResult(args);

                };
                _eventManager.SubscribeToEvent(gameId, handler, EventType.EVERYONE_VOTED);


                await Task.WhenAny(tcs.Task, Task.Delay(voteTime));

                _eventManager.UnsubscribeFromEvent(gameId, handler, EventType.EVERYONE_VOTED); //refactor event handling, shall be game level for all gamemode -> everyone voted as well

                var categoryId = _votingService.GetWinningCategory(gameId);
                if (categoryId == null)
                {
                    categoryId = await _categoryService.GetRandomCategoryId();
                }
                var category = await _categoryService.GetCategory(categoryId);
                await _gameHubContext.Clients.Group(gameId).ReceiveVotingEnded(category);
                _votingService.ResetVotesForGame(gameId);
                await Task.Delay(3000);
                gameStatus = await this.StartNewRound(gameId, categoryId);
            }


            //status finsihed either returned or sent by hub - shall be checking aborted game logic, but that should be basically fine
        }

        public async Task DistributePointsBetweenPlayers(string questionId)
        {
            var answers = await _dbContext.PlayerAnswers
                .Where(a => a.QuestionId == questionId)
                .ToListAsync();
            var correctPlayerIds = answers.Where(a => a.IsCorrect).OrderBy(a => a.AnswerTimeInMilliseconds)
                .Select(a => a.PlayerId).ToList();
            for (int i = 0; i < correctPlayerIds.Count; i++)
            {
                var points = Math.Max(100 - (i * 10), 50);
                answers.First(a => a.PlayerId == correctPlayerIds[i]).PointsAwarded = points;
                var player = await _dbContext.Players.FirstOrDefaultAsync(p => p.Id.ToString() == correctPlayerIds[i]);
                if (player != null) { player.Score += points; }
            }
            await _dbContext.SaveChangesAsync();

        }

        public async Task<List<QuestionDto>> GetQuestionsInGameRound(string gameRoundId)
        {
            var gameRound = await _dbContext.GameRounds
                .Include(gr => gr.Questions)
                .FirstOrDefaultAsync(gr => gr.Id == Guid.Parse(gameRoundId));
            var questions = gameRound.Questions;
            List<QuestionDto> questionDtos = new List<QuestionDto>();
            foreach (var question in questions)
            {
                Console.WriteLine(question.CorrectAnswer);
                var song = await _dbContext.Songs.FirstOrDefaultAsync(t => t.Id.ToString() == question.SongId);
                var url = await _deezerApiService.GetPreviewUrlOfTrack(song.DeezerId);
                questionDtos.Add(new QuestionDto(question.Id.ToString(), question.AnswerOptions, url));
            }
            return questionDtos;
        }

        public async Task RegisterAnswer(string gameId, string gameRoundId, string questionId, string answer, string playerId, DateTime time)
        {
            var gameRound = await _dbContext.GameRounds
                .Include(gr => gr.Questions)
                .Include(gr => gr.Answers)
                .FirstOrDefaultAsync(gr => gr.Id == Guid.Parse(gameRoundId));
            if (gameRound == null)
            {
                throw new ArgumentException("Game round not found");
            }
            var question = gameRound.Questions.FirstOrDefault(q => q.Id == Guid.Parse(questionId));
            if (question == null)
            {
                throw new ArgumentException("Question not found");
            }
            var elapsedTime = (time - question.SendTime).TotalMilliseconds;
            var playerAnswer = new PlayerAnswer
            {
                QuestionId = question.Id.ToString(),
                PlayerId = playerId,
                SelectedAnswer = answer,
                IsCorrect = (question.CorrectAnswer == answer),
                AnswerTimeInMilliseconds = (int)(elapsedTime),
            };
            gameRound.Answers.Add(playerAnswer);
            await _dbContext.SaveChangesAsync();

            var playerCount = await this.GetPlayerCountInGame(gameId);
            Console.WriteLine("Players: " + gameRound.Answers.Count + " " + playerCount);
            if (gameRound.Answers.FindAll(gr => gr.QuestionId.ToString() == question.Id.ToString()).Count >= playerCount)
            {
                Console.WriteLine("All players answered: " + gameRound.Answers.Count + playerCount);
                _eventManager.RaiseEventOfGame(gameId, EventType.EVERYONE_ANSWERED);
            }
        }

        public async Task<GameStatus> PlayGameRound(string gameId, GameRoundDto gameRound)
        {

            await _gameHubContext.Clients.Group(gameId).ReceiveNewRoundStarted(gameRound);
            var questions = await this.GetQuestionsInGameRound(gameRound.Id);
            foreach (var question in questions)
            {
                var game = await _dbContext.Games.
                    AsNoTracking().
                    FirstOrDefaultAsync(g => g.Id == Guid.Parse(gameId));
                if (game == null)
                {
                    throw new ArgumentException("Game not found");
                }

                if (game.Status != GameStatus.IN_GAME)
                {
                    Console.WriteLine("GAME STOPPED");
                    return game.Status;
                }
                await _questionService.SetSendDateForQuestion(question.Id, DateTime.Now);


                await _gameHubContext.Clients.Group(gameId).ReceiveNextQuestion(new SendQuestionDto(question, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 15000));
                var tcs = new TaskCompletionSource<object>();

                EventHandler handler = (sender, args) =>
                {
                    tcs.TrySetResult(args);

                };
                _eventManager.SubscribeToEvent(game.Id.ToString(), handler, EventType.EVERYONE_ANSWERED);


                await Task.WhenAny(tcs.Task, Task.Delay(15000));

                _eventManager.UnsubscribeFromEvent(game.Id.ToString(), handler, EventType.EVERYONE_ANSWERED);

                var questionEntity = await _dbContext.Questions.FirstOrDefaultAsync(q => q.Id == Guid.Parse(question.Id));
                if (questionEntity == null)
                    throw new ArgumentException("Question not found");
                var correctAnswer = questionEntity.CorrectAnswer;
                await _gameHubContext.Clients.Group(gameId).ReceiveEndAnswerTime(correctAnswer);
                await this.DistributePointsBetweenPlayers(question.Id);
                var players = await this.GetPlayersInGame(gameId);
                await _gameHubContext.Clients.Group(gameId).ReceivePlayersInGame(players);
                await Task.Delay(5000);
            }
            return GameStatus.IN_GAME;

        }

        public async Task<GameDto> GetGame(string gameId)
        {
            var game = await _dbContext.Games.FirstOrDefaultAsync(g => g.Id == Guid.Parse(gameId));
            if (game == null)
                throw new ArgumentException("Game is not found");
            return new GameDto(game.Id.ToString(), game.Name, game.Mode, game.TotalRoundCount);
        }

        public async Task ManageLeaveGame(string connectionId)
        {
            var player = await _dbContext.Players.FirstOrDefaultAsync(p => p.ConnectionId == connectionId);
            if (player != null)
            {
                var game = await _dbContext.Games.Include(g => g.Players)
                    .FirstOrDefaultAsync(g => g.Players.Any(p => p.Id == player.Id));
                if (player.IsGuest == false && player.UserId != null)
                {
                    await _userService.CalculateSumScoresForUser(player.UserId);
                }
                if (game != null)
                {

                    game.Players.Remove(player);
                    await _dbContext.SaveChangesAsync();
                    if (game.Players.Count <= 1)
                    {
                        game.Status = GameStatus.ABORTED;
                        await _dbContext.SaveChangesAsync();
                        await _gameHubContext.Clients.Group(game.Id.ToString()).ReceiveGameEnd(new GameEndDto(GameEndReason.TOO_MANY_PLAYERS_LEFT_GAME));
                    }
                    else
                    {
                        var players = await this.GetPlayersInGame(game.Id.ToString());
                        await _gameHubContext.Clients.Group(game.Id.ToString()).ReceivePlayersInGame(players);
                    }


                }
            }
            else
            {
                var game = await _dbContext.Games.FirstOrDefaultAsync(g => g.HostConnectionId == connectionId);
                if (game != null)
                {
                    if (game.Mode == GameMode.LOCAL)
                    {

                        game.Status = GameStatus.ABORTED;
                        await _dbContext.SaveChangesAsync();
                        await _gameHubContext.Clients.Group(game.Id.ToString()).ReceiveGameEnd(new GameEndDto(GameEndReason.HOST_LEFT_GAME));
                    }

                }
            }
        }

        private async Task EndGame(string gameId)
        {
            var game = await _dbContext.Games.Include(g => g.Players).FirstOrDefaultAsync(g => g.Id == Guid.Parse(gameId));
            if (game == null)
                throw new ArgumentException("Game is not found");
            foreach (var player in game.Players)
            {
                if (!player.IsGuest && player.UserId != null)
                {
                    await _userService.CalculateSumScoresForUser(player.UserId);
                }
            }
            game.EndTime = DateTime.Now;
            _votingService.RemoveVoteSummaryForGame(gameId);
            //Unsubscribe from game events
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetPlayerCountInGame(string gameId)
        {
            var game = await _dbContext.Games.Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == Guid.Parse(gameId));
            if (game == null)
                throw new ArgumentException("Game is not found");
            return game.Players.Count;

        }

    }
}
