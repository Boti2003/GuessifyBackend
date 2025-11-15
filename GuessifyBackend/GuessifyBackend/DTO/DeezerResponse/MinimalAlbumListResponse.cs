using System.Text.Json.Serialization;

namespace GuessifyBackend.DTO.DeezerResponse
{
    public class MinimalAlbumListResponse
    {
        [JsonPropertyName("data")]
        public List<MinimalAlbumResponse> MinimalAlbumsList { get; set; } = null!;
    }
}
