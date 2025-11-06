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

        public LobbyDto CreateLobby(string name, int capacity, string connectionId, GameMode gameMode, int totalRoundCount)
        {
            string? connectionCode = null;
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
            Lobby lobby = new Lobby
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Players = new List<Player>(),
                Status = LobbyStatus.OPEN,
                Capacity = capacity,
                HostConnectionId = connectionId,
                GameMode = gameMode,
                ConnectionCode = connectionCode,
                TotalRoundCount = totalRoundCount
            };
            _lobbies.Add(lobby);
            return new LobbyDto(lobby.Id, lobby.Name, lobby.Players.Count, lobby.Capacity, lobby.Status, lobby.GameMode, lobby.TotalRoundCount, lobby.ConnectionCode);
        }

        public JoinStatusDto JoinLobbyWithCode(string code, string playerName, string connectionId)
        {
            var lobby = _lobbies.FirstOrDefault(l => l.ConnectionCode == code);
            if (lobby == null)
                return new JoinStatusDto(null, JoinStatus.LOBBY_NOT_FOUND, null);
            else if (lobby.Players.Count >= lobby.Capacity)
                return new JoinStatusDto(null, JoinStatus.LOBBY_FULL, null);
            else
            {
                var newPlayer = new Player
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = playerName,
                    Score = 0,
                    ConnectionId = connectionId
                };
                lobby.Players.Add(newPlayer);
                return new JoinStatusDto(new PlayerDto
                {
                    Id = newPlayer.Id,
                    Name = newPlayer.Name,
                    Score = newPlayer.Score
                }, JoinStatus.SUCCESS, lobby.Id);
            }

        }

        public PlayerDto JoinLobbyAsGuest(string lobbyId, string playerName, string connectionId)
        {
            var lobby = _lobbies.Find(lobby => lobby.Id == lobbyId);
            Console.WriteLine(lobby);
            var newPlayer = new Player
            {
                Id = Guid.NewGuid().ToString(),
                Name = playerName,
                Score = 0,
                ConnectionId = connectionId
            };
            if (lobby.Capacity <= lobby.Players.Count)
                throw new Exception("Lobby is full");
            lobby?.Players.Add(newPlayer);

            return new PlayerDto
            {
                Id = newPlayer.Id,
                Name = newPlayer.Name,
                Score = newPlayer.Score
            };
        }
        public List<LobbyDto> GetLobbies()
        {
            List<LobbyDto> lobbyDtos = new List<LobbyDto>();
            foreach (var lobby in _lobbies)
            {
                Console.WriteLine(lobby.Name);
                if (lobby.Status == LobbyStatus.CLOSED)
                    continue;
                lobbyDtos.Add(new LobbyDto(
                    lobby.Id,
                    lobby.Name,
                    lobby.Players.Count,
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
                    var players = await this.GetPlayersInLobby(l.Id);
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

        public async Task<List<PlayerDto>> GetPlayersInLobby(string lobbyId)
        {
            var lobby = _lobbies.Find(lobby => lobby.Id == lobbyId);
            if (lobby == null)
                return null;
            List<PlayerDto> playerDtos = new List<PlayerDto>();
            foreach (var player in lobby.Players)
            {
                playerDtos.Add(new PlayerDto
                {
                    Id = player.Id,
                    Name = player.Name,
                    Score = player.Score
                });
            }
            return playerDtos;
        }
    }
}
