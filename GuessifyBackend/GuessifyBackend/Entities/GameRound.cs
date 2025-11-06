using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuessifyBackend.Entities
{
    public class GameRound
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string GameCategoryId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public virtual List<Question> Questions { get; set; }
        public virtual List<PlayerAnswer> Answers { get; set; }
    }
}
