using GuessifyBackend.DTO.GameModel;
using GuessifyBackend.DTO.LobbyModel;
using GuessifyBackend.Models.Enum;
using GuessifyBackend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GuessifyBackend.Hubs
{
    [Authorize]
    public class GameHub : Hub<IGameClient>
    {
        private readonly ICategoryService _categoryService;
        private readonly IGameService _gameService;
        private readonly IVotingService _votingService;
        private readonly IServiceScopeFactory _serviceFactory;


        public GameHub(ICategoryService categoryService, IGameService gameService, IVotingService votingService, IServiceScopeFactory serviceFactory)
        {
            _categoryService = categoryService;
            _gameService = gameService;
            _votingService = votingService;
            _serviceFactory = serviceFactory;
        }
        public async Task<List<CategoryGroupDto>> GetCategoryGroups()
        {
            return await _categoryService.GetCategoryGroups();
        }

        public async Task<GameDto> StartGame(string gameName, GameMode gameMode, int totalRoundCount)
        {
            var hostConnectionId = gameMode == GameMode.LOCAL ? Context.ConnectionId : null;
            return await _gameService.StartNewGame(gameName, DateTime.Now, gameMode, totalRoundCount, hostConnectionId); ;
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
                try
                {
                    using (var scope = _serviceFactory.CreateScope())
                    {
                        var _scopedGameService = scope.ServiceProvider.GetRequiredService<IGameService>();
                        await _scopedGameService.StartNewRound(gameId, categoryId);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in StartNewRound: " + ex.Message);
                    Console.WriteLine(ex.InnerException);
                    throw ex;
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
                try
                {

                    using (var scope = _serviceFactory.CreateScope())
                    {
                        var _scopedGameService = scope.ServiceProvider.GetRequiredService<IGameService>();
                        await _scopedGameService.ManageRemoteGamePlay(gameId);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in ManageRemoteGame: " + ex.Message);
                    throw ex;
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
