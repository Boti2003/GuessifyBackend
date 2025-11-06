using GuessifyBackend.Models;
using GuessifyBackend.Models.Enum;
using System.Collections.Concurrent;

namespace GuessifyBackend.Service
{
    public class VotingService
    {

        private List<VoteSummary> _voteSummaries;
        private readonly GameEventManager _gameEventManager;

        public VotingService(GameEventManager gameEventManager)
        {
            _voteSummaries = new List<VoteSummary>();
            _gameEventManager = gameEventManager;
        }

        public void AddVoteSummaryForGame(string gameId)
        {
            var votesummary = new VoteSummary
            {
                GameId = gameId,
                VoteCounts = new ConcurrentDictionary<string, int>()
            };
            _voteSummaries.Add(votesummary);
        }

        public void RemoveVoteSummaryForGame(string gameId)
        {
            _voteSummaries.RemoveAll(vs => vs.GameId == gameId);
        }

        public void RegisterVote(string gameId, string categoryId, int playerCount)
        {
            var voteSummary = _voteSummaries.FirstOrDefault(vs => vs.GameId == gameId);
            if (voteSummary != null)
            {
                voteSummary.VoteCounts.AddOrUpdate(categoryId, 1, (key, oldValue) => oldValue + 1);
                int sumVotes = voteSummary.VoteCounts.Values.Sum();
                if (sumVotes >= playerCount)
                {
                    _gameEventManager.RaiseEventOfGame(gameId, EventType.EVERYONE_VOTED);
                    Console.WriteLine($"All votes registered for game {gameId}. Winning category: {GetWinningCategory(gameId)}");
                }
                foreach (var key in voteSummary.VoteCounts.Keys)
                {
                    Console.WriteLine($"Category: {key}, Votes: {voteSummary.VoteCounts[key]}");

                }
            }
        }

        public void ResetVotesForGame(string gameId)
        {
            var voteSummary = _voteSummaries.FirstOrDefault(vs => vs.GameId == gameId);
            if (voteSummary != null)
            {
                voteSummary.VoteCounts.Clear();
            }
        }

        public string? GetWinningCategory(string gameId)
        {
            var voteSummary = _voteSummaries.FirstOrDefault(vs => vs.GameId == gameId);
            if (voteSummary != null && voteSummary.VoteCounts.Count > 0)
            {
                return voteSummary.VoteCounts.OrderByDescending(vc => vc.Value).First().Key;
            }
            return null;
        }

    }
}
