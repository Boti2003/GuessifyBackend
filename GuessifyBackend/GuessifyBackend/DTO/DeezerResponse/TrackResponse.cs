using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GuessifyBackend.DTO.DeezerResponse
{
    public class TrackResponseList
    {
        [JsonPropertyName("data")]
        public List<TrackResponse> TrackList { get; set; }
    }

    public class TrackResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        
    }

    public class PreviewTrackResponse
    {
        [JsonPropertyName("preview")]
        public string PreviewUrl { get; set; }
    }
}
