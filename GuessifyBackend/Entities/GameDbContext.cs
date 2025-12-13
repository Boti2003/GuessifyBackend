using Microsoft.EntityFrameworkCore;

namespace GuessifyBackend.Entities
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {

        }

        public DbSet<Song> Songs { get; set; }

        public DbSet<CategoryGroup> CategoryGroups { get; set; }

        public DbSet<GameCategory> GameCategories { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<GameRound> GameRounds { get; set; }
        public DbSet<DbPlayer> Players { get; set; }

        public DbSet<PlayerAnswer> PlayerAnswers { get; set; }

        public DbSet<Question> Questions { get; set; }

    }
}
