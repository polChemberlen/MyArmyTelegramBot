using MyArmyTelegramBot.Application.DTOs;

namespace MyArmyTelegramBot.Application.Interfaces.Services
{
    public interface IQuestionService
    {
        Task<QuestionDTO> CreateQuestionAsync(QuestionDTO dto, CancellationToken ct = default);

        Task<IEnumerable<QuestionDTO>> GetAllQuestionsAsync(CancellationToken ct = default);

        Task<IEnumerable<QuestionDTO>> GetAllQuestionsByTelegramId(long telegramId, CancellationToken ct = default);
    }
}
