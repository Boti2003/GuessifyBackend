using System.Text.Json.Serialization;

namespace GuessifyBackend.DTO.SetupConfig
{
    public class SetupConfig
    {
        [JsonPropertyName("genreGroups")]
        public List<GenreGroupConfig> GenreGroups { get; set; } = null!;

        [JsonPropertyName("eraGroups")]
        public List<EraGroupConfig> EraGroups { get; set; } = null!;
    }

    public class CategoryGroupConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("categories")]
        public List<CategoryConfig> Categories { get; set; } = null!;
    }
    public class EraGroupConfig : CategoryGroupConfig
    {

        public int StartYear { get; set; }
        [JsonPropertyName("endYear")]
        public int EndYear { get; set; }


    }
    public class GenreGroupConfig : CategoryGroupConfig
    {

    }
    public class CategoryConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
        [JsonPropertyName("artists")]
        public List<ArtistConfig> Artists { get; set; } = null!;
        [JsonPropertyName("albums")]
        public List<AlbumConfig> Albums { get; set; } = null!;
        [JsonPropertyName("excluded_albums")]
        public List<AlbumConfig> ExcludedAlbums { get; set; } = null!;


    }


    public class ArtistConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
        [JsonPropertyName("deezerId")]
        public string ArtistDeezerId { get; set; } = null!;
    }

    public class AlbumConfig
    {

        [JsonPropertyName("artistName")]
        public string ArtistName { get; set; } = null!;
        [JsonPropertyName("deezerId")]
        public string AlbumDeezerId { get; set; } = null!;
    }
}
