using GuessifyBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace GuessifyBackend.Service
{
    public class QuestionService
    {
        private readonly GameDbContext _dbContext;

        private readonly DeezerApiService _deezerApiService;

        public QuestionService(GameDbContext gameDbContext, DeezerApiService deezerApiService)
        {
            _dbContext = gameDbContext;
            _deezerApiService = deezerApiService;
        }

        public async Task<List<Question>> CreateQuestions(string categoryId, int count)
        {
            var categ = await _dbContext.GameCategories.Include(c => c.TrackList)
                                                    .FirstOrDefaultAsync(c => c.Id.ToString() == categoryId);
            if (categ == null || categ.TrackList.Count < count)
            {
                throw new Exception("Category not found or not enough songs in category");
            }
            var tracks = categ.TrackList.ToArray();
            Random pickSongs = new();
            pickSongs.Shuffle(tracks);
            var songsForQuestions = tracks.Take(count);
            var titles = categ.TrackList.Select(t => t.Title).Distinct().ToArray();
            var artists = categ.TrackList.Select(t => t.Artist).Distinct().ToArray();
            List<Question> questions = new List<Question>();
            foreach (var song in songsForQuestions)
            {
                var isTitle = pickSongs.Next(0, 10) < 7;
                if (isTitle)
                {
                    var titlesWithoutAnswer = titles.Where(t => t != song.Title).ToArray();
                    pickSongs.Shuffle(titlesWithoutAnswer);
                    var options = titlesWithoutAnswer.Take(4).ToArray();
                    options[3] = song.Title;
                    pickSongs.Shuffle(options);
                    questions.Add(new Question
                    {
                        AnswerOptions = options.ToList(),
                        CorrectAnswer = song.Title,
                        SongId = song.Id.ToString(),
                    });
                }
                else
                {
                    var artistsWithoutAnswer = artists.Where(a => a != song.Artist).ToArray();
                    pickSongs.Shuffle(artistsWithoutAnswer);
                    var options = artistsWithoutAnswer.Take(4).ToArray();
                    options[3] = song.Artist;
                    questions.Add(new Question
                    {
                        AnswerOptions = options.ToList(),
                        CorrectAnswer = song.Artist,
                        SongId = song.Id.ToString(),
                    });
                }

            }
            return questions;

        }

        public async Task SetSendDateForQuestion(string questionId, DateTime sendDate)
        {
            var question = await _dbContext.Questions.FirstOrDefaultAsync(q => q.Id == Guid.Parse(questionId));
            if (question == null)
            {
                throw new ArgumentException("Question not found");
            }
            question.SendTime = sendDate;
            await _dbContext.SaveChangesAsync();
        }
    }
}
