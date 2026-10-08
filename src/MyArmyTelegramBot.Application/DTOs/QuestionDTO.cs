namespace MyArmyTelegramBot.Application.DTOs
{
    public record QuestionDTO(
        string QuestionText,
        Guid Id = default,
        Guid UserId = default,
        DateTime CreatedAt = default,
        DateTime? UpdatedAt = null
    );
}
