namespace MyArmyTelegramBot.Worker.Options
{
    public class BotConfiguration
    {
        public const string SectionName = "BotConfiguration";
        public string BotToken { get; set; } = string.Empty;
    }
}
