using Microsoft.AspNetCore.Identity;

namespace GuessifyBackend.Entities.Identity
{
    public class User : IdentityUser
    {
        public string? DisplayName { get; set; }
    }
}
