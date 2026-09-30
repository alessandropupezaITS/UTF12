using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgettoUSF12.BackEnd.DbModels
{
    [Table("Film")]
    public class FilmDbModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Mantiene l'ID di SWAPI
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int EpisodeId { get; set; }
        public string OpeningCrawl { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public string ReleaseDate { get; set; } = string.Empty;
        public string? PosterUrl { get; set; }

        // Relazioni memorizzate come JSON String nel DB
        public List<int> PersonaggioIds { get; set; } = new();
        public List<int> PianetaIds { get; set; } = new();
        public List<int> RazzaIds { get; set; } = new();
        public List<int> AstronaveIds { get; set; } = new();
    }
}