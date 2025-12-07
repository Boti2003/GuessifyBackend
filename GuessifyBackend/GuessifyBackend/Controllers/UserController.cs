using GuessifyBackend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GuessifyBackend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("Me")]
        [Authorize]
        public async Task<IResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userService.GetUserProfile(userId!);


            if (user == null)
                return Results.NotFound();

            return Results.Ok(user);
        }

        [HttpGet("Scores")]
        [Authorize]
        public async Task<IResult> GetScoresForUsers()
        {
            var users = await _userService.GetScoresForUsers();
            return Results.Ok(users);
        }

    }
}
