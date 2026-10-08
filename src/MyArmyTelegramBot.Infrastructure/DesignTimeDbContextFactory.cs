using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyArmyTelegramBot.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext> // Укажи свое имя контекста
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Локальная строка только для генерации файлов миграций
        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=MyArmyBotDb;Username=postgres;Password=123");

        return new AppDbContext(optionsBuilder.Options);
    }
}
