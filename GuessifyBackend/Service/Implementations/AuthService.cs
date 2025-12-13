using GuessifyBackend.DTO.AuthDto;
using GuessifyBackend.Entities.Identity;
using GuessifyBackend.Service.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GuessifyBackend.Service.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenProviderService _tokenService;

        public AuthService(UserManager<User> userManager, ITokenProviderService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<bool> RegisterUser(string email, string username, string password)
        {
            var user = new User
            {
                UserName = email,
                Email = email,
                DisplayName = username
            };
            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task<TokensDto?> LoginUser(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                var tokens = await _tokenService.GenerateTokensForUser(user);
                return tokens;
            }
            return null;
        }

        public async Task<TokensDto?> RefreshTokens(string refreshToken)
        {
            var tokens = await _tokenService.RefreshTokens(refreshToken);
            return tokens;
        }

        public string? GenerateGuestToken()
        {
            return _tokenService.GenerateTokenForGuest();
        }

        public async Task Logout(string refreshToken)
        {
            await _tokenService.RevokeRefreshToken(refreshToken);
        }
    }
}
