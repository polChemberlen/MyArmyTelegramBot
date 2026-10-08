using MyArmyTelegramBot.Domain.Entities;

namespace MyArmyTelegramBot.Application.Interfaces.Repositories
{
    public interface IQuestionRepository
    {
        Task<Question?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<IEnumerable<Question>> GetAllAsync(CancellationToken ct = default);

        Task<IEnumerable<Question>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

        Task AddAsync(Question question, CancellationToken ct = default);
    }
}
