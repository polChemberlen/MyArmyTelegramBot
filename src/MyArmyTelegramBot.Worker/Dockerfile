# 1. Этап сборки (SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем csproj-файлы для кеширования слоев
COPY ["MyArmyTelegramBot.Worker/MyArmyTelegramBot.Worker.csproj", "MyArmyTelegramBot.Worker/"]
COPY ["MyArmyTelegramBot.Application/MyArmyTelegramBot.Application.csproj", "MyArmyTelegramBot.Application/"]
COPY ["MyArmyTelegramBot.Domain/MyArmyTelegramBot.Domain.csproj", "MyArmyTelegramBot.Domain/"]
COPY ["MyArmyTelegramBot.Infrastructure/MyArmyTelegramBot.Infrastructure.csproj", "MyArmyTelegramBot.Infrastructure/"]

# Восстанавливаем пакеты
RUN dotnet restore "MyArmyTelegramBot.Worker/MyArmyTelegramBot.Worker.csproj"

# Копируем весь исходный код
COPY . .

# Переходим в папку воркера и публикуем релизный билд в /app/publish
WORKDIR "/src/MyArmyTelegramBot.Worker"
RUN dotnet publish "MyArmyTelegramBot.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Этап выполнения (Runtime)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

ENV TZ=Asia/Yekaterinburg

# Копируем скомпилированные файлы из этапа build
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyArmyTelegramBot.Worker.dll"]