using GuessifyBackend.DTO.LobbyModel;
using GuessifyBackend.Hubs;
using GuessifyBackend.Models;
using GuessifyBackend.Models.Enum;
using GuessifyBackend.Service.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace GuessifyBackend.Service.Implementations
{
    public class LobbyService : ILobbyService
    {
        private List<Lobby> _lobbies;

        private IHubContext<LobbyHub, IlobbyClient> _lobbyHubContext;

        private IServiceProvider _serviceScopeFactory;

        private ILogger<LobbyService> _logger;

        public LobbyService(IHubContext<LobbyHub, IlobbyClient> lobbyhub, IServiceProvider serviceFactory, ILogger<LobbyService> logger)
        {
            _lobbyHubContext = lobbyhub;
            _lobbies = new List<Lobby>();
            _serviceScopeFactory = serviceFactory;
            _logger = logger;
        }

        public async Task<CreateLobbyDto> CreateLobby(string name, int capacity, string connectionId, GameMode gameMode, int totalRoundCount, string? userId, string? userName)
        {
            string? connectionCode = null;
            int currentPlayerCount = 0;
            if (gameMode == GameMode.REMOTE)
            {
                currentPlayerCount = 1;
                if (_lobbies.Any(l => l.Name == name && l.GameMode == GameMode.REMOTE))
                {
                    return new CreateLobbyDto(LobbyCreateStatus.LOBBY_ALREADY_EXISTS_WITH_NAME, null);
                }
            }
            else if (gameMode == GameMode.LOCAL)
            {
                char[] _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
                Random rand = new();
                List<string> existingCodes = new List<string>();
                foreach (var l in _lobbies)
                {
                    if (l.ConnectionCode != null)
                    {
                        existingCodes.Add(l.ConnectionCode);
                    }

                }
                do
                {
                    connectionCode = "#" + new string(rand.GetItems(_chars, 6));
                    _logger.LogInformation("Generated connection code is: " + connectionCode);
                } while (existingCodes.Contains(connectionCode));


            }

            Lobby lobby = new Lobby
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Players = new List<Player>(),
                Status = LobbyStatus.OPEN,
                Capacity = capacity,
                HostConnectionId = connectionId,
                CurrentPlayerCount = currentPlayerCount,
                GameMode = gameMode,
                ConnectionCode = connectionCode,
                TotalRoundCount = totalRoundCount,
                HostUserId = userId,
                HostUserName = userName
            };
            _lobbies.Add(lobby);
            _logger.LogInformation("Lobbyí created with id: " + lobby.Id);
            var lobbies = GetLobbies();
            await _lobbyHubContext.Groups.AddToGroupAsync(connectionId, lobby.Id);
            await _lobbyHubContext.Clients.All.ReceiveLobbies(lobbies);
            return new CreateLobbyDto(LobbyCreateStatus.CREATED, new LobbyDto(lobby.Id, lobby.Name, lobby.CurrentPlayerCount, lobby.Capacity, lobby.Status, lobby.GameMode, lobby.TotalRoundCount, lobby.ConnectionCode));
        }

        public async Task<JoinStatusDto> JoinLobbyWithCode(string code, string playerName, string connectionId, string? userId)
        {
            var lobby = _lobbies.FirstOrDefault(l => l.ConnectionCode == code);
            if (lobby == null)
                return new JoinStatusDto(null, JoinStatus.LOBBY_NOT_FOUND, null);
            else if (lobby.CurrentPlayerCount >= lobby.Capacity)
                return new JoinStatusDto(null, JoinStatus.LOBBY_FULL, null);
            else if (lobby.Players.Any(p => p.Name == playerName))
                return new JoinStatusDto(null, JoinStatus.USERNAME_TAKEN, null);
            else
            {
                if (userId != null)
                {
                    var isPlayerInLobby = lobby.Players.Any(p => p.UserId == userId);
                    if (isPlayerInLobby)
                    {
                        return new JoinStatusDto(null, JoinStatus.USER_ALREADY_IN_LOBBY, null);
                    }
                    else if (lobby.HostUserId == userId)
                    {
                        return new JoinStatusDto(null, JoinStatus.USER_HOST_GAME, null);
                    }
                }

                var newPlayer = new Player
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = playerName,
                    Score = 0,
                    ConnectionId = connectionId,
                    UserId = userId,
                };
                lobby.Players.Add(newPlayer);
                lobby.CurrentPlayerCount += 1;

                var lobbies = GetLobbies();

                var players = GetPlayersInLobby(lobby.Id);
                await _lobbyHubContext.Groups.AddToGroupAsync(connectionId, lobby.Id);
                await _lobbyHubContext.Clients.Group(lobby.Id).ReceivePlayersInLobby(players);
                await _lobbyHubContext.Clients.All.ReceiveLobbies(lobbies);
                return new JoinStatusDto(new PlayerDto(newPlayer.Id, newPlayer.Name, newPlayer.Score), JoinStatus.SUCCESS, lobby.Id);
            }

        }

        public async Task<JoinStatusDto> JoinLobby(string lobbyId, string playerName, string connectionId, string? userId)
        {
            var lobby = _lobbies.Single(lobby => lobby.Id == lobbyId);
            if (userId != null)
            {
                var isPlayerInLobby = lobby.Players.Any(p => p.Id == userId);
                if (isPlayerInLobby)
                {
                    return new JoinStatusDto(null, JoinStatus.USER_ALREADY_IN_LOBBY, null);
                }
                else if (lobby.HostUserId == userId)
                {
                    return new JoinStatusDto(null, JoinStatus.USER_HOST_GAME, null);
                }
            }

            var newPlayer = new Player
            {
                Id = Guid.NewGuid().ToString(),
                Name = playerName,
                Score = 0,
                ConnectionId = connectionId,
                UserId = userId
            };
            if (lobby.Capacity <= lobby.CurrentPlayerCount)
                return new JoinStatusDto(null, JoinStatus.LOBBY_FULL, null);
            if (lobby.Players.Any(p => p.Name == playerName) || lobby.HostUserName == playerName)
                return new JoinStatusDto(null, JoinStatus.USERNAME_TAKEN, null);
            lobby.Players.Add(newPlayer);
            lobby.CurrentPlayerCount += 1;
            var lobbies = GetLobbies();
            var players = GetPlayersInLobby(lobbyId);

            await _lobbyHubContext.Groups.AddToGroupAsync(connectionId, lobbyId);
            await _lobbyHubContext.Clients.Group(lobby.Id).ReceivePlayersInLobby(players);
            await _lobbyHubContext.Clients.All.ReceiveLobbies(lobbies);
            return new JoinStatusDto(new PlayerDto(newPlayer.Id, newPlayer.Name, newPlayer.Score), JoinStatus.SUCCESS, null);

        }

        public async Task AbandonLobby(string lobbyId, string gameId)
        {
            using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
            var _gameServiceInstance = serviceScope.ServiceProvider.GetRequiredService<IGameService>();
            var game = await _gameServiceInstance.GetGame(gameId);
            var lobby = _lobbies.Single(lobby => lobby.Id == lobbyId);
            await _lobbyHubContext.Clients.Groups(lobbyId).RequestConnectionToGame(game);
            await RemoveLobby(lobbyId);

            await _lobbyHubContext.Clients.All.ReceiveLobbies(this.GetLobbies());
        }
        public List<LobbyDto> GetLobbies()
        {
            List<LobbyDto> lobbyDtos = new List<LobbyDto>();
            foreach (var lobby in _lobbies)
            {


                lobbyDtos.Add(new LobbyDto(
                    lobby.Id,
                    lobby.Name,
                    lobby.CurrentPlayerCount,
                    lobby.Capacity,
                    lobby.Status,
                    lobby.GameMode,
                    lobby.TotalRoundCount,
                    null


                ));
            }
            return lobbyDtos;
        }


        public async Task HandleLobbyLeaving(string connectionId)
        {
            _logger.LogInformation("Client disconnected: " + connectionId);
            var lobby = _lobbies.Find(lobby => lobby.HostConnectionId == connectionId);
            if (lobby != null)
            {
                await _lobbyHubContext.Clients.Group(lobby.Id).ReceiveHostDisconnectedFromLobby();
                await RemoveLobby(lobby.Id);
                var remainingLobbiesDto = GetLobbies();
                await _lobbyHubContext.Clients.All.ReceiveLobbies(remainingLobbiesDto);
                return;
            }
            foreach (var l in _lobbies)
            {
                var player = l.Players.Find(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    l.Players.Remove(player);
                    var lobbiesDto = GetLobbies();
                    var players = GetPlayersInLobby(l.Id);
                    await _lobbyHubContext.Groups.RemoveFromGroupAsync(connectionId, l.Id);
                    await _lobbyHubContext.Clients.All.ReceiveLobbies(lobbiesDto);
                    await _lobbyHubContext.Clients.Group(l.Id).ReceivePlayersInLobby(players);
                    return;
                }

            }

        }

        private async Task RemoveLobby(string lobbyId)
        {
            await RemoveAllPlayersFromLobbyGroup(lobbyId);
            _lobbies.RemoveAll(p => p.Id == lobbyId);

        }

        public List<PlayerDto> GetPlayersInLobby(string lobbyId)
        {
            var lobby = _lobbies.Find(lobby => lobby.Id == lobbyId);
            if (lobby == null)
                return new List<PlayerDto>();
            List<PlayerDto> playerDtos = new List<PlayerDto>();
            foreach (var player in lobby.Players)
            {
                playerDtos.Add(new PlayerDto(player.Id, player.Name, player.Score));
            }
            return playerDtos;
        }

        public StartGameStatus CheckWhetherGameCanBeStarted(string lobbyId, string? hostPlayerName)
        {
            var lobby = _lobbies.Single(lobby => lobby.Id == lobbyId);
            if (lobby.CurrentPlayerCount < 2)
            {
                return StartGameStatus.NOT_ENOUGH_PLAYERS;
            }
            if (hostPlayerName != null && lobby.Players.Any(p => p.Name == hostPlayerName))
            {
                return StartGameStatus.HOST_PLAYER_NAME_TAKEN;
            }
            return StartGameStatus.GAME_CAN_BE_STARTED;
        }

        private async Task RemoveAllPlayersFromLobbyGroup(string lobbyId)
        {
            var lobby = _lobbies.Single(lobby => lobby.Id == lobbyId);
            foreach (var player in lobby.Players)
            {
                await _lobbyHubContext.Groups.RemoveFromGroupAsync(player.ConnectionId, lobbyId);
            }
            await _lobbyHubContext.Groups.RemoveFromGroupAsync(lobby.HostConnectionId, lobbyId);
        }
    }
}
