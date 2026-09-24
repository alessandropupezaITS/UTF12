using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using ProgettoUSF12.BackEnd.Services;
namespace ProgettoUSF12.BackEnd.Classi
{
    public class Razza : ISwapiEntity
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("classification")]
        public string Classification { get; set; }

        [JsonPropertyName("average_height")]
        public string AverageHeight { get; set; }

        [JsonPropertyName("average_lifespan")]
        public string AverageLifespan { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("homeworld"), JsonConverter(typeof(SwapiIdConverter))]
        public int HomeworldId { get; set; }

        [JsonPropertyName("people"), JsonConverter(typeof(SwapiIdListConverter))]
        public List<int> PersonaggioIds { get; set; } = new();

        [JsonPropertyName("films"), JsonConverter(typeof(SwapiIdListConverter))]
        public List<int> FilmIds { get; set; } = new();
        public string? MainImage { get; set; }
        public List<string> ImageGallery { get; set; } = new();
    }
}
