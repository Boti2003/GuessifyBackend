using GuessifyBackend.DTO.AuthDto;
using GuessifyBackend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuessifyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _authService.RegisterUser(request.Email, request.Username, request.Password);
            if (result)
            {
                return Results.Ok(new { Message = "User registered successfully." });
            }
            else
            {
                return Results.BadRequest(result);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IResult> Login([FromBody] LoginRequestDto request)
        {
            var tokens = await _authService.LoginUser(request.Email, request.Password);
            if (tokens != null)
            {
                return Results.Ok(tokens);
            }
            return Results.Unauthorized();
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var tokens = await _authService.RefreshTokens(request.RefreshToken);
            if (tokens != null)
            {
                return Results.Ok(tokens);
            }
            return Results.Unauthorized();
        }





    }
}
