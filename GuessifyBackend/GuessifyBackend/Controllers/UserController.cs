using GuessifyBackend.DTO.AuthDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GuessifyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet("me")]
        [Authorize]
        public IResult GetCurrentUser()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;


            if (name == null || email == null)
                return Results.NotFound();

            return Results.Ok(new UserProfileDto(name, email));
        }
    }
}
