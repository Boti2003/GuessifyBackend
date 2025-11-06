using GuessifyBackend.Models.Enum;

namespace GuessifyBackend.Models
{
    public class Lobby
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Player> Players { get; set; }
        public LobbyStatus Status { get; set; }
        public int Capacity { get; set; }
        public string HostConnectionId { get; set; }
        public int TotalRoundCount { get; set; }
        public GameMode GameMode { get; set; }
        public string? ConnectionCode { get; set; }
    }
}
