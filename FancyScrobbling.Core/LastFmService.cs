using FancyScrobbling.Core.Database;
using FancyScrobbling.Core.Models;
using MediaDevices;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Core
{

    public class LastFmService
    {
        private readonly HttpClient _client;
        private readonly DataManager _data;
        private Session _session;
        private AuthToken _token;

        //получаем эти параметры после регистрации приложения на сайте
        private const string API_KEY = "c928ef6276d3f61383a255d94ce6cf52";
        private const string SHARED_SECRET = "090e6b5b1d5930bfbf40ded01f66bc09";

        public LastFmService()
        {
            _client = new HttpClient();
            _data = new DataManager();
        }

        /// <summary>
        /// Получаем токен для авторизации, срок службы токена 60 минут
        /// </summary>
        /// <returns></returns>
        public async Task<AuthToken> GetToken()
        {
            var response = await _client.GetAsync(new Uri($"http://ws.audioscrobbler.com/2.0/?method=auth.gettoken&api_key={API_KEY}&format=json"));
            var token = await response.Content.ReadFromJsonAsync<AuthToken>();
            await _data.AuthTokenRepository.SetAuthTokenAsync(token);
            _token = token;
            return token;
        }

        /// <summary>
        /// Открываем браузер, подтверждаем на сайте разрешение для приложения
        /// </summary>
        public void GivePermissionInBrowser()
        {
            Helpers.OpenUrlInBrowser($"http://www.last.fm/api/auth/?api_key={API_KEY}&token={_token.Token}");
        }

        public Session GetSessionFromDb()
        {
            _session = _data.SessionTokenRepository.GetSessionToken();
            return _session;
        }

        /// <summary>
        /// Получаем токен сессии, срок службы токена неограниченный
        /// </summary>
        /// <returns></returns>
        public async Task<Session> GetSession()
        {
            //_session = _data.SessionTokenRepository.GetSessionToken();
            //if (_session != null) 
            //{
            //    return _session;
            //}

            var signature = Helpers.CreateMD5FromString($"api_key{API_KEY}methodauth.getSessiontoken{_token.Token}{SHARED_SECRET}");
            //format=json для того чтобы получать ответ в json формате, иначе будет xml
            var response = await _client.GetAsync(new Uri($"http://ws.audioscrobbler.com/2.0/?method=auth.getSession&api_sig={signature}&api_key={API_KEY}&token={_token.Token}&format=json"));
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Can't get session response");
            }
            var session = await response.Content.ReadFromJsonAsync<FastFmSession>();
            await _data.SessionTokenRepository.SetSessionTokenAsync(session.Session);
            //TODO запилить обработку ошибок
            _session = session.Session;
            return _session;
        }

        public async Task<bool> ScrobbleTracks(MediaDevice device, List<ScrobbleTrack> files, int batchSize = 50)
        {
            var current_batch = 0;
            while(current_batch * batchSize < files.Count)
            {
                var batch = files.Skip(current_batch * batchSize).Take(batchSize).ToList();

                var scrobble_result = await ScrobbleTracks(batch);

                if (!scrobble_result)
                {
                    Debug.WriteLine("Error while scrobbling tracks");
                    return false;
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
            return true;
        }

        /// <summary>
        /// Скробблим трэки
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public async Task<bool> ScrobbleTracks(List<ScrobbleTrack> files)
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
                values.Add($"timestamp[{i}]", (Helpers.GetTimeStamp(DateTime.UtcNow) - i*60).ToString());
            }

            var signature = Helpers.CreateMD5FromString(GetApiSignature(values, SHARED_SECRET));

            values.Add("api_sig", signature);
            values.Add("format", "json");

            var content = new FormUrlEncodedContent(values);
            var response = await _client.PostAsync(new Uri("http://ws.audioscrobbler.com/2.0/"), content);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Can't scrobble");
                return false;
            }
            //var response_content = await response.Content.ReadAsStringAsync();
            var response_content = await response.Content.ReadFromJsonAsync<ScrobbleTracksResponse>();
            if(response_content is null)
            {
                Debug.WriteLine("Can't read content");
                return false;
            }
            if(response_content.Response is null)
            {
                Debug.WriteLine("Response is null");
                return false;
            }
            if(response_content.ErrorMessage != null)
            {
                Debug.WriteLine(response_content.ErrorMessage);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Получаем api_sig необходимый для запросов
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        private static string GetApiSignature(Dictionary<string, string> dictionary, string secret)
        {
            var ordered = dictionary.OrderBy(o => o.Key);
            var result = "";
            foreach (var pair in ordered)
            {
                result += pair.Key; result += pair.Value;
            }
            result += secret;
            return result;
        }
    }
}
