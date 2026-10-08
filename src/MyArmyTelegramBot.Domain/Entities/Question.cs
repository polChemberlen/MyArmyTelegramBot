using MyArmyTelegramBot.Domain.Common;

namespace MyArmyTelegramBot.Domain.Entities
{
    public class Question : BaseEntity
    {
        public Guid UserId { get; set; }
        public UserEntity User { get; set; } = null!;
        public string QuestionText { get; set; } = string.Empty;

        protected Question() { }

        public Question(
            Guid userId,
            string questionText)
        {
            UserId = userId;
            QuestionText = questionText;
        }
    }
}
