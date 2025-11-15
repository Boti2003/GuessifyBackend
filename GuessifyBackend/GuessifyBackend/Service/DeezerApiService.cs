using GuessifyBackend.DTO.DeezerResponse;
using GuessifyBackend.Utils.Converters;
using System.Text.Json;


namespace GuessifyBackend.Service
{
    public class DeezerApiService
    {
        static HttpClient client = new HttpClient();

        static JsonSerializerOptions serializerOptions = null!;

        public DeezerApiService()
        {
            serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new JsonNumberToStringConverter());
        }

        public async Task<MinimalAlbumListResponse?> GetAlbumsList(string artistId)
        {
            string path = $"https://api.deezer.com/artist/{artistId}/albums";
            HttpResponseMessage response = await client.GetAsync(path);
            MinimalAlbumListResponse? albumsList = null;
            if (response.IsSuccessStatusCode)
            {
                albumsList = await response.Content.ReadFromJsonAsync<MinimalAlbumListResponse>(serializerOptions);
            }
            return albumsList;
        }

        public async Task<AlbumResponse?> GetAlbum(string albumId)
        {
            string path = $"https://api.deezer.com/album/{albumId}";
            HttpResponseMessage response = await client.GetAsync(path);
            AlbumResponse? album = null;
            if (response.IsSuccessStatusCode)
            {
                album = await response.Content.ReadFromJsonAsync<AlbumResponse>(serializerOptions);
            }
            return album;
        }

        public async Task<string?> GetPreviewUrlOfTrack(string trackId)
        {
            string path = $"https://api.deezer.com/track/{trackId}";
            HttpResponseMessage response = await client.GetAsync(path);
            string? previewUrl = "";
            if (response.IsSuccessStatusCode)
            {
                var track = await response.Content.ReadFromJsonAsync<PreviewTrackResponse>(serializerOptions);
                previewUrl = track?.PreviewUrl;
            }
            return previewUrl;
        }
    }
}
