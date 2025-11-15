using System.Text.Json.Serialization;

namespace GuessifyBackend.DTO.DeezerResponse
{
    public class AlbumResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = null!;

        [JsonPropertyName("tracks")]
        public TrackResponseList Tracks { get; set; } = null!;
    }
}
