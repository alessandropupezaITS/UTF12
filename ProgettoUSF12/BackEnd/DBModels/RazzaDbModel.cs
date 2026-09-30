using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgettoUSF12.BackEnd.DbModels
{
    [Table("Razze")]
    public class RazzaDbModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Classification { get; set; } = string.Empty;
        public string AverageHeight { get; set; } = string.Empty;
        public string AverageLifespan { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int HomeworldId { get; set; }

        public List<int> PersonaggioIds { get; set; } = new();
        public List<int> FilmIds { get; set; } = new();

        public string? MainImage { get; set; }
        public List<string> ImageGallery { get; set; } = new();
    }
}