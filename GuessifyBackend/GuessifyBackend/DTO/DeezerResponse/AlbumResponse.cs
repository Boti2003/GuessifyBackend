using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GuessifyBackend.DTO.DeezerResponse
{
    public class AlbumResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("genre_id")]
        public string GenreId { get; set; }

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; }

        [JsonPropertyName("contributors")]
        public List<ContributorResponse> Contributors { get; set; }

        [JsonPropertyName("tracks")]
        public TrackResponseList Tracks { get; set; }
    }
}
