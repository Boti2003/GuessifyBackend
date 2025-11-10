using Microsoft.AspNetCore.Identity;

namespace GuessifyBackend.Entities.Identity
{
    public class User : IdentityUser
    {
        public string? DisplayName { get; set; }

        public int ScoreSum { get; set; }
    }
}
