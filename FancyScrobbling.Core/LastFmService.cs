using FancyScrobbling.Core.Database;
using FancyScrobbling.Core.Models;
using MediaDevices;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net.Http.Json;

namespace FancyScrobbling.Core
{

    public class LastFmService
    {
        private readonly HttpClient _client;
        private readonly DataManager _data;
        private Session _session;
        private AuthToken _token;

        public event EventHandler<string> ProgressStatus;

        //получаем эти параметры после регистрации приложения на сайте
        private const string API_KEY = "c928ef6276d3f61383a255d94ce6cf52";
        private const string SHARED_SECRET = "090e6b5b1d5930bfbf40ded01f66bc09";

        public LastFmService()
        {
            _client = new HttpClient();
            _data = new DataManager();
        }

        public bool IsAuthorized => GetSessionFromDb() is null ? false : true;

        /// <summary>
        /// Получаем токен для авторизации, срок службы токена 60 минут
        /// </summary>
        /// <returns>AuthToken</returns>
        public async Task<Result> GetTokenAsync()
        {
            var response = await _client.GetAsync(new Uri($"http://ws.audioscrobbler.com/2.0/?method=auth.gettoken&api_key={API_KEY}&format=json"));
            if(!response.IsSuccessStatusCode)
            {
                return new Result(false, await response.Content.ReadAsStringAsync());
            }
            var token = await response.Content.ReadFromJsonAsync<AuthToken>();
            //await _data.AuthTokenRepository.SetAuthTokenAsync(token);
            _token = token;
            return new Result(true, token);
        }

        /// <summary>
        /// Открываем браузер, подтверждаем на сайте разрешение для приложения
        /// </summary>
        public void GivePermissionInBrowser()
        {
            Helpers.OpenUrlInBrowser($"http://www.last.fm/api/auth/?api_key={API_KEY}&token={_token.Token}");
        }

        /// <summary>
        /// Получаем временный токен, открываем браузер
        /// </summary>
        /// <returns>AuthToken</returns>
        public async Task<Result> GetAccountAccessAsync()
        {
            var token = await GetTokenAsync();
            if (token == null) return new Result(false, "Error. Couldn't get token.");

            GivePermissionInBrowser();
            return new Result(true, token);
        }

        public Session GetSessionFromDb()
        {
            _session = _data.SessionTokenRepository.GetSessionToken();
            return _session;
        }

        public async Task<bool> RemoveSessionFromDb()
        {
            return await _data.SessionTokenRepository.RemoveSessionToken();
        }

        /// <summary>
        /// Получаем токен сессии, срок службы токена неограниченный
        /// </summary>
        /// <returns>Session</returns>
        public async Task<Result> GetSessionAsync()
        {
            if (_token is null) return new Result(false, "Token is null");
            var signature = Helpers.CreateMD5FromString($"api_key{API_KEY}methodauth.getSessiontoken{_token.Token}{SHARED_SECRET}");
            //format=json для того чтобы получать ответ в json формате, иначе будет xml
            var response = await _client.GetAsync(new Uri($"http://ws.audioscrobbler.com/2.0/?method=auth.getSession&api_sig={signature}&api_key={API_KEY}&token={_token.Token}&format=json"));
            if (!response.IsSuccessStatusCode)
            {
                return new Result(false, await response.Content.ReadAsStringAsync());
            }
            var session = await response.Content.ReadFromJsonAsync<LastFmSession>();
            if(session.Session is null)
            {
                return new Result(false, "Session is null");
            }
            await _data.SessionTokenRepository.SetSessionTokenAsync(session.Session);
            //TODO запилить обработку ошибок
            _session = session.Session;
            return new Result(true, _session);
        }



