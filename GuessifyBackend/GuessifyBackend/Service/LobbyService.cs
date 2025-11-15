using GuessifyBackend.DTO.LobbyModel;
using GuessifyBackend.Hubs;
using GuessifyBackend.Models;
using GuessifyBackend.Models.Enum;
using Microsoft.AspNetCore.SignalR;

namespace GuessifyBackend.Service
{
    public class LobbyService
    {
        private List<Lobby> _lobbies;

        private IHubContext<LobbyHub, IlobbyClient> _lobbyHubContext;

        public LobbyService(IHubContext<LobbyHub, IlobbyClient> lobbyhub)
        {
            _lobbyHubContext = lobbyhub;
            _lobbies = new List<Lobby>();
        }

        public LobbyDto CreateLobby(string name, int capacity, string connectionId, GameMode gameMode, int totalRoundCount, string? userId)
        {
            string? connectionCode = null;
            int currentPlayerCount = 0;
            if (gameMode == GameMode.LOCAL)
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
                    Console.WriteLine(connectionCode);
                } while (existingCodes.Contains(connectionCode));


            }
            else if (gameMode == GameMode.REMOTE)
            {
                currentPlayerCount = 1;
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
                HostUserId = userId
            };
            _lobbies.Add(lobby);
            return new LobbyDto(lobby.Id, lobby.Name, lobby.CurrentPlayerCount, lobby.Capacity, lobby.Status, lobby.GameMode, lobby.TotalRoundCount, lobby.ConnectionCode);
        }

        public async Task<JoinStatusDto> JoinLobbyWithCode(string code, string playerName, string connectionId, string? userId)
        {
            var lobby = _lobbies.FirstOrDefault(l => l.ConnectionCode == code);
            if (lobby == null)
                return new JoinStatusDto(null, JoinStatus.LOBBY_NOT_FOUND, null);
            else if (lobby.CurrentPlayerCount >= lobby.Capacity)
                return new JoinStatusDto(null, JoinStatus.LOBBY_FULL, null);
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

                var lobbies = this.GetLobbies();

                var players = this.GetPlayersInLobby(lobby.Id);
                await _lobbyHubContext.Groups.AddToGroupAsync(connectionId, lobby.Id);
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
            Console.WriteLine(lobby);
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
            lobby.Players.Add(newPlayer);
            lobby.CurrentPlayerCount += 1;
            var lobbies = this.GetLobbies();
            var players = GetPlayersInLobby(lobbyId);

            await _lobbyHubContext.Groups.AddToGroupAsync(connectionId, lobbyId);
            await _lobbyHubContext.Clients.All.ReceiveLobbies(lobbies);
            return new JoinStatusDto(new PlayerDto(newPlayer.Id, newPlayer.Name, newPlayer.Score), JoinStatus.SUCCESS, null);

        }
        public List<LobbyDto> GetLobbies()
        {
            List<LobbyDto> lobbyDtos = new List<LobbyDto>();
            foreach (var lobby in _lobbies)
            {
                Console.WriteLine(lobby.Name);

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
            var lobby = _lobbies.Find(lobby => lobby.HostConnectionId == connectionId);
            if (lobby != null)
            {
                await _lobbyHubContext.Clients.Group(lobby.Id).ReceiveHostDisconnectedFromLobby();
                _lobbies.Remove(lobby);
                var remainingLobbiesDto = this.GetLobbies();
                await _lobbyHubContext.Clients.All.ReceiveLobbies(remainingLobbiesDto);
                return;
            }
            foreach (var l in _lobbies)
            {
                var player = l.Players.Find(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    l.Players.Remove(player);
                    var lobbiesDto = this.GetLobbies();
                    var players = this.GetPlayersInLobby(l.Id);
                    await _lobbyHubContext.Clients.All.ReceiveLobbies(lobbiesDto);
                    await _lobbyHubContext.Clients.Group(l.Id).ReceivePlayersInLobby(players);
                    return;
                }

            }

        }

        public void RemoveLobby(string lobbyId)
        {
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
    }
}
