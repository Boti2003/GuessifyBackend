namespace GuessifyBackend.DTO.GameModel
{
    public record CategoryGroupDto(string Id, string Name, List<CategoryDto> Categories);

}
