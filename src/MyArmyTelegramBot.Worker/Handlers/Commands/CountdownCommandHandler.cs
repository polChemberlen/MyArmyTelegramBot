using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyArmyTelegramBot.Worker.Handlers.Commands
{
    public class CountdownCommandHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly DateTime _demobDate;

        public CountdownCommandHandler(ITelegramBotClient botClient, IConfiguration configuration)
        {
            _botClient = botClient;

            // Читаем дату из конфигурации (.env или appsettings.json)
            var startDateStr = configuration["Demob:StartDate"]
                            ?? configuration["Demob__StartDate"];

            if (DateTime.TryParse(startDateStr, out var startDate))
            {
                _demobDate = startDate.AddYears(1); // Ровно 1 год службы
            }
            else
            {
                // Заглушка, если дата не задана
                _demobDate = DateTime.UtcNow.AddYears(1);
            }
        }

        public async Task HandleAsync(Message message, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            if (now >= _demobDate)
            {
                await _botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "🥳 <b>УРА! ДЕМБЕЛЬ НАСТУПИЛ!</b> 🥳\nЯ уже дома!",
                    parseMode: ParseMode.Html,
                    cancellationToken: ct
                );
                return;
            }

            var remaining = _demobDate - now;

            var text = $"⏳ <b>До возвращения осталось:</b>\n\n" +
                       $"📅 <b>Дата ДМБ:</b> <code>{_demobDate:dd.MM.yyyy}</code>\n\n" +
                       $"🎯 <b>Дней:</b> <code>{remaining.Days}</code>\n" +
                       $"⏱ <b>Часов:</b> <code>{remaining.Hours}</code>\n" +
                       $"⚡ <b>Минут:</b> <code>{remaining.Minutes}</code>\n\n" +
                       $"<i>Не зная горя горя горя</i>";

            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: text,
                parseMode: ParseMode.Html,
                cancellationToken: ct
            );
        }
    }
}