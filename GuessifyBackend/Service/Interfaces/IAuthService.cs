using GuessifyBackend.DTO.AuthDto;

namespace GuessifyBackend.Service.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterUser(string email, string username, string password);

        Task<TokensDto?> LoginUser(string email, string password);
        Task<TokensDto?> RefreshTokens(string refreshToken);

        string? GenerateGuestToken();

        Task Logout(string refreshToken);
    }
}
