using GuessifyBackend.DTO.GameModel;
using GuessifyBackend.DTO.LobbyModel;
using GuessifyBackend.Entities;
using GuessifyBackend.Hubs;
using GuessifyBackend.Models.Enum;
using GuessifyBackend.Service.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GuessifyBackend.Service.Implementations
{
    public class GameService : IGameService
    {
        private readonly GameDbContext _dbContext;

        private readonly ICategoryService _categoryService;

        private readonly IQuestionService _questionService;

        private readonly IDeezerApiService _deezerApiService;

        private readonly IGameEventManager _eventManager;

        private readonly IVotingService _votingService;

        private readonly IUserService _userService;

        private readonly IConfiguration _configuration;

        private readonly ILogger<GameService> _logger;

        private IHubContext<GameHub, IGameClient> _gameHubContext
        {
            get;
        }


        public GameService(IConfiguration configuration, GameDbContext dbContext, ICategoryService categoryService, IQuestionService questionService, IDeezerApiService deezerApiService, IHubContext<GameHub, IGameClient> hubContext, IGameEventManager gameEventManager, IVotingService votingService, IUserService userService, ILogger<GameService> logger)
        {
            _categoryService = categoryService;
            _dbContext = dbContext;
            _questionService = questionService;
            _deezerApiService = deezerApiService;
            _gameHubContext = hubContext;
            _eventManager = gameEventManager;
            _votingService = votingService;
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
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
            if (gameMode == GameMode.REMOTE)
            {
                _votingService.AddVoteSummaryForGame(newGame.Id.ToString());
            }
            else if (gameMode == GameMode.LOCAL)
            {
                if (hostConnectionId == null)
                {
                    throw new ArgumentException("Host connection ID cannot be null for local games");
                }
                await _gameHubContext.Groups.AddToGroupAsync(hostConnectionId, newGame.Id.ToString());
            }
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
            var players = await GetPlayersInGame(gameId);
            await _gameHubContext.Clients.Group(gameId).ReceivePlayersInGame(players);
            return new PlayerDto(player.Id.ToString(), player.Name, player.Score);

        }

        public async Task<List<PlayerDto>> GetPlayersInGame(string gameId)
        {
            var game = await _dbContext.Games.Include(g => g.Players).SingleAsync(g => g.Id == Guid.Parse(gameId));
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
            try
            {
                var game = await _dbContext.Games.Include(g => g.GameRounds).FirstOrDefaultAsync(g => g.Id == Guid.Parse(gameId));
                if (game == null)
                {
                    throw new ArgumentException("Game does not exists");
                }

                var category = await _categoryService.GetCategory(categoryId);
                var questions = await _questionService.CreateQuestions(categoryId, 5);
                var newRound = new GameRound
                {
                    StartTime = DateTime.Now,
                    GameCategoryId = new Guid(categoryId),
                    Questions = questions,
                    Answers = new List<PlayerAnswer>(),

                };
                game.GameRounds.Add(newRound);
                await _dbContext.SaveChangesAsync();


                var status = await PlayGameRound(gameId, new GameRoundDto(newRound.Id.ToString(), newRound.GameCategoryId.ToString(), category.Name));
                if (status == GameStatus.ABORTED)
                {
                    await EndGame(gameId, status);
                    return status;
                }
                if (game.GameRounds.Count >= game.TotalRoundCount)
                {

                    await _gameHubContext.Clients.Group(gameId).ReceiveGameEnd(new GameEndDto(GameEndReason.ALL_ROUNDS_COMPLETED));
                    await EndGame(gameId, status);
                    return GameStatus.FINISHED;
                }

                int roundCount = game.GameRounds.Count;

                await _gameHubContext.Clients.Group(gameId).ReceiveEndGameRound(roundCount + 1);
                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in StartNewRound: " + ex.Message);
                _logger.LogError(ex.InnerException?.Message);
                await _gameHubContext.Clients.Group(gameId).ExceptionThrown(ex.Message);
                return GameStatus.ABORTED;
            }

        }

        public async Task ManageRemoteGamePlay(string gameId)
        {
            try
            {
                GameStatus gameStatus = GameStatus.IN_GAME;
                while (gameStatus == GameStatus.IN_GAME)
                {
                    await Task.Delay(1000);

                    int voteTime = _configuration.GetValue<int>("GameConstants:VotingTimeInMilisec");
                    await _gameHubContext.Clients.Group(gameId).ReceiveVotingStarted(new VotingTime(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), voteTime));

                    var tcs = new TaskCompletionSource<object>();

                    EventHandler handler = (sender, args) =>
                    {
                        tcs.TrySetResult(args);

                    };
                    _eventManager.SubscribeToEvent(gameId, handler, EventType.EVERYONE_VOTED);


                    await Task.WhenAny(tcs.Task, Task.Delay(voteTime));

                    _eventManager.UnsubscribeFromEvent(gameId, handler, EventType.EVERYONE_VOTED);

                    var categoryId = _votingService.GetWinningCategory(gameId);
                    if (categoryId == null)
                    {
                        categoryId = await _categoryService.GetRandomCategoryId();
                    }
                    var category = await _categoryService.GetCategory(categoryId);
                    await _gameHubContext.Clients.Group(gameId).ReceiveVotingEnded(category);
                    _votingService.ResetVotesForGame(gameId);
                    var waitingTime = _configuration.GetValue<int>("GameConstants:WaitingTimeInMilisec");
                    await Task.Delay(waitingTime);
                    gameStatus = await StartNewRound(gameId, categoryId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in ManageRemoteGamePlay: " + ex.Message);
                _logger.LogError(ex.InnerException?.Message);
                await _gameHubContext.Clients.Group(gameId).ExceptionThrown(ex.Message);
            }



        }

        public async Task DistributePointsBetweenPlayers(string questionId)
        {
            var answers = await _dbContext.PlayerAnswers
                .Include(a => a.Player)
                .Include(a => a.Question)
                .Where(a => a.Question.Id == Guid.Parse(questionId))
                .ToListAsync();
            var correctPlayerIds = answers.Where(a => a.IsCorrect).OrderBy(a => a.AnswerTimeInMilliseconds)
                .Select(a => a.Player.Id).ToList();
            for (int i = 0; i < correctPlayerIds.Count; i++)
            {
                var points = Math.Max(100 - i * 10, 50);
                var answer = answers.Single(a => a.Player.Id == correctPlayerIds[i]);
                answer.PointsAwarded = points;
                var player = answer.Player;
                player.Score += points;
            }
            await _dbContext.SaveChangesAsync();

        }

        public async Task RegisterAnswer(string gameId, string gameRoundId, string questionId, string answer, string playerId, DateTime time)
        {
            var gameRound = await _dbContext.GameRounds
                .Include(gr => gr.Questions)
                .Include(gr => gr.Answers).ThenInclude(gra => gra.Question)
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
            var answerTime = _configuration.GetValue<int>("GameConstants:AnswerTimeInMilisec");
            if (elapsedTime > answerTime)
            {
                throw new ArgumentException("Answer time exceeded");
            }

            var playerAnswer = new PlayerAnswer
            {
                SelectedAnswer = answer,
                IsCorrect = question.CorrectAnswer == answer,
                AnswerTimeInMilliseconds = (int)elapsedTime,
                PlayerId = new Guid(playerId),
                Question = question,
            };
            gameRound.Answers.Add(playerAnswer);
            await _dbContext.SaveChangesAsync();

            var playerCount = await GetPlayerCountInGame(gameId);

            if (gameRound.Answers.Count(gra => gra.QuestionId == question.Id) >= playerCount)
            {
                _logger.LogInformation("All players answered to the question. Answer count: " + gameRound.Answers.Count + ". Player count: " + playerCount);
                _eventManager.RaiseEventOfGame(gameId, EventType.EVERYONE_ANSWERED);
            }
        }

        public async Task<GameStatus> PlayGameRound(string gameId, GameRoundDto gameRound)
        {

            await _gameHubContext.Clients.Group(gameId).ReceiveNewRoundStarted(gameRound);
            var gameDbRound = await _dbContext.GameRounds
                .Include(gr => gr.Questions).ThenInclude(q => q.Song)
                .SingleAsync(gr => gr.Id == Guid.Parse(gameRound.Id));
            var questionDtos = await _questionService.FormatQuestionsForGameRound(gameDbRound.Questions);
            foreach (var question in questionDtos)
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
                    _logger.LogInformation("GAME STOPPED");
                    return game.Status;
                }
                await _questionService.SetSendDateForQuestion(question.Id, DateTime.Now);

                var answerTime = _configuration.GetValue<int>("GameConstants:AnswerTimeInMilisec");
                await _gameHubContext.Clients.Group(gameId).ReceiveNextQuestion(new SendQuestionDto(question, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), answerTime));


                var tcs = new TaskCompletionSource<object>();

                EventHandler handler = (sender, args) =>
                {
                    tcs.TrySetResult(args);

                };
                _eventManager.SubscribeToEvent(game.Id.ToString(), handler, EventType.EVERYONE_ANSWERED);


                await Task.WhenAny(tcs.Task, Task.Delay(answerTime));

                await _questionService.SetEndDateForQuestion(question.Id, DateTime.Now);

                _eventManager.UnsubscribeFromEvent(game.Id.ToString(), handler, EventType.EVERYONE_ANSWERED);

                var questionEntity = await _dbContext.Questions.FirstOrDefaultAsync(q => q.Id == Guid.Parse(question.Id));
                if (questionEntity == null)
                    throw new ArgumentException("Question not found");
                var correctAnswer = questionEntity.CorrectAnswer;
                await _gameHubContext.Clients.Group(gameId).ReceiveEndAnswerTime(correctAnswer);
                await DistributePointsBetweenPlayers(question.Id);
                var players = await GetPlayersInGame(gameId);
                await _gameHubContext.Clients.Group(gameId).ReceivePlayersInGame(players);
                var waitingTime = _configuration.GetValue<int>("GameConstants:WaitingTimeInMilisec");
                await Task.Delay(waitingTime);
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
                        var players = await GetPlayersInGame(game.Id.ToString());
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

        private async Task EndGame(string gameId, GameStatus gameStatus)
        {
            var game = await _dbContext.Games.Include(g => g.Players).FirstOrDefaultAsync(g => g.Id == Guid.Parse(gameId));
            if (game == null)
                throw new ArgumentException("Game is not found");
            foreach (var player in game.Players)
            {
                await _gameHubContext.Groups.RemoveFromGroupAsync(player.ConnectionId, gameId);
                if (!player.IsGuest && player.UserId != null)
                {
                    await _userService.CalculateSumScoresForUser(player.UserId);
                }
            }
            if (game.Mode == GameMode.LOCAL && game.HostConnectionId != null)
            {
                await _gameHubContext.Groups.RemoveFromGroupAsync(game.HostConnectionId, gameId);
            }
            if (gameStatus == GameStatus.FINISHED)
            {
                game.Status = gameStatus;
            }

            game.EndTime = DateTime.Now;
            _votingService.RemoveVoteSummaryForGame(gameId);
            _eventManager.RemoveGameEventState(gameId);
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
