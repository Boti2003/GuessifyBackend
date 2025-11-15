namespace GuessifyBackend.Models
{
    public class Player
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Score { get; set; }
        public string ConnectionId { get; set; } = null!;
        public string? UserId { get; set; }
    }
}
