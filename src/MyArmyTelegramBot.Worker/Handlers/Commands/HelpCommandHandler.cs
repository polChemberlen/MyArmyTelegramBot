using MyArmyTelegramBot.Application.Interfaces.Services;
using MyArmyTelegramBot.Domain.Enums;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyArmyTelegramBot.Worker.Handlers.Commands
{
    public class HelpCommandHandler
    {
        private readonly IUserService _userService;
        private readonly ITelegramBotClient _botClient;

        public HelpCommandHandler(IUserService userService, ITelegramBotClient botClient)
        {
            _userService = userService;
            _botClient = botClient;
        }

        public async Task HandleAsync(Message message, CancellationToken ct)
        {
            var tgUser = message.From;
            if (tgUser == null) return;

            var user = await _userService.GetByTelegramIdAsync(tgUser.Id, ct);
            bool isAdmin = user != null && user.Role == UserRole.Admin;

            var sb = new StringBuilder();
            sb.AppendLine("<b>Доступные команды:</b>\n");
            sb.AppendLine("<code>/start</code> — Запустить бота / Перезапустить");
            sb.AppendLine("<code>/apply</code> — Подать заявку на доступ");
            sb.AppendLine("<code>/ask &lt;текст&gt;</code> — Задать вопрос администраторам");
            sb.AppendLine("<code>/help</code> — Получить справку по командам");
            sb.AppendLine("<code>/dmb</code> — Когда Ълья вернется");

            if (isAdmin)
            {
                sb.AppendLine("\n<b>Онли для Лены, Артемиды и мейби кого-то ищэ:</b>");
                sb.AppendLine("<code>/users</code> — Список одобренных пользователей");
                sb.AppendLine("<code>/questions</code> — Список всех вопросов пользователей");
                sb.AppendLine("<code>/broadcast &lt;текст&gt;</code> — Массовая рассылка сообщений");
            }

            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: sb.ToString(),
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: ct
            );
        }
    }
}