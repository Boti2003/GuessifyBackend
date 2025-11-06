using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuessifyBackend.Entities
{
    public class PlayerAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string PlayerId { get; set; }
        public string QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int AnswerTimeInMilliseconds { get; set; }
        public int PointsAwarded { get; set; }
    }
}
