using System.Text.Json.Serialization;

namespace GuessifyBackend.DTO.DeezerResponse
{
    public class MinimalAlbumResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;
    }
}
