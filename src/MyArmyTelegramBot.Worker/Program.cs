using Microsoft.EntityFrameworkCore;
using MyArmyTelegramBot.Application.Interfaces.Repositories;
using MyArmyTelegramBot.Application.Interfaces.Services;
using MyArmyTelegramBot.Application.Services;
using MyArmyTelegramBot.Infrastructure;
using MyArmyTelegramBot.Infrastructure.Repositories;
using MyArmyTelegramBot.Worker.Handlers;
using MyArmyTelegramBot.Worker.Handlers.Commands;
using MyArmyTelegramBot.Worker.Handlers.Commands.Admin;
using Telegram.Bot;

namespace MyArmyTelegramBot.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        DotNetEnv.Env.Load();

        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        // 1. Конфигурация PostgreSQL и DbContext
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // 2. Регистрация TelegramBotClient
        var botToken = builder.Configuration["TelegramBot:Token"]
            ?? throw new InvalidOperationException("Telegram Bot Token is not configured!");

        builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

        // 3. Регистрация AutoMapper (передаём тип из сборки с профилями маппинга)
        builder.Services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));

        // 4. Регистрация Репозиториев (Infrastructure)
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

        // 5. Регистрация Сервисов (Application)
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IQuestionService, QuestionService>();

        // 6. Регистрация Роутера и Хэндлеров Команд (Worker)
        builder.Services.AddTransient<UpdateHandler>();
        builder.Services.AddTransient<StartCommandHandler>();
        builder.Services.AddTransient<ApplyCommandHandler>();
        builder.Services.AddTransient<ApproveCommandHandler>();
        builder.Services.AddTransient<QuestionCommandHandler>();
        builder.Services.AddTransient<CountdownCommandHandler>();

        builder.Services.AddTransient<HelpCommandHandler>();
        builder.Services.AddTransient<BroadcastCommandHandler>();
        builder.Services.AddTransient<UsersListCommandHandler>();
        builder.Services.AddTransient<QuestionsListCommandHandler>();

        // 7. Регистрация фонового сервиса
        builder.Services.AddHostedService<TelegramBotWorker>();

        var host = builder.Build();
        host.Run();
    }
}