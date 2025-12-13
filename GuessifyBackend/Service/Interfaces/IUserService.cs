using GuessifyBackend.DTO.AuthDto;

namespace GuessifyBackend.Service.Interfaces
{
    public interface IUserService
    {
        Task CalculateSumScoresForUser(string userId);
        Task<List<UserProfileDto>> GetScoresForUsers();
        Task<UserProfileDto?> GetUserProfile(string userId);
    }
}