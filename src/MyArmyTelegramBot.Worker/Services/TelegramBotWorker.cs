using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using MyArmyTelegramBot.Worker.Handlers;

namespace MyArmyTelegramBot.Worker;

public class TelegramBotWorker : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelegramBotWorker> _logger;

    public TelegramBotWorker(
        ITelegramBotClient botClient,
        IServiceProvider serviceProvider,
        ILogger<TelegramBotWorker> logger)
    {
        _botClient = botClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation("Бот {Username} успешно запущен!", me.Username);

        // Старт прослушивания обновлений
        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            cancellationToken: stoppingToken
        );

        await Task.Delay(-1, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        // Создаем Scope, чтобы передать Scoped-зависимости (DbContext, Handlers) в Singleton-воркер
        using var scope = _serviceProvider.CreateScope();
        var updateHandler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();

        // Вызываем оригинальный метод HandleUpdateAsync
        await updateHandler.HandleUpdateAsync(botClient, update, ct);
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
    {
        _logger.LogError(exception, "Ошибка при обработке Telegram API");
        return Task.CompletedTask;
    }
}