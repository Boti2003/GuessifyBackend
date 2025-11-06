using GuessifyBackend.DTO.GameModel;
using GuessifyBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace GuessifyBackend.Service
{
    public class CategoryService
    {
        private readonly GameDbContext _dbContext;
        public CategoryService(GameDbContext gameDbContext)
        {
            _dbContext = gameDbContext;
        }
        public async Task<List<CategoryGroupDto>> GetCategoryGroups()
        {
            List<CategoryGroupDto> categoryGroupDtos = new List<CategoryGroupDto>();
            var categoryGroupsFromDb = await _dbContext.CategoryGroups.Include(cg => cg.Categories).ToListAsync();
            foreach (var categoryGroup in categoryGroupsFromDb)
            {
                List<CategoryDto> categoryDtos = new List<CategoryDto>();
                foreach (var category in categoryGroup.Categories)
                {
                    categoryDtos.Add(new CategoryDto
                    {
                        Id = category.Id.ToString(),
                        Name = category.Name
                    });
                }
                categoryGroupDtos.Add(new CategoryGroupDto
                {
                    Id = categoryGroup.Id.ToString(),
                    Name = categoryGroup.Name,
                    Categories = categoryDtos,
                });
            }
            return categoryGroupDtos;
        }

        public async Task<CategoryDto> GetCategory(string categoryId)
        {
            var category = await _dbContext.GameCategories.FirstOrDefaultAsync(c => c.Id == Guid.Parse(categoryId));
            if (category == null)
            {
                throw new ArgumentException("Category does not found");
            }
            return new CategoryDto
            {
                Id = category.Id.ToString(),
                Name = category.Name
            };
        }

        public async Task<string> GetRandomCategoryId()
        {
            var categories = await _dbContext.GameCategories.ToListAsync();
            var random = new Random();
            var randomIndex = random.Next(categories.Count);
            return categories[randomIndex].Id.ToString();
        }
    }
}
