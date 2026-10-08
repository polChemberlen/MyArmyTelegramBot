using System.Net;
using System.Text;
using MyArmyTelegramBot.Application.Interfaces.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyArmyTelegramBot.Worker.Handlers.Commands.Admin
{
    public class BroadcastCommandHandler
    {
        private readonly IUserService _userService;
        private readonly ITelegramBotClient _botClient;

        public BroadcastCommandHandler(IUserService userService, ITelegramBotClient botClient)
        {
            _userService = userService;
            _botClient = botClient;
        }

        public async Task HandleAsync(Message message, CancellationToken ct)
        {
            var adminUser = message.From;
            if (adminUser == null || string.IsNullOrWhiteSpace(message.Text)) return;

            var parts = message.Text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var broadcastText = parts.Length > 1 ? parts[1].Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(broadcastText))
            {
                await _botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "⚠️ <b>Укажите текст для рассылки.</b>\n\n" +
                          "Пример:\n" +
                          "<code>/broadcast План ВЕБ ОСИ на ближайший год</code>",
                    parseMode: ParseMode.Html,
                    cancellationToken: ct
                );
                return;
            }

            var users = (await _userService.GetApprovedUsersAsync(ct)).ToList();

            if (!users.Any())
            {
                await _botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "🫪 <b>Нет одобренных пользователей для рассылки.</b>",
                    parseMode: ParseMode.Html,
                    cancellationToken: ct
                );
                return;
            }

            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"🧑‍🦽🚀 Начинаю рассылку для <b>{users.Count}</b> пользователей...",
                parseMode: ParseMode.Html,
                cancellationToken: ct
            );

            int successCount = 0;
            int failedCount = 0;

            var safeBroadcastText = WebUtility.HtmlEncode(broadcastText);

            foreach (var user in users)
            {
                try
                {
                    await _botClient.SendMessage(
                        chatId: user.TelegramId,
                        text: safeBroadcastText,
                        parseMode: ParseMode.Html,
                        cancellationToken: ct
                    );

                    successCount++;
                    await Task.Delay(50, ct);
                }
                catch (ApiRequestException ex) when (ex.ErrorCode == 403)
                {
                    failedCount++;
                }
                catch (Exception)
                {
                    failedCount++;
                }
            }

            var report = new StringBuilder();
            report.AppendLine("✅ <b>Рассылка завершена!</b>\n");
            report.AppendLine($"📊 Успешно доставлено: <b>{successCount}</b>");

            if (failedCount > 0)
            {
                report.AppendLine($"❌ Не доставлено (блок/ошибка): <b>{failedCount}</b>");
            }

            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: report.ToString(),
                parseMode: ParseMode.Html,
                cancellationToken: ct
            );
        }
    }
}
