using GuessifyBackend.DTO.LobbyModel;
using GuessifyBackend.Models.Enum;
using GuessifyBackend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GuessifyBackend.Hubs
{
    [Authorize]
    public class LobbyHub : Hub<IlobbyClient>
    {
        private readonly ILobbyService _lobbyService;
        private readonly IGameService _gameService;
        public LobbyHub(ILobbyService lobbyService, IGameService gameService)
        {
            _lobbyService = lobbyService;
            _gameService = gameService;
        }
        public async Task<CreateLobbyDto> CreateLobby(string lobbyName, int capacity, GameMode gameMode, int totalRoundCount)
        {
            //should be checked if another remote lobby exists with the name, or (host is a user and already plays or host somewhere else) - update thesis
            var isGuest = Context.User?.FindFirst(ClaimTypes.Anonymous)?.Value != null;
            string? userId = null;
            string? userName = null;
            if (!isGuest)
            {
                userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
                userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            var lobbyCreateStatus = await _lobbyService.CreateLobby(lobbyName, capacity, Context.ConnectionId, gameMode, totalRoundCount, userId, userName);

            return lobbyCreateStatus;
        }

        public List<LobbyDto> GetLobbies()
        {
            var lobbies = _lobbyService.GetLobbies();
            return lobbies;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _lobbyService.HandleLobbyLeaving(Context.ConnectionId);
        }

        public async Task AbandonLobby(string lobbyId, string gameId)
        {

            await _lobbyService.AbandonLobby(lobbyId, gameId);

        }

        public StartGameStatus CheckWhetherGameCanBeStarted(string lobbyId, string? hostPlayerName)
        {

            var isGuest = Context.User?.FindFirst(ClaimTypes.Anonymous)?.Value != null;
            string? name;
            if (isGuest)
            {
                name = hostPlayerName;
            }
            else
            {
                var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
                name = userName!;
            }

            return _lobbyService.CheckWhetherGameCanBeStarted(lobbyId, name);
        }

        public async Task<JoinStatusDto> JoinLobbyWithCode(string connectionCode, string? playerName)
        {
            var isGuest = Context.User?.FindFirst(ClaimTypes.Anonymous)?.Value != null;
            string name;
            string? userId = null;
            if (isGuest)
            {
                name = playerName!;
            }
            else
            {
                var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
                name = userName!;
                userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            var joinStatus = await _lobbyService.JoinLobbyWithCode(connectionCode, name, Context.ConnectionId, userId);
            return joinStatus;
        }

        public async Task<JoinStatusDto> JoinLobby(string lobbyId, string? playerName)
        {
            var isGuest = Context.User?.FindFirst(ClaimTypes.Anonymous)?.Value != null;
            string name;
            string? userId = null;
            if (isGuest)
            {
                name = playerName!;
            }
            else
            {
                var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
                name = userName!;
                userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            var joinStatusDto = await _lobbyService.JoinLobby(lobbyId, name, Context.ConnectionId, userId);
            return joinStatusDto;

        }
    }
}
