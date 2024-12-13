using FancyScrobbling.Core;
using FancyScrobbling.Core.Database;
using MediaDevices;
using System.Reflection.Metadata.Ecma335;
using static FancyScrobbling.Core.Helpers;

namespace FancyScrobblingConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ScrobbleTracksAsync().Wait();
        }

        private async static Task ScrobbleTracksAsync()
        {
            var animatedConsole = new AnimatedConsole();
            var deviceService = new DeviceService();
            var devices = deviceService.GetDevices();

            if (devices.Count == 0)
            {
                await animatedConsole.ConsoleAnimatedWriteLineAsync($"There are no connected MTP devices. Application will be terminated.");
                Console.ReadLine();
                return;
            }          
            for (int i = 0; i < devices.Count; i++)
            {
                await animatedConsole.ConsoleAnimatedWriteLineAsync($"{i} {devices[i].FriendlyName}");
            }
            await animatedConsole.ConsoleAnimatedWriteLineAsync("Choose a device");
            int.TryParse(Console.ReadLine(), out var device_number);
            if (device_number > devices.Count)
            {
                await animatedConsole.ConsoleAnimatedWriteLineAsync("Invalid device number");
                Console.ReadLine();
                return;
            }

            var device = devices[device_number];
            device.Connect();
            //var tracks_for_scrobbling = deviceService.GetUsedFiles(device);
            var tracks_for_scrobbling = await LoadingAsync(() => deviceService.GetScribbleFiles(device), "Loading Tracks");
            if (!tracks_for_scrobbling.Any())
            {
                await animatedConsole.ConsoleAnimatedWriteLineAsync("No tracks to scrobble.");
                Console.ReadLine();
                device.Disconnect();
                return;
            }
            foreach (var file in tracks_for_scrobbling)
            {

                await animatedConsole.ConsoleAnimatedWriteLineAsync($" {tracks_for_scrobbling.IndexOf(file)} {file.Artist} - {file.Title} | {file.Album}");
            }

            var lastfm = new LastFmService();

            lastfm.ProgressStatus += async (sender, status) =>
            {
                await animatedConsole.ConsoleAnimatedWriteLineAsync(status);
            };

            var token = await lastfm.GetToken();
            if(token is null)
            {
                await animatedConsole.ConsoleAnimatedWriteLineAsync("Cat's get a token. The application wille be terminated.");
                Console.ReadLine();
                return;
            }
            var session = lastfm.GetSessionFromDb();
            if(session is null)
            {
                lastfm.GivePermissionInBrowser();
                await animatedConsole.ConsoleAnimatedWriteLineAsync("Press Enter as you gave a permission to the application");
                Console.ReadLine();
                session = await lastfm.GetSession();
            }

            if (session is null)
            {
                await animatedConsole.ConsoleAnimatedWriteLineAsync("Can't take session token. Application will be terminated");
                Console.ReadLine();
                device.Disconnect();
                return;
            }

            await animatedConsole.ConsoleAnimatedWriteLineAsync($"Logged as {session.Name}");
            await animatedConsole.ConsoleAnimatedWriteLineAsync("Scrobble tracks?[y/n]", ConsoleWriteType.Write); 
            var scr = Console.ReadLine();
            if (scr != "y") return;

            var scrobble_result = await lastfm.ScrobbleTracks(device, tracks_for_scrobbling);
            if (scrobble_result)
            {
                await animatedConsole.ConsoleAnimatedWriteLineAsync($"Success! Scrobbled {tracks_for_scrobbling.Count} tracks.");
            }
            else
            {
                await animatedConsole.ConsoleAnimatedWriteLineAsync("Failure.");
            }

            device.Disconnect();
            Console.ReadLine();
        }

        private async static Task<T> LoadingAsync<T>(Func<T> func, string message = "Loading")
        {
            //Console.WriteLine(); 
            var loading = true;
            Task.Run(async () =>
            {
                while (true)
                {
                    if (!loading) return;
                    Console.Write($"\r{message}   ");
                    await Task.Delay(500);
                    if (!loading) return;
                    Console.Write($"\r{message}.  ");
                    await Task.Delay(500);
                    if (!loading) return;
                    Console.Write($"\r{message}.. ");
                    await Task.Delay(500);
                    if (!loading) return;
                    Console.Write($"\r{message}...");
                    await Task.Delay(500);
                }
            });
            var result = func();
            loading = false;
            Console.Write($"\r");
            Console.Write(new String(' ', Console.BufferWidth));
            return result;
        }
    }

    public class AnimatedConsole
    {
        private readonly Queue<string> _queue;
        private readonly Random _random;
        private bool _isWorking;

        public AnimatedConsole()
        {
            _queue = new Queue<string>();
            _random = new Random();
            _isWorking = false;
        }

        public async Task ConsoleAnimatedWriteLineAsync(string queueText, ConsoleWriteType consoleWriteType = ConsoleWriteType.WriteLine)
        {
            _queue.Enqueue(queueText);
            if (_isWorking) return;
            while (true)
            {
                _isWorking = true;

                if (_queue.Count == 0) break;

                var text = _queue.Dequeue();
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
#if DEBUG
                    
#else
                    await Task.Delay(_random.Next(10));
#endif

                }
                if (consoleWriteType == ConsoleWriteType.WriteLine) Console.WriteLine();
            }
            _isWorking = false;
        }
    }

    public enum ConsoleWriteType
    {
        Write, WriteLine
    }
}
