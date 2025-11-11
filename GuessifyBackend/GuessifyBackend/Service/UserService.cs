using GuessifyBackend.DTO.AuthDto;
using GuessifyBackend.Entities;
using GuessifyBackend.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GuessifyBackend.Service
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly GameDbContext _dbContext;

        public UserService(UserManager<User> userManager, GameDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;

        }

        public async Task CalculateSumScoresForUser(string userId)
        {
            var sumScore = await _dbContext.Players.Where(p => p.UserId == userId).SumAsync(p => p.Score);
            var user = await _userManager.FindByIdAsync(userId);
            user!.ScoreSum = sumScore;
            await _userManager.UpdateAsync(user);
        }

        public async Task<List<UserProfileDto>> GetScoresForUsers()
        {
            var users = await _userManager.Users.OrderByDescending(u => u.ScoreSum).ThenBy(u => u.DisplayName).
                Select(u => new { u.DisplayName, u.ScoreSum }).
                ToListAsync();
            var rankedUsers = users.Select((u, index) => new UserProfileDto(u.DisplayName!, u.ScoreSum, index + 1)).ToList();
            return rankedUsers;
        }

        public async Task<UserProfileDto?> GetUserProfile(string userId)
        {

            var users = await _userManager.Users.OrderByDescending(u => u.ScoreSum).ThenBy(u => u.DisplayName).
               Select(u => new { u.Id, u.DisplayName, u.ScoreSum }).ToListAsync();
            var user = users.Select((u, index) => new { u.Id, u.DisplayName, u.ScoreSum, Rank = index + 1 }).
                FirstOrDefault(u => u.Id == userId);

            if (user == null) return null;
            var userProfile = new UserProfileDto(user.DisplayName!, user.ScoreSum, user.Rank);
            return userProfile;
        }


    }
}
