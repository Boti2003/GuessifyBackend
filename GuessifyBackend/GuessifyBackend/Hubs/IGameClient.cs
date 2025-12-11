using GuessifyBackend.DTO.GameModel;
using GuessifyBackend.DTO.LobbyModel;

namespace GuessifyBackend.Hubs
{
    public interface IGameClient
    {
        public Task ReceivePlayersInGame(List<PlayerDto> players);

        public Task ReceiveNewRoundStarted(GameRoundDto gameRound);

        public Task ReceiveNextQuestion(SendQuestionDto question);

        public Task ReceiveEndAnswerTime(string correctAnswer);

        public Task ReceiveEndGameRound(int nextRoundNumber);
        public Task ReceiveGameEnd(GameEndDto gameEndDto);

        public Task ReceiveVotingStarted(VotingTime votingTime);

        public Task ReceiveVotingEnded(CategoryDto categoryDto);

        public Task ExceptionThrown(string message);


    }
}
