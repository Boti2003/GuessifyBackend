using GuessifyBackend.DTO.LobbyModel;
using GuessifyBackend.Models.Enum;
using GuessifyBackend.Service;
using Microsoft.AspNetCore.SignalR;

namespace GuessifyBackend.Hubs
{
    public class LobbyHub : Hub<IlobbyClient>
    {
        private readonly LobbyService _lobbyService;
        private readonly GameService _gameService;
        public LobbyHub(LobbyService lobbyService, GameService gameService)
        {
            _lobbyService = lobbyService;
            _gameService = gameService;
        }
        public async Task<LobbyDto> CreateLobby(string lobbyName, int capacity, GameMode gameMode, int totalRoundCount)
        {
            var lobby = _lobbyService.CreateLobby(lobbyName, capacity, Context.ConnectionId, gameMode, totalRoundCount);
            Console.WriteLine(lobby.Id);
            var lobbies = _lobbyService.GetLobbies();
            await Groups.AddToGroupAsync(Context.ConnectionId, lobby.Id);
            await Clients.All.ReceiveLobbies(lobbies);
            return lobby;
        }

        public async Task<List<LobbyDto>> GetLobbies()
        {
            var lobbies = _lobbyService.GetLobbies();
            return lobbies;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("Client disconnected: " + Context.ConnectionId);
            await _lobbyService.HandleLobbyLeaving(Context.ConnectionId);
        }

        public async Task RefreshPlayersInLobby(string lobbyId)
        {
            var players = await _lobbyService.GetPlayersInLobby(lobbyId);
            await Clients.Group(lobbyId).ReceivePlayersInLobby(players);
        }

        public async Task StartGame(string lobbyId, string gameId)
        {
            var game = await _gameService.GetGame(gameId);
            await Clients.Groups(lobbyId).RequestConnectionToGame(game);
            _lobbyService.RemoveLobby(lobbyId);
            await Clients.All.ReceiveLobbies(_lobbyService.GetLobbies());
        }

        public async Task<JoinStatusDto> JoinLobbyAsGuestWithCode(string connectionCode, string playerName)
        {
            var joinResult = _lobbyService.JoinLobbyWithCode(connectionCode, playerName, Context.ConnectionId);
            if (joinResult.JoinStatus == JoinStatus.SUCCESS)
            {
                var lobbies = _lobbyService.GetLobbies();
                if (joinResult.lobbyId != null)
                {
                    var players = await _lobbyService.GetPlayersInLobby(joinResult.lobbyId);
                    await Groups.AddToGroupAsync(Context.ConnectionId, joinResult.lobbyId);
                    await Clients.All.ReceiveLobbies(lobbies);
                }
                return joinResult;
            }
            else
            {
                return joinResult;
            }
        }

        public async Task<JoinStatusDto> JoinLobbyAsGuest(string lobbyId, string playerName)
        {
            try
            {
                var connectionId = Context.ConnectionId;
                var playerDto = _lobbyService.JoinLobbyAsGuest(lobbyId, playerName, connectionId);
                var lobbies = _lobbyService.GetLobbies();
                var players = await _lobbyService.GetPlayersInLobby(lobbyId);
                await Groups.AddToGroupAsync(connectionId, lobbyId);
                await Clients.All.ReceiveLobbies(lobbies);
                return new JoinStatusDto(playerDto, JoinStatus.SUCCESS, null);

            }
            catch (Exception)
            {
                return new JoinStatusDto(null, JoinStatus.LOBBY_FULL, null);

            }
        }
    }
}
