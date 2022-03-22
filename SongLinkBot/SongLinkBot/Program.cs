using System.Text.RegularExpressions;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
                if (update.Message?.Text?.Contains("open.spotify.com/track", StringComparison.OrdinalIgnoreCase) == true)
                {
                    Console.WriteLine($"Received Spotify link from chat {update.Message.Chat.Username} ({update.Message.Chat.Id}): {update.Message?.Text}");

                    string? spotifyId = default;
                    try
                    {
                        Uri uri = new Uri(update.Message?.Text);
                        spotifyId = uri.Segments[2];
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Received error parsing link: {ex}");
                    }

                    if (!string.IsNullOrEmpty(spotifyId))
                    {
                        await ScrapeSongLink(botClient, update.Message.Chat.Id, $"https://song.link/s/{spotifyId}", cancellationToken);
                    }

                    Console.WriteLine($"Finished processing {update.Message?.Text}{Environment.NewLine}");
                }
                else if (update.Message?.Text?.Contains("music.youtube.com/watch", StringComparison.OrdinalIgnoreCase) == true)
                {
                    Console.WriteLine($"Received YouTube Music link from chat {update.Message.Chat.Username} ({update.Message.Chat.Id}): {update.Message?.Text}");

                    string? youtubeId = YoutubeIdRegex.Match(new Uri(update.Message?.Text).Query).Groups.OfType<Capture>().Skip(1).FirstOrDefault()?.Value;

                    if (string.IsNullOrEmpty(youtubeId))
                    {
                        Console.WriteLine("Unable to get YouTube ID from URL.");
                    }
                    else
                    {
                        await ScrapeSongLink(botClient, update.Message.Chat.Id, $"https://song.link/y/{youtubeId}", cancellationToken);
                    }

                    Console.WriteLine($"Finished processing {update.Message?.Text}{Environment.NewLine}");
                }
            }
        }

        private static async Task ScrapeSongLink(ITelegramBotClient botClient, long chatId, string songLink, CancellationToken cancellationToken)
        {
            string songLinkPageContents = await HttpClient.GetStringAsync(songLink, cancellationToken);

            if (songLinkPageContents.Contains("\"statusCode\":404", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Unable to find any results for Song Link: {songLink}");
                return;
            }
            
            string youtubeMusicLink = YoutubeMusicRegex.Match(songLinkPageContents).Value;
            if (string.IsNullOrEmpty(youtubeMusicLink))
            {
                Console.WriteLine($"Couldn't get YouTube Music link for {songLink}");
            }

            string? spotifyLink = SpotifyRegex.Match(songLinkPageContents).Groups.OfType<Capture>().Skip(1).FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(youtubeMusicLink))
            {
                Console.WriteLine($"Couldn't get Spotify link for {songLink}");
            }

            string? albumArtLink = HttpUtility.UrlDecode(AlbumArtRegex.Match(songLinkPageContents).Groups.OfType<Capture>().Skip(1).FirstOrDefault()?.Value);
            if (string.IsNullOrEmpty(albumArtLink))
            {
                Console.WriteLine($"Couldn't get Album Art link for {songLink}");
            }

            string? songName = SongNameRegex.Match(songLinkPageContents).Groups.OfType<Capture>().Skip(1).FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(songName))
            {
                Console.WriteLine($"Couldn't get Song name for {songLink}");
            }

            await PublishMessage(botClient, chatId, songName, albumArtLink, spotifyLink, youtubeMusicLink, songLink, cancellationToken);
        }

        private static async Task PublishMessage(ITelegramBotClient botClient, long chatId, string songName, string albumArtLink, string spotifyLink, string youtubeMusicLink, string allLinks, CancellationToken cancellationToken)
        {
            List<string> links = new List<string>();

            if (!string.IsNullOrEmpty(albumArtLink))
            {
                links.Add($"[Album Art]({albumArtLink})");
            }

            if (!string.IsNullOrEmpty(spotifyLink))
            {
                links.Add($"[Spotify]({spotifyLink})");
            }

            if (!string.IsNullOrEmpty(youtubeMusicLink))
            {
                links.Add($"[YouTube Music]({youtubeMusicLink})");
            }

            if (!string.IsNullOrEmpty(allLinks))
            {
                links.Add($"[All Links]({allLinks})");
            }

            string message = string.IsNullOrEmpty(songName) ? string.Join(" | ", links) : $"*{songName}*{Environment.NewLine}{Environment.NewLine}{string.Join(" | ", links)}";

            await botClient.SendTextMessageAsync(chatId, message, parseMode: ParseMode.Markdown, disableWebPagePreview: false, cancellationToken: cancellationToken);
        }

        private static TelegramBotClient? _botClient;

        private static readonly HttpClient HttpClient = new();

        private static readonly Regex YoutubeMusicRegex = new(@"https:\/\/music.youtube.com\/watch\?v=.{11}");

        private static readonly Regex SpotifyRegex = new("(https:\\/\\/open\\.spotify\\.com.*?)\\\"");

        private static readonly Regex AlbumArtRegex = new("imagesrcset=\"\\/_next\\/image\\?url=(https.*?)&");

        private static readonly Regex SongNameRegex = new(@"<title>(.*?)<\/title>");

        private static readonly Regex YoutubeIdRegex = new("=(.*?)(&|\\z)");
    }
}