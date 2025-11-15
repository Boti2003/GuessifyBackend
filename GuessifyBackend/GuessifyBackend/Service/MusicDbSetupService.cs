using GuessifyBackend.DTO.SetupConfig;
using GuessifyBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace GuessifyBackend.Service
{
    public class MusicDbSetupService
    {
        private readonly GameDbContext _dbContext;
        private readonly DeezerApiService _deezerApiService;
        public MusicDbSetupService(GameDbContext dbContext, DeezerApiService deezerApiService)
        {
            _dbContext = dbContext;
            _deezerApiService = deezerApiService;
        }

        public async Task BuildMusicDbStructure(SetupConfig config)
        {

            _dbContext.Songs.ExecuteDelete();
            _dbContext.GameCategories.ExecuteDelete();
            _dbContext.CategoryGroups.ExecuteDelete();

            List<CategoryGroupConfig> allCategoryGroups = new List<CategoryGroupConfig>();
            allCategoryGroups.AddRange(config.GenreGroups);
            allCategoryGroups.AddRange(config.EraGroups);
            foreach (var gr in allCategoryGroups)
            {
                var categoryGroup = new CategoryGroup
                {
                    Name = gr.Name,
                    Categories = new List<GameCategory>()
                };
                foreach (var c in gr.Categories)
                {
                    var category = new GameCategory
                    {
                        Name = c.Name,
                        TrackList = new List<Song>()
                    };
                    List<Song> songs = new List<Song>();
                    List<string> excludedAlbumIds = c.ExcludedAlbums.Select(a => a.AlbumDeezerId).ToList();

                    foreach (var artist in c.Artists)
                    {
                        var songsFromThisArtist = await GetSongsFromArtist(artist.ArtistDeezerId, artist.Name, excludedAlbumIds);
                        if (songsFromThisArtist != null)
                        {
                            songs.AddRange(songsFromThisArtist);
                        }

                    }

                    foreach (var album in c.Albums)
                    {
                        var songsFromExtraAlbums = await GetSongsFromAlbum(album.AlbumDeezerId, album.ArtistName);
                        if (songsFromExtraAlbums == null)
                        {
                            continue;
                        }
                        foreach (var song in songsFromExtraAlbums)
                        {
                            var songsWithThisTitle = songs.Where(s => s.Title.ToLower() == song.Title.ToLower() && s.Artist == song.Artist).ToList();
                            if (songsWithThisTitle.Count == 1)
                            {
                                if (songsWithThisTitle[0].YearOfPublication > song.YearOfPublication)
                                {
                                    songs.Remove(songsWithThisTitle[0]);
                                    songs.Add(song);
                                }
                            }
                            else if (songsWithThisTitle.Count == 0)
                            {
                                songs.Add(song);
                            }
                        }
                    }

                    category.TrackList = songs;
                    categoryGroup.Categories.Add(category);
                }
                _dbContext.CategoryGroups.Add(categoryGroup);

            }
            /*foreach (var gr in config.EraGroups)
            {
                var categoryGroup = new CategoryGroup
                {
                    Name = gr.Name,
                    Categories = new List<GameCategory>()
                };
                foreach (var c in gr.Categories)
                {
                    var category = new GameCategory
                    {
                        Name = c.Name,
                        TrackList = new List<Song>()
                    };
                    List<Song> songs = new List<Song>();
                    List<string> excludedAlbumIds = c.ExcludedAlbums.Select(a => a.AlbumDeezerId).ToList();
                    foreach (var artist in c.Artists)
                    {
                        var songsFromThisArtist = await GetSongsFromArtist(artist.ArtistDeezerId, artist.Name, excludedAlbumIds);
                        if (songsFromThisArtist != null)
                        {
                            songs.AddRange(songsFromThisArtist);
                        }
                    }

                    foreach (var album in c.Albums)
                    {
                        var songsFromExtraAlbums = await GetSongsFromAlbum(album.AlbumDeezerId, album.ArtistName);
                        if (songsFromExtraAlbums == null)
                        {
                            continue;
                        }
                        foreach (var song in songsFromExtraAlbums)
                        {
                            var songsWithThisTitle = songs.Where(s => s.Title.ToLower() == song.Title.ToLower() && s.Artist == song.Artist).ToList();
                            if (songsWithThisTitle.Count == 1)
                            {
                                if (songsWithThisTitle[0].YearOfPublication > song.YearOfPublication)
                                {
                                    songs.Remove(songsWithThisTitle[0]);
                                    songs.Add(song);
                                }
                            }
                            else if (songsWithThisTitle.Count == 0)
                            {
                                songs.Add(song);
                            }
                        }
                    }

                    category.TrackList = songs;
                    categoryGroup.Categories.Add(category);
                }
                _dbContext.CategoryGroups.Add(categoryGroup);

            }*/
            _dbContext.SaveChanges();
        }


        private async Task<List<Song>?> GetSongsFromArtist(string artistId, string artistName, List<string> excludedAlbums)
        {
            List<Song> songs = new List<Song>();
            var albumList = await _deezerApiService.GetAlbumsList(artistId);
            if (albumList == null)
            {
                return null;
            }
            if (albumList.MinimalAlbumsList == null)
            {
                return null;
            }
            foreach (var album in albumList.MinimalAlbumsList)
            {
                if (excludedAlbums.Contains(album.Id))
                    continue;

                List<Song>? songsFromThisAlbum = null;

                songsFromThisAlbum = await GetSongsFromAlbum(album.Id, artistName);

                if (songsFromThisAlbum == null || songsFromThisAlbum.Count == 0)
                {
                    continue;
                }
                foreach (var song in songsFromThisAlbum)
                {
                    var songsWithThisTitle = songs.Where(s => s.Title.ToLower() == song.Title.ToLower() && s.Artist == artistName).ToList();

                    if (songsWithThisTitle.Count == 1)
                    {
                        if (songsWithThisTitle[0].YearOfPublication > song.YearOfPublication)
                        {
                            songs.Remove(songsWithThisTitle[0]);
                            songs.Add(song);
                        }
                    }
                    else if (songsWithThisTitle.Count == 0)
                    {
                        songs.Add(song);
                    }
                }
            }
            return songs;
        }


        private async Task<List<Song>?> GetSongsFromAlbum(string albumId, string artistName)
        {
            List<Song> songs = new List<Song>();
            var fullAlbumResponse = await _deezerApiService.GetAlbum(albumId);
            if (fullAlbumResponse == null || fullAlbumResponse.Tracks == null || fullAlbumResponse.Tracks.TrackList.Count == 0)
            {
                return null;
            }
            foreach (var track in fullAlbumResponse.Tracks.TrackList)
            {
                int year = Int32.Parse(fullAlbumResponse.ReleaseDate.Substring(0, 4));
                var song = new Song
                {

                    DeezerId = track.Id,
                    Title = track.Title,
                    YearOfPublication = year,
                    Artist = artistName,
                    Album = fullAlbumResponse.Title,

                };

                songs.Add(song);
            }

            return songs;
        }

    }
}
