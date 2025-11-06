namespace GuessifyBackend.DTO.GameModel
{
    public class CategoryGroupDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<CategoryDto> Categories { get; set; }
    }
}
