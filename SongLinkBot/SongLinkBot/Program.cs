using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace SongLinkBot
{
    class Program
    {
        static void Main()
        {
            string? botTokenEnv = Environment.GetEnvironmentVariable("BOT_TOKEN");
            if (string.IsNullOrEmpty(botTokenEnv))
            {
                Console.WriteLine("Error retrieving Telegram bot token. Be sure to set BOT_TOKEN environment variable. " +
                                  "If running from Visual Studio, set the env vars in settings.env");
                return;
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            _botClient = new TelegramBotClient(botTokenEnv);
            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, new ReceiverOptions(), cancellationTokenSource.Token);

            // Handle graceful shutdown
            AppDomain.CurrentDomain.ProcessExit += (_, __) =>
            {
                Console.WriteLine("SongLinkBot is gracefully shutting down.");
                cancellationTokenSource.Cancel();
            };

            Console.WriteLine("SongLinkBot is running.");

            Thread.Sleep(int.MaxValue);
        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Received exception: {exception}");
            
            return Task.CompletedTask;
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message != null)
            {
                // Try to parse link

                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Hello", cancellationToken: cancellationToken);
            }
        }

        private static TelegramBotClient? _botClient;
    }
}