using System.ComponentModel.DataAnnotations.Schema;

namespace GuessifyBackend.Entities.Identity
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = null!;
        public string UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
    }
}
