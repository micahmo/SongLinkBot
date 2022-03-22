# Build and run locally
# nerdctl build -f .\SongLinkBot\Dockerfile . -t songlinkbot
# nerdctl run -e BOT_TOKEN="" songlinkbot -t songlinkbot

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SongLinkBot/SongLinkBot.csproj", "SongLinkBot/"]
RUN dotnet restore "SongLinkBot/SongLinkBot.csproj"
COPY . .
WORKDIR "/src/SongLinkBot"
RUN dotnet build "SongLinkBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SongLinkBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SongLinkBot.dll"]