        //TODO разобраться почему не отправляется больше 10 файлов за 1 раз, возможно что-то с сортировкой
        /// <summary>
        /// Returns source files in case of success
        /// </summary>
        /// <param name="device"></param>
        /// <param name="files"></param>
        /// <param name="batchSize"></param>
        /// <param name="secondsBetweenTracks"></param>
        /// <returns>ScrobbleTracks</returns>
        public async Task<Result> ScrobbleTracksAsync(MediaDevice device, List<ScrobbleTrack> files, int batchSize = 10, int secondsBetweenTracks = 30)
        {
            //расставляем timestamp в треки
            var time = DateTime.Now;
            for(int i = 0; i < files.Count; i++)
            {
                files[i].Timestamp = (Helpers.GetTimeStamp(DateTime.UtcNow) - i * secondsBetweenTracks).ToString();
            }

            var current_batch = 0;
            while(current_batch * batchSize < files.Count)
            {
                var batch = files.Skip(current_batch * batchSize).Take(batchSize).ToList();

                var scrobble_result = await ScrobbleTracksAsync(batch);

                if (!scrobble_result.Success)
                {
                    return new Result(false, scrobble_result.Parameter);
                }

                foreach(var scrobble_track in batch)
                {
                    var file_info = device.GetFileInfo(scrobble_track.Path);
                    if (file_info is null) continue;

                    var count = file_info.UseCount - 1;
                    file_info.UseCount = int.Max(0, (int)count);
                }

                current_batch++;
            }
            return new Result(true, files);
        }

        /// <summary>
        /// Скробблим треки
        /// </summary>
        /// <param name="files"></param>
        /// <returns>ScrobbleTracks</returns>
        public async Task<Result> ScrobbleTracksAsync(List<ScrobbleTrack> files)
        {

            var values = new Dictionary<string, string>
            {
                { "api_key", API_KEY },
                { "method", "track.scrobble" },
                { "sk", _session.Key },
            };

            for(int i = 0; i < files.Count(); i++)
            {
                if (string.IsNullOrEmpty(files[i].Artist) || string.IsNullOrEmpty(files[i].Title)) continue;

                values.Add($"artist[{i}]", files[i].Artist);
                values.Add($"track[{i}]", files[i].Title);
                values.Add($"album[{i}]", files[i].Album);
                values.Add($"timestamp[{i}]", files[i].Timestamp);
            }

            var signature = Helpers.CreateMD5FromString(GetApiSignature(values, SHARED_SECRET));

            values.Add("api_sig", signature);
            values.Add("format", "json");

            var content = new FormUrlEncodedContent(values);
            var response = await _client.PostAsync(new Uri("http://ws.audioscrobbler.com/2.0/"), content);

            var response_content = await response.Content.ReadFromJsonAsync<ScrobbleTracksResponse>();
            if(!response.IsSuccessStatusCode)
            {
                return new Result(false, $"Error: {response_content.ErrorNum} | {response_content.ErrorMessage}");
            }
            if (response_content.Response is null)
            {
                return new Result(false, $"Error: Response is null.");
            }
            if(response_content.ErrorMessage != null)
            {
                new Result(false, response_content.ErrorMessage);
            }
            return new Result(true, files);
        }
        /// <summary>
        /// Получаем api_sig необходимый для запросов
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="secret"></param>
        /// <returns>string</returns>
        private static string GetApiSignature(Dictionary<string, string> dictionary, string secret)
        {
            //var ordered = dictionary.OrderBy(o => o.Key);
            //custom comparer for natural order, (1, 2, 10) instead of (1, 10, 2)
            var comparer = new Helpers.NaturalStringComparer();
            var ordered = dictionary.OrderBy(o => o.Key, comparer);
            var result = "";
            foreach (var pair in ordered)
            {
                result += pair.Key; result += pair.Value;
            }
            result += secret;
            return result;
        }

        public bool RemoveScrobbleTracks(MediaDevice device, IEnumerable<ScrobbleTrack> tracks)
        {
            foreach (var scrobble_track in tracks)
            {
                var file_info = device.GetFileInfo(scrobble_track.Path);
                if (file_info is null) continue;

                var count = file_info.UseCount - 1;
                file_info.UseCount = int.Max(0, (int)count);
            }
            return true;
        }

    }
}
