using GuessifyBackend.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuessifyBackend.Entities
{
    [Table("Games")]
    public class Game
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public GameStatus Status { get; set; }
        public GameMode Mode { get; set; }
        public string? HostConnectionId { get; set; }
        public int TotalRoundCount { get; set; }
        public virtual List<DbPlayer> Players { get; set; } = null!;
        public virtual List<GameRound> GameRounds { get; set; } = null!;
    }

}
