using MyArmyTelegramBot.Domain.Common;
using MyArmyTelegramBot.Domain.Enums;

namespace MyArmyTelegramBot.Domain.Entities
{
    public class UserEntity : BaseEntity
    {
        public long TelegramId { get; protected set; }
        public string? Username { get; protected set; }
        public string FirstName { get; protected set; } = string.Empty;
        public string? LastName { get; protected set; }
        public UserRole Role { get; protected set; }
        public UserApplicationStatus Status { get; protected set; }

        public DateTime? ApprovedAt { get; protected set; }

        // Foreign key to Questtion entity
        public ICollection<Question> Questions { get; } = new List<Question>();

        protected UserEntity() { }

        public UserEntity(
            long telegramId,
            string? username,
            string? firstName,
            string? lastName,
            DateTime? approvedAt)
        {
            TelegramId = telegramId;
            Username = username;
            FirstName = firstName ?? string.Empty;
            LastName = lastName;
            Role = UserRole.Guest;
            Status = UserApplicationStatus.None;
            ApprovedAt = approvedAt;
        }

        public void Update(
            string? username,
            string? firstName,
            string? lastName)
        {
            Username = username;
            FirstName = firstName ?? string.Empty;
            LastName = lastName;
        }

        public void SetStatus(UserApplicationStatus status)
        {
            Status = status;
        }

        public void SetRole(UserRole role)
        {
            Role = role;
        }

        public bool IsApproved() => Role == UserRole.ApprovedUser && Status == UserApplicationStatus.Approved;
        public bool IsNew() => Role == UserRole.Guest && Status == UserApplicationStatus.None;
    }
}
