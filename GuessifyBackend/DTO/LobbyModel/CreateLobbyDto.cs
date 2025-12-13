using GuessifyBackend.Models.Enum;

namespace GuessifyBackend.DTO.LobbyModel
{
    public record CreateLobbyDto(LobbyCreateStatus CreateStatus, LobbyDto? Lobby);

}
