using GuessifyBackend.DTO.GameModel;
using GuessifyBackend.DTO.LobbyModel;
using GuessifyBackend.Models.Enum;

namespace GuessifyBackend.Service.Interfaces
{
    public interface IGameService
    {
        Task<GameDto> StartNewGame(string name, DateTime startTime, GameMode gameMode, int totalRoundCount, string? hostConnectionId = null);
        Task<PlayerDto> AddPlayerToGame(string gameId, string playerName, string connectionId, string? userId);
        Task<List<PlayerDto>> GetPlayersInGame(string gameId);

        Task<GameStatus> StartNewRound(string gameId, string categoryId);
        Task ManageRemoteGamePlay(string gameId);
        Task DistributePointsBetweenPlayers(string questionId);
        Task RegisterAnswer(string gameId, string gameRoundId, string questionId, string answer, string playerId, DateTime time);
        Task<GameStatus> PlayGameRound(string gameId, GameRoundDto gameRound);
        Task<GameDto> GetGame(string gameId);
        Task ManageLeaveGame(string connectionId);
        Task<int> GetPlayerCountInGame(string gameId);

    }
}