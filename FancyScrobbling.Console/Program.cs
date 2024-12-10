using FancyScrobbling.Core;
using MediaDevices;
using System.Reflection.Metadata.Ecma335;

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
            var deviceService = new DeviceService();
            var devices = deviceService.GetDevices();

            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine($"{i} {devices[i].FriendlyName}");
            }
            Console.WriteLine("Choose a device");
            int.TryParse(Console.ReadLine(), out var device_number);
            if (device_number > devices.Count)
            {
                Console.WriteLine("Invalid device number");
                Console.ReadLine();
                return;
            }

            var device = devices[device_number];
            device.Connect();
            //var tracks_for_scrobbling = deviceService.GetUsedFiles(device);
            var tracks_for_scrobbling = await LoadingAsync(() => deviceService.GetScribbleFiles(device), "Loading Tracks");
            if (!tracks_for_scrobbling.Any())
            {
                Console.WriteLine("No tracks for scrobble.");
                Console.ReadLine();
                device.Disconnect();
                return;
            }
            foreach (var file in tracks_for_scrobbling)
            {

                Console.WriteLine($" {tracks_for_scrobbling.IndexOf(file)} {file.Artist} - {file.Title} | {file.Album}");
            }

            var lastfm = new LastFmService();
            var token = await lastfm.GetToken();
            var session = lastfm.GetSessionFromDb();
            if(session is null)
            {
                lastfm.GivePermissionInBrowser();
                Console.WriteLine("Press Enter as you gave a permission to the application");
                Console.ReadLine();
                session = await lastfm.GetSession();
            }

            if (session is null)
            {
                Console.WriteLine("Can't take session token. Application will be terminated");
                Console.ReadLine();
                device.Disconnect();
                return;
            }

            Console.WriteLine($"Logged as {session.Name}");
            Console.Write("Scrobble tracks?[y/n]"); 
            var scr = Console.ReadLine();
            if (scr != "y") return;

            var scrobble_result = await LoadingAsync(async() => await lastfm.ScrobbleTracks(device, tracks_for_scrobbling), "Scrobbling").Result;
            if (scrobble_result)
            {
                Console.WriteLine($"Success! Scrobbled {tracks_for_scrobbling.Count} tracks.");
            }
            else
            {
                Console.WriteLine("Failure.");
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
}
