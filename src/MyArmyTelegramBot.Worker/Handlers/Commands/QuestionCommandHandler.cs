using System.Net;
using Microsoft.Extensions.Configuration;
using MyArmyTelegramBot.Application.DTOs;
using MyArmyTelegramBot.Application.Interfaces.Services;
using MyArmyTelegramBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyArmyTelegramBot.Worker.Handlers.Commands
{
    public class QuestionCommandHandler
    {
        private readonly IQuestionService _questionService;
        private readonly IUserService _userService;
        private readonly ITelegramBotClient _botClient;
        private readonly List<long> _adminTelegramIds;

        public QuestionCommandHandler(
            IQuestionService questionService,
            ITelegramBotClient botClient,
            IUserService userService,
            IConfiguration configuration)
        {
            _questionService = questionService;
            _botClient = botClient;
            _userService = userService;

            // 1. Пробуем получить как список из JSON-массива
            var idsFromSection = configuration.GetSection("Admins:TelegramIds").Get<List<long>>();

            if (idsFromSection != null && idsFromSection.Any())
            {
                _adminTelegramIds = idsFromSection;
            }
            else
            {
                // 2. Если в конфиге/env лежала строка с запятыми (как в ApplyCommandHandler)
                var rawString = configuration["Admins:TelegramIds"] ?? string.Empty;
                _adminTelegramIds = rawString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(long.Parse)
                    .ToList();
            }
        }

        public async Task HandleAsync(Message message, CancellationToken ct)
        {
            var tgUser = message.From;
            if (tgUser == null || string.IsNullOrWhiteSpace(message.Text)) return;

            var user = await _userService.GetByTelegramIdAsync(tgUser.Id, ct);

            if (user == null || user.Status != UserApplicationStatus.Approved)
            {
                await _botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Отправлять вопросы могут только одобренные пользователи. Подай заявку с помощью /apply.",
                    cancellationToken: ct
                );
                return;
            }

            var parts = message.Text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var questionText = parts.Length > 1 ? parts[1].Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(questionText))
            {
                await _botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "⚠️ Укажите текст вопроса.\n\nПример:\n<code>/ask Когда следующий сбор?</code>",
                    parseMode: ParseMode.Html,
                    cancellationToken: ct
                );
                return;
            }

            var dto = new QuestionDTO(
                QuestionText: questionText,
                UserId: user.Id
            );

            await _questionService.CreateQuestionAsync(dto, ct);

            // 1. Подтверждение пользователю
            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "✅ Ваш вопрос успешно сохранен и отправлен администраторам!",
                parseMode: ParseMode.Html,
                cancellationToken: ct
            );

            // 2. Уведомление всем администраторам
            var safeQuestionText = WebUtility.HtmlEncode(questionText);
            var username = string.IsNullOrEmpty(user.Username) ? "без username" : $"@{user.Username}";

            Console.WriteLine($"[DEBUG] Число админов для уведомления: {_adminTelegramIds.Count}");

            foreach (var adminId in _adminTelegramIds)
            {
                try
                {
                    await _botClient.SendMessage(
                        chatId: adminId,
                        text: $"❓ <b>Новый вопрос!</b>\n\n" +
                              $"👤 <b>От:</b> {username} (<code>{tgUser.Id}</code>)\n" +
                              $"💬 <b>Текст:</b> {safeQuestionText}",
                        parseMode: ParseMode.Html,
                        cancellationToken: ct
                    );
                    Console.WriteLine($"[SUCCESS] Уведомление о вопросе отправлено админу {adminId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Не удалось отправить вопрос админу {adminId}: {ex.Message}");
                }
            }
        }
    }
}