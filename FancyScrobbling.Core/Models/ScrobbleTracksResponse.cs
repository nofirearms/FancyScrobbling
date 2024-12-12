using FancyScrobbling.Core.Converters;
using FancyScrobbling.Core.Temp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FancyScrobbling.Core.Models
{
    public class ScrobbleTracksResponse
    {
        [JsonPropertyName("scrobbles")]
        public TrackScrobbles Response { get; set; }

        [JsonPropertyName("message")]
        public string ErrorMessage { get; set; }
        [JsonPropertyName("error")]
        public int ErrorNum { get; set; }
    }

    public class TrackScrobbles
    {
        [JsonPropertyName("scrobble")]
        [JsonConverter(typeof(SingleOrArrayConverter))]
        public List<Scrobble> Scrobbles { get; set; }

        [JsonPropertyName("@attr")]
        public Attributes Attributes { get; set; }
    }
    public class Attributes
    {
        [JsonPropertyName("ignored")]
        public int Ignored { get; set; }
        [JsonPropertyName("accepted")]
        public int Accepted { get; set; }
    }
    public class Scrobble
    {
        [JsonPropertyName("artist")]
        public Artist Artist { get; set; }
        [JsonPropertyName("album")]
        public Album Album { get; set; }
        [JsonPropertyName("track")]
        public Track Track { get; set; }
        [JsonPropertyName("ignoredMessage")]
        public Ignoredmessage Ignoredmessage { get; set; }
        [JsonPropertyName("albumArtist")]
        public Albumartist Albumartist { get; set; }
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }
    }
    public class Artist
    {
        [JsonPropertyName("corrected")]
        public string Corrected { get; set; }
        [JsonPropertyName("#text")]
        public string Text { get; set; }
    }

    public class Album
    {
        [JsonPropertyName("corrected")]
        public string Corrected { get; set; }
        [JsonPropertyName("#text")]
        public string Text { get; set; }
    }

    public class Track
    {
        [JsonPropertyName("corrected")]
        public string Corrected { get; set; }
        [JsonPropertyName("#text")]
        public string Text { get; set; }
    }

    public class Ignoredmessage
    {
        [JsonPropertyName("corrected")]
        public string Corrected { get; set; }
        [JsonPropertyName("#text")]
        public string Text { get; set; }
    }

    public class Albumartist
    {
        [JsonPropertyName("corrected")]
        public string Corrected { get; set; }
        [JsonPropertyName("#text")]
        public string Text { get; set; }
    }

}
