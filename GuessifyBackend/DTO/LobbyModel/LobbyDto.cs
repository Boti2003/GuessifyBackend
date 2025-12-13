using GuessifyBackend.Models.Enum;

namespace GuessifyBackend.DTO.LobbyModel
{
    public record LobbyDto(string Id, string Name, int NumberOfPlayers, int Capacity, LobbyStatus Status, GameMode GameMode, int TotalRoundCount, string? ConnectionCode);
}
