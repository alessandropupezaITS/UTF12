using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgettoUSF12.BackEnd.DbModels
{
    [Table("Astronavi")]
    public class AstronaveDbModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string StarshipClass { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Crew { get; set; } = string.Empty;
        public string Passengers { get; set; } = string.Empty;
        public string Mglt { get; set; } = string.Empty;
        public string HyperdriveRating { get; set; } = string.Empty;

        public List<int> FilmIds { get; set; } = new();
        public List<int> PilotIds { get; set; } = new();

        public string? MainImage { get; set; }
        public List<string> ImageGallery { get; set; } = new();
    }
}