using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GuessifyBackend.DTO.DeezerResponse
{
    public class MinimalAlbumListResponse
    {
        [JsonPropertyName("data")]
        public List<MinimalAlbumResponse> MinimalAlbumsList { get; set; }
    }
}
