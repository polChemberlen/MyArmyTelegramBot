using MyArmyTelegramBot.Domain.Entities;
using MyArmyTelegramBot.Domain.Enums;

namespace MyArmyTelegramBot.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<UserEntity?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default);

        Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<IEnumerable<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<UserEntity>> GetAllByRoleAsync(UserRole role, CancellationToken cancellationToken = default);

        Task<IEnumerable<UserEntity>> GetAllByStatusAsync(UserApplicationStatus status, CancellationToken cancellationToken = default);

        Task AddAsync(UserEntity user, CancellationToken ct = default);

        Task UpdateAsync(UserEntity user, CancellationToken cancellationToken = default);
    }
}

