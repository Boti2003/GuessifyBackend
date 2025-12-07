using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuessifyBackend.Entities
{
    public class GameRound
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public Guid GameCategoryId { get; set; }
        [ForeignKey("GameCategoryId")]
        public virtual GameCategory GameCategory { get; set; } = null!;
        public virtual List<Question> Questions { get; set; } = null!;
        public virtual List<PlayerAnswer> Answers { get; set; } = null!;
    }
}
