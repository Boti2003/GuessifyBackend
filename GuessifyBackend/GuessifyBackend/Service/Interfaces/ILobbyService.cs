using GuessifyBackend.DTO.LobbyModel;
using GuessifyBackend.Models.Enum;

namespace GuessifyBackend.Service.Interfaces
{
    public interface ILobbyService
    {
        Task<LobbyDto> CreateLobby(string name, int capacity, string connectionId, GameMode gameMode, int totalRoundCount, string? userId, string? userName);
        Task<JoinStatusDto> JoinLobbyWithCode(string code, string playerName, string connectionId, string? userId);
        Task<JoinStatusDto> JoinLobby(string lobbyId, string playerName, string connectionId, string? userId);
        List<LobbyDto> GetLobbies();
        Task HandleLobbyLeaving(string connectionId);
        void RemoveLobby(string lobbyId);
        List<PlayerDto> GetPlayersInLobby(string lobbyId);


    }
}