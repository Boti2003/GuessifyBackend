namespace GuessifyBackend.Service.Interfaces
{
    public interface IVotingService
    {
        void AddVoteSummaryForGame(string gameId);
        void RemoveVoteSummaryForGame(string gameId);
        void RegisterVote(string gameId, string categoryId, int playerCount);
        void ResetVotesForGame(string gameId);
        string? GetWinningCategory(string gameId);
    }
}