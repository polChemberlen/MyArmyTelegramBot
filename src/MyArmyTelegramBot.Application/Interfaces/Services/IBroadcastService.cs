namespace MyArmyTelegramBot.Application.Interfaces.Services
{
    public interface IBroadcastService
    {
        Task BroadcastAsync(string messageText, CancellationToken ct);
    }
}
