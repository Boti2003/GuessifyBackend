using System.Collections.Concurrent;

namespace GuessifyBackend.Models
{
    public class VoteSummary
    {
        public string GameId { get; set; } = null!;

        public ConcurrentDictionary<string, int> VoteCounts { get; set; } = null!;

    }
}
