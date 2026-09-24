using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using ProgettoUSF12.BackEnd.Services;

namespace ProgettoUSF12.BackEnd.Classi
{
    public class Astronave : ISwapiEntity
    {
        public int Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;


        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("starship_class")]
        public string StarshipClass { get; set; }

        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; }

        [JsonPropertyName("crew")]
        public string Crew { get; set; }

        [JsonPropertyName("passengers")]
        public string Passengers { get; set; }

        [JsonPropertyName("MGLT")]
        public string Mglt { get; set; }

        [JsonPropertyName("hyperdrive_rating")]
        public string HyperdriveRating { get; set; }

        [JsonPropertyName("films"), JsonConverter(typeof(SwapiIdListConverter))]
        public List<int> FilmIds { get; set; } = new();

        [JsonPropertyName("pilots"), JsonConverter(typeof(SwapiIdListConverter))]
        public List<int> PilotIds { get; set; } = new();
        public string? MainImage { get; set; }
        public List<string> ImageGallery { get; set; } = new();
    }
}
