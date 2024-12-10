using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FancyScrobbling.Core.Models
{
    public class FastFmSession
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("session")]
        public Session Session { get; set; }
    }

    public class Session 
    {
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("key")]
        public string Key { get; set; }
        [JsonPropertyName("subscriber")]
        public byte Subscriber { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }

}
