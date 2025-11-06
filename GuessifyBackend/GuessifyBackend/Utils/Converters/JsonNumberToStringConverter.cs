using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GuessifyBackend.Utils.Converters
{
    public class JsonNumberToStringConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.Number) {
                var doc = JsonDocument.ParseValue(ref reader);
                return doc.RootElement.GetRawText() ?? string.Empty;
            }
            else return reader.GetString();
            
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
        }
    }
}
