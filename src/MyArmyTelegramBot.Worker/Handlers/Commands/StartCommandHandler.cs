using MyArmyTelegramBot.Application.DTOs;
using MyArmyTelegramBot.Application.Interfaces.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyArmyTelegramBot.Worker.Handlers.Commands
{
    public class StartCommandHandler
    {
        private readonly IUserService _userService;
        private readonly ITelegramBotClient _botClient;

        public StartCommandHandler(IUserService userService, ITelegramBotClient botClient)
        {
            _userService = userService;
            _botClient = botClient;
        }

        public async Task HandleAsync(Message message, CancellationToken ct)
        {
            var tgUser = message.From;
            if (tgUser == null) return;

            var dto = new UserDTO(
                TelegramId: tgUser.Id,
                Username: tgUser.Username,
                FirstName: tgUser.FirstName,
                LastName: tgUser.LastName
            );

            var user = await _userService.GetOrCreateUserAsync(dto, ct);

            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"Привет, {user.FirstName} ({user.Username}). Это бот для контакта с Ильей на время его пребывания в армии. Чтобы получать рассылку и задавать вопросы ты должен подать заявку, которую рассмотрит админ. Список команд по команде /help",
                cancellationToken: ct
            );
        }
    }
}