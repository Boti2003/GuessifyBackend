using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuessifyBackend.Entities
{
    public class PlayerAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string PlayerId { get; set; } = null!;
        public string QuestionId { get; set; } = null!;
        public string SelectedAnswer { get; set; } = null!;
        public bool IsCorrect { get; set; }
        public int AnswerTimeInMilliseconds { get; set; }
        public int PointsAwarded { get; set; }
    }
}
