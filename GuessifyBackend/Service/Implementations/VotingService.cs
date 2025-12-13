using GuessifyBackend.Models;
using GuessifyBackend.Models.Enum;
using GuessifyBackend.Service.Interfaces;
using System.Collections.Concurrent;

namespace GuessifyBackend.Service.Implementations
{
    public class VotingService : IVotingService
    {

        private List<VoteSummary> _voteSummaries;
        private readonly IGameEventManager _gameEventManager;
        private readonly ILogger<VotingService> _logger;

        public VotingService(IGameEventManager gameEventManager, ILogger<VotingService> logger)
        {
            _voteSummaries = new List<VoteSummary>();
            _gameEventManager = gameEventManager;
            _logger = logger;
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
                    _logger.LogInformation($"All votes registered for game {gameId}. Winning category: {GetWinningCategory(gameId)}");
                }
                foreach (var key in voteSummary.VoteCounts.Keys)
                {
                    _logger.LogInformation($"Category: {key}, Votes: {voteSummary.VoteCounts[key]}");

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
