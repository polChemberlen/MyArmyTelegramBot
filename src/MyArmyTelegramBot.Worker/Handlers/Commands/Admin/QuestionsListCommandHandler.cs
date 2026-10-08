using System.Net;
using System.Text;
using MyArmyTelegramBot.Application.Interfaces.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyArmyTelegramBot.Worker.Handlers.Commands.Admin
{
    public class QuestionsListCommandHandler
    {
        private readonly IQuestionService _questionService;
        private readonly IUserService _userService;
        private readonly ITelegramBotClient _botClient;

        private const int PageSize = 5; // Число вопросов на 1 страницу

        public QuestionsListCommandHandler(
            IQuestionService questionService,
            IUserService userService,
            ITelegramBotClient botClient)
        {
            _questionService = questionService;
            _userService = userService;
            _botClient = botClient;
        }

        public async Task HandleAsync(long chatId, int page = 1, int? messageIdToEdit = null, CancellationToken ct = default)
        {
            var questions = (await _questionService.GetAllQuestionsAsync(ct))
                .OrderByDescending(q => q.CreatedAt)
                .ToList();

            if (!questions.Any())
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "❓ Вопросов пока нет.",
                    cancellationToken: ct
                );
                return;
            }

            var totalQuestions = questions.Count;
            var totalPages = (int)Math.Ceiling(totalQuestions / (double)PageSize);

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var pagedQuestions = questions
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"❓ <b>Список вопросов (Стр. {page}/{totalPages}):</b>\n");

            int index = (page - 1) * PageSize + 1;
            foreach (var q in pagedQuestions)
            {
                var user = await _userService.GetByIdAsync(q.UserId, ct);
                var telegramId = user?.TelegramId ?? 0;
                var username = string.IsNullOrEmpty(user?.Username) ? "без username" : $"@{user.Username}";
                var safeText = WebUtility.HtmlEncode(q.QuestionText);

                sb.AppendLine($"{index++}. 👤 <code>{telegramId}</code> ({username}) — <i>{q.CreatedAt:dd.MM.yyyy HH:mm}</i>");
                sb.AppendLine($"💬 {safeText}\n");
            }

            // Формируем кнопки пагинации
            var buttons = new List<InlineKeyboardButton>();

            if (page > 1)
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData("◀️ Назад", $"qpage_{page - 1}"));
            }

            if (page < totalPages)
            {
                buttons.Add(InlineKeyboardButton.WithCallbackData("Вперёд ▶️", $"qpage_{page + 1}"));
            }

            var keyboard = buttons.Count > 0 ? new InlineKeyboardMarkup(buttons) : null;

            // Если вызвали через CallbackQuery — редактируем текущее сообщение
            if (messageIdToEdit.HasValue)
            {
                await _botClient.EditMessageText(
                    chatId: chatId,
                    messageId: messageIdToEdit.Value,
                    text: sb.ToString(),
                    parseMode: ParseMode.Html,
                    replyMarkup: keyboard,
                    cancellationToken: ct
                );
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: sb.ToString(),
                    parseMode: ParseMode.Html,
                    replyMarkup: keyboard,
                    cancellationToken: ct
                );
            }
        }
    }
}