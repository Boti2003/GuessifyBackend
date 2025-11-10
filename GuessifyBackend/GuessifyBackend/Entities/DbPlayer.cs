using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuessifyBackend.Entities
{
    [Table("Players")]
    public class DbPlayer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Score { get; set; }
        public string ConnectionId { get; set; } = null!;
        public string? UserId { get; set; }

        public bool IsGuest { get; set; }


    }
}
