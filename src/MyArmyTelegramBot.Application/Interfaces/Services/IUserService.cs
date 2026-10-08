using MyArmyTelegramBot.Application.DTOs;

namespace MyArmyTelegramBot.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserDTO> GetOrCreateUserAsync(UserDTO dto, CancellationToken ct = default);

        Task<IEnumerable<UserDTO>> GetApprovedUsersAsync(CancellationToken ct = default);

        Task<UserDTO?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default);

        Task<UserDTO?> GetByIdAsync(Guid userId, CancellationToken ct = default);

        Task SubmitApplicationAsync(long telegramId, CancellationToken ct = default);

        Task ApproveUserAsync(long telegramId, CancellationToken ct = default);

        Task RejectUserAsync(long telegramId, CancellationToken ct = default);

        Task BanUserAsync(Guid userId, CancellationToken ct = default);

        Task<UserDTO> GetApprovedTelegramIdsAsync();
    }
}
