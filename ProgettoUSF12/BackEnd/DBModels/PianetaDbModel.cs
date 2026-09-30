using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgettoUSF12.BackEnd.DbModels
{
    [Table("Pianeti")]
    public class PianetaDbModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Diameter { get; set; } = string.Empty;
        public string RotationPeriod { get; set; } = string.Empty;
        public string Gravity { get; set; } = string.Empty;
        public string Population { get; set; } = string.Empty;
        public string Climate { get; set; } = string.Empty;
        public string SurfaceWater { get; set; } = string.Empty;
        public string Terrain { get; set; } = string.Empty;

        public List<int> FilmIds { get; set; } = new();

        public string? MainImage { get; set; }
        public List<string> ImageGallery { get; set; } = new();
    }
}