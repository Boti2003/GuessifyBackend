using System.Text.Json.Serialization;

namespace GuessifyBackend.DTO.SetupConfig
{
    public class SetupConfig
    {
        [JsonPropertyName("genreGroups")]
        public List<GenreGroupConfig> GenreGroups { get; set; }

        [JsonPropertyName("eraGroups")]
        public List<EraGroupConfig> EraGroups { get; set; }
    }
    public class EraGroupConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("startYear")]
        public int StartYear { get; set; }
        [JsonPropertyName("endYear")]
        public int EndYear { get; set; }

        [JsonPropertyName("categories")]
        public List<EraCategoryConfig> Categories { get; set; }
    }
    public class GenreGroupConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("categories")]
        public List<GenreCategoryConfig> Categories { get; set; }
    }
    public class GenreCategoryConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("artists")]
        public List<ArtistConfig> Artists { get; set; }
        [JsonPropertyName("albums")]
        public List<AlbumConfig> Albums { get; set; }

    }

    public class EraCategoryConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("artists")]
        public List<ArtistConfig> Artists { get; set; }
        [JsonPropertyName("artistsWithMultipleActiveEras")]
        public List<ArtistConfig> ArtistsWithMultipleActiveEras { get; set; }
        [JsonPropertyName("albums")]
        public List<AlbumConfig> Albums { get; set; }

    }

    public class ArtistConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("deezerId")]
        public string ArtistDeezerId { get; set; }
    }

    public class AlbumConfig
    {
        //[JsonPropertyName("name")]
        //public string Name { get; set; }
        [JsonPropertyName("artistName")]
        public string ArtistName { get; set; }
        [JsonPropertyName("deezerId")]
        public string AlbumDeezerId { get; set; }
    }
}
