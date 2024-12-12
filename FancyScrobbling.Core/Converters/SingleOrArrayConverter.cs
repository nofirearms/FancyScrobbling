using FancyScrobbling.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;


namespace FancyScrobbling.Core.Converters
{
    internal class SingleOrArrayConverter : JsonConverter<List<Scrobble>>
    {
        public override List<Scrobble>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            
            var elements = new List<Scrobble>();
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    elements.Add(JsonSerializer.Deserialize<Scrobble>(ref reader, options)!);

                    reader.Read();
                }
            }
            else if(reader.TokenType == JsonTokenType.StartObject)
            {
                elements.Add(JsonSerializer.Deserialize<Scrobble>(ref reader, options)!);
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            return elements;
        }

        public override void Write(Utf8JsonWriter writer, List<Scrobble> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
