using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using ProgettoUSF12.BackEnd.Services;

namespace ProgettoUSF12.BackEnd.Classi
{
    public class Pianeta : ISwapiEntity
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("diameter")]
        public string Diameter { get; set; }

        [JsonPropertyName("rotation_period")]
        public string RotationPeriod { get; set; }

        [JsonPropertyName("gravity")]
        public string Gravity { get; set; }

        [JsonPropertyName("population")]
        public string Population { get; set; }

        [JsonPropertyName("climate")]
        public string Climate { get; set; }

        [JsonPropertyName("surface_water")]
        public string SurfaceWater { get; set; }

        [JsonPropertyName("terrain")]
        public string Terrain { get; set; }

        [JsonPropertyName("films"), JsonConverter(typeof(SwapiIdListConverter))]
        public List<int> FilmIds { get; set; } = new();

        public string? MainImage { get; set; }
        public List<string> ImageGallery { get; set; } = new();
    }
}
