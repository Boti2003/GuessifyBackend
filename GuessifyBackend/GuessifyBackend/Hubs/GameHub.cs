using GuessifyBackend.DTO.GameModel;
using GuessifyBackend.DTO.LobbyModel;
using GuessifyBackend.Models.Enum;
using GuessifyBackend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GuessifyBackend.Hubs
{
    [Authorize]
    public class GameHub : Hub<IGameClient>
    {
        private readonly CategoryService _categoryService;
        private readonly GameService _gameService;
        private readonly QuestionService _questionService;
        private readonly VotingService _votingService;
        private readonly IServiceScopeFactory _serviceFactory;


        public GameHub(CategoryService categoryService, GameService gameService, QuestionService questionService, VotingService votingService, IServiceScopeFactory serviceFactory)
        {
            _categoryService = categoryService;
            _gameService = gameService;
            _questionService = questionService;
            _votingService = votingService;
            _serviceFactory = serviceFactory;
        }
        public async Task<List<CategoryGroupDto>> GetCategoryGroups()
        {
            return await _categoryService.GetCategoryGroups();
        }

        public async Task<GameDto> StartGame(string gameName, GameMode gameMode, int totalRoundCount)
        {

            var startTime = DateTime.Now;
            GameDto game;
            if (gameMode == GameMode.LOCAL)
            {
                game = await _gameService.StartNewGame(gameName, startTime, gameMode, totalRoundCount, Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
            }
            else
            {
                game = await _gameService.StartNewGame(gameName, startTime, gameMode, totalRoundCount);
            }
            return game;
        }

        public async Task<PlayerDto> JoinGame(string? playerName, string gameId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = userId == null ? playerName! : Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var player = await _gameService.AddPlayerToGame(gameId, name!, Context.ConnectionId, userId);

            return player;
        }

        public async Task SubmitAnswer(string gameId, string gameRoundId, string questionId, string answer, string playerId)
        {
            await _gameService.RegisterAnswer(gameId, gameRoundId, questionId, answer, playerId, DateTime.Now);
        }

        public Task StartNewRound(string gameId, string categoryId)
        {
            Task.Run(async () =>
            {

                using (var scope = _serviceFactory.CreateScope())
                {
                    var _scopedGameService = scope.ServiceProvider.GetRequiredService<GameService>();
                    await _scopedGameService.StartNewRound(gameId, categoryId);
                }

            });

            return Task.CompletedTask;


        }


        public async Task RegisterVote(string gameId, string categoryId)
        {
            var playerCount = await _gameService.GetPlayerCountInGame(gameId);
            _votingService.RegisterVote(gameId, categoryId, playerCount);
        }



        public Task ManageRemoteGame(string gameId)
        {
            Task.Run(async () =>
            {

                using (var scope = _serviceFactory.CreateScope())
                {
                    var _scopedGameService = scope.ServiceProvider.GetRequiredService<GameService>();
                    await _scopedGameService.ManageRemoteGamePlay(gameId);
                }

            });
            return Task.CompletedTask;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _gameService.ManageLeaveGame(Context.ConnectionId);
        }


    }
}
