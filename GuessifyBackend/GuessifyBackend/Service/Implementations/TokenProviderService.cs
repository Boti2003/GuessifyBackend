using GuessifyBackend.DTO.AuthDto;
using GuessifyBackend.Entities.Identity;
using GuessifyBackend.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GuessifyBackend.Service.Implementations
{
    public class TokenProviderService : ITokenProviderService
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
                new Claim(ClaimTypes.NameIdentifier, user.Id),
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

        public string? GenerateTokenForGuest()
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            List<Claim> claims =
            [
                new Claim(ClaimTypes.Anonymous, "true"),
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

            return token;
        }

        public async Task<string> GenerateRefreshToken(User user)
        {
            string newToken;
            bool notExistingToken;
            do
            {
                newToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                notExistingToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == newToken) == null;

            } while (!notExistingToken);

            var refreshToken = new RefreshToken
            {
                Token = newToken,
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
            await _dbContext.SaveChangesAsync();
            return tokens;
        }

        public async Task RevokeRefreshToken(string refreshToken)
        {
            var token = _dbContext.RefreshTokens.Single(t => t.Token == refreshToken);
            token.IsRevoked = true;
            await _dbContext.SaveChangesAsync();

        }
    }
}
