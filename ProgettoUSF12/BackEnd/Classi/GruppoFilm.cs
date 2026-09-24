using System.Collections.Generic;

namespace ProgettoUSF12.BackEnd.Classi
{
    public class GruppoFilm
    {
        public string Titolo { get; set; } = string.Empty;
        public List<Film> Film { get; set; } = new();
    }
}
