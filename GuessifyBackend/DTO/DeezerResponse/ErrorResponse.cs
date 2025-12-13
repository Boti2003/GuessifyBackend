using System.Text.Json.Serialization;

namespace GuessifyBackend.DTO.DeezerResponse
{
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public ErrorData Error { get; set; } = null!;
    }

    public class ErrorData
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
    }

}
