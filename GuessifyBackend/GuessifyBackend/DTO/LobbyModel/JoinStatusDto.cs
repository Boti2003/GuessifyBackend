using GuessifyBackend.Models.Enum;

namespace GuessifyBackend.DTO.LobbyModel
{
    public record JoinStatusDto(PlayerDto? Player, JoinStatus JoinStatus, string? lobbyId);
    //{
    //    public PlayerDto Player { get; set; }
    //    public JoinStatus JoinStatus { get; set; }
    //}
}
