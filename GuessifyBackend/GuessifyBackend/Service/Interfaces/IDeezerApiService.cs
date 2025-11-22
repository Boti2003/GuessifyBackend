using GuessifyBackend.DTO.DeezerResponse;

namespace GuessifyBackend.Service.Interfaces
{
    public interface IDeezerApiService
    {
        Task<MinimalAlbumListResponse?> GetAlbumsList(string artistId);

        Task<AlbumResponse?> GetAlbum(string albumId);

        Task<string?> GetPreviewUrlOfTrack(string trackId);

    }
}
