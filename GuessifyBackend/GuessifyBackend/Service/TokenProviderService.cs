using GuessifyBackend.DTO.AuthDto;
using GuessifyBackend.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GuessifyBackend.Service
{
    public class TokenProviderService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        public TokenProviderService(IConfiguration configuration, ApplicationDbContext dbContext, UserManager<User> userManager)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<TokensDto> GenerateTokensForUser(User user)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            List<Claim> claims =
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.DisplayName!)
            ];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var tokenHandler = new JsonWebTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = await GenerateRefreshToken(user);
            return new TokensDto(token, refreshToken);
        }

        public async Task<string> GenerateRefreshToken(User user)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            return refreshToken.Token;
        }

        public async Task<TokensDto?> RefreshTokens(string refreshToken)
        {
            var token = _dbContext.RefreshTokens.Single(t => t.Token == refreshToken);
            var user = await _userManager.FindByIdAsync(token.UserId);
            if (token == null || token.IsRevoked || token.ExpiryDate <= DateTime.UtcNow || user == null)
            {
                return null;
            }
            var tokens = await GenerateTokensForUser(user);
            token.IsRevoked = true;
            return tokens;
        }
    }
}
