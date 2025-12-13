using GuessifyBackend.DTO.AuthDto;
using GuessifyBackend.Entities.Identity;

namespace GuessifyBackend.Service.Interfaces
{
    public interface ITokenProviderService
    {
        Task<TokensDto> GenerateTokensForUser(User user);
        string? GenerateTokenForGuest();
        Task<string> GenerateRefreshToken(User user);
        Task<TokensDto?> RefreshTokens(string refreshToken);
        Task RevokeRefreshToken(string refreshToken);
    }
}