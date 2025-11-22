using GuessifyBackend.DTO.GameModel;
using GuessifyBackend.DTO.LobbyModel;

namespace GuessifyBackend.Hubs
{
    public interface IlobbyClient
    {

        public Task ReceiveLobbies(List<LobbyDto> lobbies);

        public Task ReceivePlayersInLobby(List<PlayerDto> players);

        public Task RequestConnectionToGame(GameDto game);

        public Task ReceiveHostDisconnectedFromLobby();

    }
}
