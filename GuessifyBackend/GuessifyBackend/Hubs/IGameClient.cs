using GuessifyBackend.DTO.GameModel;
using GuessifyBackend.DTO.LobbyModel;

namespace GuessifyBackend.Hubs
{
    public interface IGameClient
    {
        public Task ReceivePlayersInGame(List<PlayerDto> players);

        public Task ReceiveNewRoundStarted(GameRoundDto gameRound);

        public Task ReceiveNextQuestion(QuestionDto question);

        public Task ReceiveEndAnswerTime(string correctAnswer);

        public Task ReceiveEndGameRound();
        public Task ReceiveGameEnd(GameEndDto gameEndDto);

        public Task ReceiveVotingStarted(List<CategoryGroupDto> categoryGroups);

        public Task ReceiveVotingEnded(CategoryDto categoryDto);


    }
}
