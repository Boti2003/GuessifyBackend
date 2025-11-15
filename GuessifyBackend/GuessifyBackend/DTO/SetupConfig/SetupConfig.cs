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

    public class CategoryGroupConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("categories")]
        public List<CategoryConfig> Categories { get; set; }
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
        public string Name { get; set; }
        [JsonPropertyName("artists")]
        public List<ArtistConfig> Artists { get; set; }
        [JsonPropertyName("albums")]
        public List<AlbumConfig> Albums { get; set; }
        [JsonPropertyName("excluded_albums")]
        public List<AlbumConfig> ExcludedAlbums { get; set; }


    }

    //public class EraCategoryConfig
    //{
    //    [JsonPropertyName("name")]
    //    public string Name { get; set; }
    //    [JsonPropertyName("artists")]
    //    public List<ArtistConfig> Artists { get; set; }

    //    [JsonPropertyName("albums")]
    //    public List<AlbumConfig> Albums { get; set; }
    //    [JsonPropertyName("excluded_albums")]
    //    public List<AlbumConfig> ExcludedAlbums { get; set; }

    //}

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
