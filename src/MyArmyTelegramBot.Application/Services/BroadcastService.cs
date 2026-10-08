using MyArmyTelegramBot.Application.Interfaces.Repositories;
using MyArmyTelegramBot.Application.Interfaces.Services;
using MyArmyTelegramBot.Domain.Enums;
using Telegram.Bot;

namespace MyArmyTelegramBot.Application.Services
{
    public class BroadcastService : IBroadcastService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITelegramBotClient _botClient;

        public BroadcastService(
            IUserRepository userRepository,
            ITelegramBotClient botClient)
        {
            _userRepository = userRepository;
            _botClient = botClient;
        }

        public async Task BroadcastAsync(string messageText, CancellationToken ct)
        {
            var approvedUsers = await _userRepository.GetAllByRoleAsync(UserRole.ApprovedUser, ct);
            var admins = await _userRepository.GetAllByRoleAsync(UserRole.Admin, ct);

            var recipients = approvedUsers.Concat(admins).DistinctBy(u => u.TelegramId);

            foreach (var user in recipients)
            {
                try
                {
                    await _botClient.SendMessage(
                        chatId: user.TelegramId,
                        text: messageText,
                        cancellationToken: ct
                    );
                }
                catch
                {

                }
            }
        }
    }
}
