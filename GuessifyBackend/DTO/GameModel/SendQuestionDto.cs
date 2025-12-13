namespace GuessifyBackend.DTO.GameModel
{
    public record SendQuestionDto(QuestionDto Question, long SendTime, int Duration);

}
