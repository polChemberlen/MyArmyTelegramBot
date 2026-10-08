using MyArmyTelegramBot.Application.Interfaces.Services;
using MyArmyTelegramBot.Domain.Enums;
using MyArmyTelegramBot.Worker.Handlers.Commands;
using MyArmyTelegramBot.Worker.Handlers.Commands.Admin;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyArmyTelegramBot.Worker.Handlers
{
    public class UpdateHandler
    {
        private readonly StartCommandHandler _startCommandHandler;
        private readonly ApplyCommandHandler _applyCommandHandler;
        private readonly QuestionCommandHandler _questionCommandHandler;
        private readonly ApproveCommandHandler _approveCommandHandler;
        private readonly UsersListCommandHandler _usersListCommandHandler;
        private readonly QuestionsListCommandHandler _questionsListCommandHandler;
        private readonly HelpCommandHandler _helpCommandHandler;
        private readonly BroadcastCommandHandler _broadcastCommandHandler;
        private readonly CountdownCommandHandler _countdownCommandHandler;

        private readonly IUserService _userService;

        public UpdateHandler(
            StartCommandHandler startCommandHandler,
            IUserService userService,
            ApplyCommandHandler applyCommandHandler,
            QuestionCommandHandler questionCommandHandler,
            ApproveCommandHandler approveCommandHandler,
            UsersListCommandHandler usersListCommandHandler,
            QuestionsListCommandHandler questionsListCommandHandler,
            HelpCommandHandler helpCommandHandler,
            BroadcastCommandHandler broadcastCommandHandler,
            CountdownCommandHandler countdownCommandHandler)
        {
            _startCommandHandler = startCommandHandler;
            _userService = userService;
            _applyCommandHandler = applyCommandHandler;
            _questionCommandHandler = questionCommandHandler;
            _approveCommandHandler = approveCommandHandler;
            _usersListCommandHandler = usersListCommandHandler;
            _questionsListCommandHandler = questionsListCommandHandler;
            _helpCommandHandler = helpCommandHandler;
            _broadcastCommandHandler = broadcastCommandHandler;
            _countdownCommandHandler = countdownCommandHandler;
        }

        private async Task<bool> IsUserAdminAsync(long telegramId, CancellationToken ct)
        {
            var user = await _userService.GetByTelegramIdAsync(telegramId, ct);
            return user != null && user.Role == UserRole.Admin;
        }

        private static bool IsAdminCommand(string command) => command switch
        {
            "/broadcast" or "/approve" or "/reject" or "/users" or "/questions" => true,
            _ => false
        };

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            // 1. Обработка всех нажатий на Inline-кнопки (CallbackQuery)
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                var callback = update.CallbackQuery;
                var data = callback.Data;

                if (!string.IsNullOrEmpty(data))
                {
                    // А. Пагинация списка вопросов
                    if (data.StartsWith("qpage_"))
                    {
                        if (int.TryParse(data.Replace("qpage_", ""), out int targetPage))
                        {
                            await _questionsListCommandHandler.HandleAsync(
                                chatId: callback.Message!.Chat.Id,
                                page: targetPage,
                                messageIdToEdit: callback.Message.MessageId,
                                ct: ct
                            );

                            await botClient.AnswerCallbackQuery(
                                callbackQueryId: callback.Id,
                                cancellationToken: ct
                            );
                        }
                        return;
                    }

                    // Б. Одобрение / отклонение заявок через ApproveCommandHandler
                    if (data.StartsWith("approve_") || data.StartsWith("reject_"))
                    {
                        // Передаем callback в твои команды одобрения
                        await _approveCommandHandler.HandleCallbackAsync(callback, ct);

                        // Гасим вечную загрузку на кнопке
                        await botClient.AnswerCallbackQuery(
                            callbackQueryId: callback.Id,
                            cancellationToken: ct
                        );
                        return;
                    }

                    // В блоке обработки CallbackQuery:
                    if (data != null && data.StartsWith("upage_"))
                    {
                        if (int.TryParse(data.Replace("upage_", ""), out int targetPage))
                        {
                            await _usersListCommandHandler.HandleAsync(
                                chatId: callback.Message!.Chat.Id,
                                page: targetPage,
                                messageIdToEdit: callback.Message.MessageId,
                                ct: ct
                            );

                            await botClient.AnswerCallbackQuery(
                                callbackQueryId: callback.Id,
                                cancellationToken: ct
                            );
                        }
                        return;
                    }
                }

                return;
            }

            // 2. Обработка текстовых команд
            if (update.Type != UpdateType.Message || update.Message?.Text == null) return;

            var message = update.Message;
            if (string.IsNullOrWhiteSpace(message.Text)) return;

            var tgUser = message.From;
            if (tgUser == null) return;

            var command = message.Text.Split(' ')[0].ToLower();

            if (IsAdminCommand(command) && !await IsUserAdminAsync(tgUser.Id, ct)) return;

            if (command == "/ask")
            {
                var user = await _userService.GetByTelegramIdAsync(tgUser.Id, ct);
                if (user == null || user.Status != UserApplicationStatus.Approved)
                {
                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Отправлять вопросы могут только одобренные пользователи. Подайте заявку через /apply.",
                        cancellationToken: ct
                    );
                    return;
                }
            }

            var task = command switch
            {
                "/start" => _startCommandHandler.HandleAsync(message, ct),
                "/apply" => _applyCommandHandler.HandleAsync(message, ct),
                "/ask" => _questionCommandHandler.HandleAsync(message, ct),
                "/help" => _helpCommandHandler.HandleAsync(message, ct),
                "/dmb" => _countdownCommandHandler.HandleAsync(message, ct),

                // Админские команды
                "/users" => _usersListCommandHandler.HandleAsync(message.Chat.Id, 1, null, ct),
                "/questions" => _questionsListCommandHandler.HandleAsync(message.Chat.Id, 1, null, ct),
                "/broadcast" => _broadcastCommandHandler.HandleAsync(message, ct),

                _ => Task.CompletedTask
            };

            await task;
        }
    }
}
