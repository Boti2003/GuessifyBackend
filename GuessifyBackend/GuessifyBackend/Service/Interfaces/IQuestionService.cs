using GuessifyBackend.Entities;

namespace GuessifyBackend.Service.Interfaces
{
    public interface IQuestionService
    {
        Task<List<Question>> CreateQuestions(string categoryId, int count);
        Task SetSendDateForQuestion(string questionId, DateTime sendDate);

        Task SetEndDateForQuestion(string questionId, DateTime endDate);
    }
}