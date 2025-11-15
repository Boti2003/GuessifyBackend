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
            /*_dbContext.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Songs', RESEED, 0)");
            _dbContext.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('GameCategories', RESEED, 0)");
            _dbContext.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('CategoryGroups', RESEED, 0)");*/

            _dbContext.Songs.ExecuteDelete();
            _dbContext.GameCategories.ExecuteDelete();
            _dbContext.CategoryGroups.ExecuteDelete();

            foreach (var gr in config.GenreGroups)
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

                    foreach (var artist in c.Artists)
                    {
                        var songsFromThisArtist = await GetSongsFromArtist(artist.ArtistDeezerId, artist.Name, false);
                        songs.AddRange(songsFromThisArtist);
                    }

                    foreach (var album in c.Albums)
                    {
                        var songsFromExtraAlbums = await GetSongsFromAlbum(album.AlbumDeezerId, album.ArtistName);
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
            foreach (var gr in config.EraGroups)
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

                    foreach (var artist in c.Artists)
                    {
                        var songsFromThisArtist = await GetSongsFromArtist(artist.ArtistDeezerId, artist.Name, false);
                        songs.AddRange(songsFromThisArtist);
                    }

                    foreach (var artist in c.ArtistsWithMultipleActiveEras)
                    {
                        Console.WriteLine(artist.Name);
                        var songsFromThisArtist = await GetSongsFromArtist(artist.ArtistDeezerId, artist.Name, true, gr.StartYear, gr.EndYear);
                        songs.AddRange(songsFromThisArtist);
                    }

                    foreach (var album in c.Albums)
                    {
                        var songsFromExtraAlbums = await GetSongsFromAlbum(album.AlbumDeezerId, album.ArtistName);
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
            _dbContext.SaveChanges();
        }


        private async Task<List<Song>> GetSongsFromArtist(string artistId, string artistName, bool era, int startYear = -1, int endYear = -1)
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
                List<Song> songsFromThisAlbum = null;

                songsFromThisAlbum = await GetSongsFromAlbum(album.Id, artistName);

                if (songsFromThisAlbum == null || songsFromThisAlbum.Count == 0)
                {
                    continue;
                }
                foreach (var song in songsFromThisAlbum)
                {
                    var songsWithThisTitle = songs.Where(s => s.Title.ToLower() == song.Title.ToLower() && s.Artist == artistName).ToList();
                    if (era && (song.YearOfPublication < startYear || song.YearOfPublication > endYear))
                    {
                        Console.WriteLine(song.YearOfPublication);
                        Console.WriteLine(startYear + " " + endYear);
                        continue;
                    }
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


        private async Task<List<Song>> GetSongsFromAlbum(string albumId, string artistName)
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
