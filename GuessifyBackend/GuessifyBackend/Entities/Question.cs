using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuessifyBackend.Entities
{
    [Table("Questions")]
    public class Question
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public List<string> AnswerOptions { get; set; } = null!;
        public DateTime SendTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string SongId { get; set; } = null!;
        public string CorrectAnswer { get; set; } = null!;

    }
}
