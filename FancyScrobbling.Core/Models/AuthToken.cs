using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace FancyScrobbling.Core.Models
{
    public class AuthToken
    {
        public int Id { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
