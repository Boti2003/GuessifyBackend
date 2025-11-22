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
        public async Task<LobbyDto> CreateLobby(string lobbyName, int capacity, GameMode gameMode, int totalRoundCount)
        {
            var isGuest = Context.User?.FindFirst(ClaimTypes.Anonymous)?.Value != null;
            string? userId = null;
            string? userName = null;
            if (!isGuest)
            {
                userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
                userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            var lobby = await _lobbyService.CreateLobby(lobbyName, capacity, Context.ConnectionId, gameMode, totalRoundCount, userId, userName);

            return lobby;
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

        public async Task RefreshPlayersInLobby(string lobbyId)
        {
            var players = _lobbyService.GetPlayersInLobby(lobbyId);
            await Clients.Group(lobbyId).ReceivePlayersInLobby(players);
        }

        public async Task StartGame(string lobbyId, string gameId)
        {
            var game = await _gameService.GetGame(gameId);
            await Clients.Groups(lobbyId).RequestConnectionToGame(game);
            _lobbyService.RemoveLobby(lobbyId);
            await Clients.All.ReceiveLobbies(_lobbyService.GetLobbies());
        }

        public async Task<JoinStatusDto> JoinLobbyAsGuestWithCode(string connectionCode, string? playerName)
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
