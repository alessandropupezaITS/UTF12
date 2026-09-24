using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using ProgettoUSF12.BackEnd.Services;
namespace ProgettoUSF12.BackEnd.Classi
{
    public class Film : ISwapiEntity
    {
        public int Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("episode_id")]
        public int EpisodeId { get; set; }

        [JsonPropertyName("opening_crawl")]
        public string OpeningCrawl { get; set; }

        [JsonPropertyName("director")]
        public string Director { get; set; }

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; }

        [JsonPropertyName("characters"), JsonConverter(typeof(SwapiIdListConverter))]
        public List<int> PersonaggioIds { get; set; } = new();

        [JsonPropertyName("planets"), JsonConverter(typeof(SwapiIdListConverter))]
        public List<int> PianetaIds { get; set; } = new();

        [JsonPropertyName("species"), JsonConverter(typeof(SwapiIdListConverter))]
        public List<int> RazzaIds { get; set; } = new();

        [JsonPropertyName("starships"), JsonConverter(typeof(SwapiIdListConverter))]
        public List<int> AstronaveIds { get; set; } = new();

        // URL del poster recuperato da TMDB (se disponibile)
        public string? PosterUrl { get; set; }
    }
}
