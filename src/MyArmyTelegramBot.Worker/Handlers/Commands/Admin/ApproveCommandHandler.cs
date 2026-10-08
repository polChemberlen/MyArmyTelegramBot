using MyArmyTelegramBot.Application.Interfaces.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyArmyTelegramBot.Worker.Handlers.Commands.Admin
{
    public class ApproveCommandHandler
    {
        private readonly IUserService _userService;
        private readonly ITelegramBotClient _botClient;

        public ApproveCommandHandler(IUserService userService, ITelegramBotClient botClient)
        {
            _userService = userService;
            _botClient = botClient;
        }

        public async Task HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken ct)
        {
            if (callbackQuery.Data == null || callbackQuery.Message == null) return;

            var parts = callbackQuery.Data.Split('_');
            if (parts.Length < 2 || !long.TryParse(parts[1], out long targetTelegramId)) return;

            var action = parts[0];

            if (action == "approve")
            {
                await _userService.ApproveUserAsync(targetTelegramId, ct);

                // Уведомляем пользователя
                await _botClient.SendMessage(
                    chatId: targetTelegramId,
                    text: "🎉 Твоя заявка одобрена! Теперь тебе доступен полный функционал бота.",
                    cancellationToken: ct
                );

                // Обновляем сообщение в админке
                await _botClient.EditMessageText(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: callbackQuery.Message.Text + "\n ✅ Заявка OДОБРЕНА",
                    cancellationToken: ct
                );
            }
            else if (action == "reject")
            {
                // Уведомляем пользователя
                await _botClient.SendMessage(
                    chatId: targetTelegramId,
                    text: "❌ К сожалению, твоя заявка была отклонена. плаки плаки",
                    cancellationToken: ct
                );

                // Обновляем сообщение в админке
                await _botClient.EditMessageText(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: callbackQuery.Message.Text + "\n ❌ Заявка ОТКЛОНЕНА",
                    cancellationToken: ct
                );
            }

            // Гасим плашку загрузки на кнопке Telegram
            await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
        }
    }
}
