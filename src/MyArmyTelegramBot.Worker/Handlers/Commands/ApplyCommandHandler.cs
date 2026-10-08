using System.Net;
using Microsoft.Extensions.Configuration;
using MyArmyTelegramBot.Application.Interfaces.Services;
using MyArmyTelegramBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyArmyTelegramBot.Worker.Handlers.Commands
{
    public class ApplyCommandHandler
    {
        private readonly IUserService _userService;
        private readonly ITelegramBotClient _botClient;
        private readonly long[] _adminChatIds;

        public ApplyCommandHandler(
            IUserService userService,
            ITelegramBotClient botClient,
            IConfiguration configuration)
        {
            _userService = userService;
            _botClient = botClient;

            var adminIdsString = configuration["Admins:TelegramIds"] ?? string.Empty;

            _adminChatIds = adminIdsString
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(long.Parse)
                .ToArray();
        }

        public async Task HandleAsync(Message message, CancellationToken ct)
        {
            var tgUser = message.From;
            if (tgUser == null) return;

            var currentUser = await _userService.GetByTelegramIdAsync(tgUser.Id, ct);
            if (currentUser != null && currentUser.Status == UserApplicationStatus.Approved)
            {
                await _botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "✅ Ты уже принят. Для отправки вопроса юзай команду /ask",
                    cancellationToken: ct
                );
                return;
            }

            await _userService.SubmitApplicationAsync(tgUser.Id, ct);

            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "Ты подал заявку на доступ к рассылке и вопросам. Ж Д И",
                cancellationToken: ct
            );

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🫪 Одобрить", $"approve_{tgUser.Id}"),
                    InlineKeyboardButton.WithCallbackData("🧑‍🦽 Пошёл нахуй", $"reject_{tgUser.Id}")
                }
            });

            // Безопасное экранирование данных пользователя для HTML
            var firstName = WebUtility.HtmlEncode(tgUser.FirstName ?? string.Empty);
            var lastName = WebUtility.HtmlEncode(tgUser.LastName ?? string.Empty);
            var fullName = $"{firstName} {lastName}".Trim();

            var username = string.IsNullOrEmpty(tgUser.Username)
                ? "<i>нет username</i>"
                : WebUtility.HtmlEncode($"@{tgUser.Username}");

            var adminText = $"📩 <b>Новая заявка на доступ!</b>\n\n" +
                            $"👤 <b>Пользователь:</b> {fullName}\n" +
                            $"🔗 <b>Username:</b> {username}\n" +
                            $"🆔 <b>ID:</b> <code>{tgUser.Id}</code>";

            foreach (var adminId in _adminChatIds)
            {
                try
                {
                    await _botClient.SendMessage(
                        chatId: adminId,
                        text: adminText,
                        parseMode: ParseMode.Html,
                        replyMarkup: keyboard,
                        cancellationToken: ct
                    );
                    Console.WriteLine($"[INFO] Заявка успешно отправлена админу: {adminId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Ошибка отправки админу {adminId}: {ex.Message}");
                }
            }
        }
    }
}
