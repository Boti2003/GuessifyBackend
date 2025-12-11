using GuessifyBackend.DTO.DeezerResponse;
using GuessifyBackend.Service.Interfaces;
using GuessifyBackend.Utils.Converters;
using System.Text.Json;


namespace GuessifyBackend.Service.Implementations
{
    public class DeezerApiService : IDeezerApiService
    {
        static HttpClient client = new HttpClient();

        static JsonSerializerOptions serializerOptions = null!;

        private readonly ILogger<DeezerApiService> _logger;

        public DeezerApiService(ILogger<DeezerApiService> logger)
        {
            _logger = logger;
            serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new JsonNumberToStringConverter());
        }

        public async Task<MinimalAlbumListResponse?> GetAlbumsList(string artistId)
        {
            string path = $"https://api.deezer.com/artist/{artistId}/albums";
            HttpResponseMessage response;
            MinimalAlbumListResponse? albumsList = null;
            int retryCount = 3;
            bool success = false;
            while (retryCount > 0 && !success)
            {
                response = await client.GetAsync(path);
                var rawData = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(rawData);
                if (data.RootElement.TryGetProperty("error", out var errorElement))
                {
                    if (errorElement.TryGetProperty("code", out var code))
                    {
                        if (code.GetInt32() == 4)
                        {
                            _logger.LogError("Response code: " + code);
                            await Task.Delay(5000);
                            retryCount--;
                        }

                    }

                }
                else
                {
                    albumsList = await response.Content.ReadFromJsonAsync<MinimalAlbumListResponse>(serializerOptions);
                    success = true;
                }

                _logger.LogInformation($"{albumsList?.MinimalAlbumsList.Count} albums found for artist with id {artistId}");
            }

            return albumsList;
        }

        public async Task<AlbumResponse?> GetAlbum(string albumId)
        {
            string path = $"https://api.deezer.com/album/{albumId}";
            HttpResponseMessage response;
            AlbumResponse? album = null;
            int retryCount = 3;
            bool success = false;
            while (retryCount > 0 && !success)
            {
                response = await client.GetAsync(path);
                var rawData = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(rawData);
                if (data.RootElement.TryGetProperty("error", out var errorElement))
                {
                    if (errorElement.TryGetProperty("code", out var code))
                    {
                        if (code.GetInt32() == 4)
                        {
                            _logger.LogError("Response code: " + code);
                            await Task.Delay(5000);
                            retryCount--;
                        }

                    }

                }
                else
                {
                    album = await response.Content.ReadFromJsonAsync<AlbumResponse>(serializerOptions);
                    success = true;
                }

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
