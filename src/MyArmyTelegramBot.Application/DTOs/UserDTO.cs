using MyArmyTelegramBot.Domain.Enums;

namespace MyArmyTelegramBot.Application.DTOs
{
    public record UserDTO(
        long TelegramId,
        string? Username,
        string FirstName,
        string? LastName,
        Guid Id = default,
        UserRole Role = UserRole.Guest,
        UserApplicationStatus Status = UserApplicationStatus.None,
        DateTime? ApprovedAt = null,
        DateTime CreatedAt = default,
        DateTime? UpdatedAt = null
    );
}

