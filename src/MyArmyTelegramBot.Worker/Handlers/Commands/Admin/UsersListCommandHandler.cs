using System.Net;
using System.Text;
using MyArmyTelegramBot.Application.Interfaces.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyArmyTelegramBot.Worker.Handlers.Commands.Admin
{
    public class UsersListCommandHandler
    {
        private const int PageSize = 10;
        private readonly IUserService _userService;
        private readonly ITelegramBotClient _botClient;

        public UsersListCommandHandler(IUserService userService, ITelegramBotClient botClient)
        {
            _userService = userService;
            _botClient = botClient;
        }

        public async Task HandleAsync(Message message, CancellationToken ct)
        {
            await HandleAsync(message.Chat.Id, page: 1, messageIdToEdit: null, ct: ct);
        }

        public async Task HandleAsync(long chatId, int page, int? messageIdToEdit, CancellationToken ct)
        {
            var allUsers = (await _userService.GetApprovedUsersAsync(ct)).ToList();

            if (!allUsers.Any())
            {
                var emptyText = "👥 Список одобренных пользователей пуст.";

                if (messageIdToEdit.HasValue)
                {
                    await _botClient.EditMessageText(
                        chatId: chatId,
                        messageId: messageIdToEdit.Value,
                        text: emptyText,
                        cancellationToken: ct
                    );
                }
                else
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: emptyText,
                        cancellationToken: ct
                    );
                }
                return;
            }

            int totalPages = (int)Math.Ceiling(allUsers.Count / (double)PageSize);
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var pageUsers = allUsers
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"👥 <b>Список одобренных пользователей ({allUsers.Count})</b> — Страница {page}/{totalPages}:\n");

            int index = (page - 1) * PageSize + 1;
            foreach (var user in pageUsers)
            {
                var fullName = WebUtility.HtmlEncode($"{user.FirstName} {user.LastName}".Trim());

                var username = string.IsNullOrEmpty(user.Username)
                    ? "<i>нет username</i>"
                    : WebUtility.HtmlEncode($"@{user.Username}");

                var role = WebUtility.HtmlEncode(user.Role.ToString());

                sb.AppendLine($"{index++}. <b>{fullName}</b> | {username}");
                sb.AppendLine($"   ID: <code>{user.TelegramId}</code> | Роль: <code>{role}</code>\n");
            }

            var buttons = new List<InlineKeyboardButton>();

            if (page > 1)
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"upage_{page - 1}"));
            }

            if (page < totalPages)
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData("Вперед ➡️", $"upage_{page + 1}"));
            }

            InlineKeyboardMarkup? inlineKeyboard = buttons.Count > 0
                ? new InlineKeyboardMarkup(buttons)
                : null;

            if (messageIdToEdit.HasValue)
            {
                await _botClient.EditMessageText(
                    chatId: chatId,
                    messageId: messageIdToEdit.Value,
                    text: sb.ToString(),
                    parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: ct
                );
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: sb.ToString(),
                    parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: ct
                );
            }
        }
    }
}