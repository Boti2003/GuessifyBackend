using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuessifyBackend.Entities
{
    public class Song
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string DeezerId { get; set; }
        public string Title { get; set; }
        public int YearOfPublication { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }
    }
}
