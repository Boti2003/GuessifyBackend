using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuessifyBackend.Entities
{
    public class PlayerAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string SelectedAnswer { get; set; } = null!;
        public bool IsCorrect { get; set; }
        public int AnswerTimeInMilliseconds { get; set; }
        public int? PointsAwarded { get; set; }

        public Guid PlayerId { get; set; }
        public Guid QuestionId { get; set; }
        [ForeignKey("PlayerId")]
        public virtual DbPlayer Player { get; set; } = null!;
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;
    }
}
