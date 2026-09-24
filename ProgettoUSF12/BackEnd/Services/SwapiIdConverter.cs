using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgettoUSF12.BackEnd.Services
{
    public class SwapiIdConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => ExtractId(reader.GetString());

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            => writer.WriteNumberValue(value);

        public static int ExtractId(string url)
        {
            if (string.IsNullOrEmpty(url)) return 0;
            var trimmed = url.TrimEnd('/');
            var lastSegment = trimmed.Substring(trimmed.LastIndexOf('/') + 1);
            return int.TryParse(lastSegment, out var id) ? id : 0;
        }
    }

    public class SwapiIdListConverter : JsonConverter<List<int>>
    {
        public override List<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = new List<int>();
            if (reader.TokenType != JsonTokenType.StartArray) return list;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                list.Add(SwapiIdConverter.ExtractId(reader.GetString()));

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<int> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var id in value) writer.WriteNumberValue(id);
            writer.WriteEndArray();
        }
    }

}