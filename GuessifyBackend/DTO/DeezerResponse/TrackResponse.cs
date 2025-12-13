using System.Text.Json.Serialization;

namespace GuessifyBackend.DTO.DeezerResponse
{
    public class TrackResponseList
    {
        [JsonPropertyName("data")]
        public List<TrackResponse> TrackList { get; set; } = null!;
    }

    public class TrackResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

    }

    public class PreviewTrackResponse
    {
        [JsonPropertyName("preview")]
        public string PreviewUrl { get; set; } = null!;
    }
}
