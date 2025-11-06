using GuessifyBackend.DTO.GameModel;
using GuessifyBackend.DTO.LobbyModel;

namespace GuessifyBackend.Hubs
{
    public interface IlobbyClient
    {
        /*public Task<string> CreateLobby(string lobbyName, int capacity);
        public Task StartNewGame(string lobbyId);
        public Task JoinLobby(string lobbyId);*/

        public Task ReceiveLobbies(List<LobbyDto> lobbies);

        public Task ReceivePlayersInLobby(List<PlayerDto> players);

        public Task RequestConnectionToGame(GameDto game);

        public Task ReceiveHostDisconnectedFromLobby();

    }
}
