using GuessifyBackend.DTO.LobbyModel;
using GuessifyBackend.Models.Enum;

namespace GuessifyBackend.Service.Interfaces
{
    public interface ILobbyService
    {
        Task<CreateLobbyDto> CreateLobby(string name, int capacity, string connectionId, GameMode gameMode, int totalRoundCount, string? userId, string? userName);
        Task<JoinStatusDto> JoinLobbyWithCode(string code, string playerName, string connectionId, string? userId);
        Task<JoinStatusDto> JoinLobby(string lobbyId, string playerName, string connectionId, string? userId);
        List<LobbyDto> GetLobbies();
        Task HandleLobbyLeaving(string connectionId);
        List<PlayerDto> GetPlayersInLobby(string lobbyId);
        StartGameStatus CheckWhetherGameCanBeStarted(string lobbyId, string? hostPlayerName);

        Task AbandonLobby(string lobbyId, string gameId);
    }
}