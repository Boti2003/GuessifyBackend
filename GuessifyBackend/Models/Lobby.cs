using GuessifyBackend.Models.Enum;

namespace GuessifyBackend.Models
{
    public class Lobby
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<Player> Players { get; set; } = null!;
        public LobbyStatus Status { get; set; }
        public int Capacity { get; set; }

        public int CurrentPlayerCount { get; set; }
        public string HostConnectionId { get; set; } = null!;

        public string? HostUserId { get; set; }

        public string? HostUserName { get; set; }
        public int TotalRoundCount { get; set; }
        public GameMode GameMode { get; set; }
        public string? ConnectionCode { get; set; }
    }
}
