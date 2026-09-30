using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgettoUSF12.BackEnd.DbModels
{
    [Table("Personaggi")]
    public class PersonaggioDbModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Height { get; set; } = string.Empty;
        public string Mass { get; set; } = string.Empty;
        public string BirthYear { get; set; } = string.Empty;
        public string EyeColor { get; set; } = string.Empty;
        public string HairColor { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int HomeworldId { get; set; }

        public List<int> SpeciesIds { get; set; } = new();
        public List<int> FilmIds { get; set; } = new();
        public List<int> StarshipIds { get; set; } = new();

        public string? MainImage { get; set; }
        public List<string> ImageGallery { get; set; } = new();
    }
}