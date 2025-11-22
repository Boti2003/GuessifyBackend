using GuessifyBackend.DTO.AuthDto;
using GuessifyBackend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GuessifyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
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

        [HttpPost("Login")]
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

        [HttpPost("Refresh")]
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

        [HttpGet("Guest-Token")]
        [AllowAnonymous]
        public IResult GetGuestToken()
        {
            var token = _authService.GenerateGuestToken();
            if (token != null)
            {
                return Results.Ok(new GuestTokenDto(token));
            }
            return Results.StatusCode(500);
        }

        [HttpGet("Validate-Token")]
        [Authorize]
        public IResult ValidateToken()
        {
            return Results.Ok();
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IResult> Logout([FromBody] LogoutRequestDto logoutRequestDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _authService.Logout(logoutRequestDto.RefreshToken);
            return Results.Ok(new { Message = "User logged out successfully." });
        }
    }
}
