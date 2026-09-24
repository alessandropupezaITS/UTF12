using ProgettoUSF12.BackEnd.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
namespace ProgettoUSF12.BackEnd.Classi
{
    public class Personaggio : ISwapiEntity
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("height")]
        public string Height { get; set; }

        [JsonPropertyName("mass")]
        public string Mass { get; set; }

        [JsonPropertyName("birth_year")]
        public string BirthYear { get; set; }

        [JsonPropertyName("eye_color")]
        public string EyeColor { get; set; }

        [JsonPropertyName("hair_color")]
        public string HairColor { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("homeworld"), JsonConverter(typeof(SwapiIdConverter))]
        public int HomeworldId { get; set; }

        [JsonPropertyName("species"), JsonConverter(typeof(SwapiIdListConverter))]
        public List<int> SpeciesIds { get; set; } = new();

        [JsonPropertyName("films"), JsonConverter(typeof(SwapiIdListConverter))]
        public List<int> FilmIds { get; set; } = new();

        [JsonPropertyName("starships"), JsonConverter(typeof(SwapiIdListConverter))]
        public List<int> StarshipIds { get; set; } = new();

        public string? MainImage { get; set; }
        public List<string> ImageGallery { get; set; } = new();
    }

}
