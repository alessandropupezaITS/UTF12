namespace ProgettoUSF12.BackEnd.Classi
{
    // Un film + i NOMI delle entità collegate (già risolti da ID a testo),
    // pronto da mostrare in PaginaFilm senza altri calcoli nel XAML.
    public class DettaglioFilm
    {
        public Film Film { get; set; } = new();

        public string Personaggi { get; set; } = string.Empty;
        public string Pianeti { get; set; } = string.Empty;
        public string Razze { get; set; } = string.Empty;
        public string Astronavi { get; set; } = string.Empty;
    }
}
