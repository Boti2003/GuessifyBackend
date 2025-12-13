using GuessifyBackend.DTO.GameModel;

namespace GuessifyBackend.Service.Interfaces
{
    public interface ICategoryService
    {

        Task<List<CategoryGroupDto>> GetCategoryGroups();

        Task<CategoryDto> GetCategory(string categoryId);

        Task<string> GetRandomCategoryId();
    }
}
