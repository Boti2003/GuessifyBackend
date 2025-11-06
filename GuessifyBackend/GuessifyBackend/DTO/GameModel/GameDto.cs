using GuessifyBackend.Models.Enum;

namespace GuessifyBackend.DTO.GameModel
{
    public record GameDto(string Id, string Name, GameMode Mode, int totalRoundCount);

}
