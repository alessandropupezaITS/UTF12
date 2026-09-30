using ProgettoUSF12.BackEnd.Classi;
using ProgettoUSF12.BackEnd.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ProgettoUSF12
{
    // Scheda "neutra": qualunque entità (personaggio, pianeta, razza, astronave)
    // viene convertita in questa, così il XAML è uno solo.
    public class SchedaDettaglio
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string? Immagine { get; set; }
        public string Riga1 { get; set; } = "";
        public string Riga2 { get; set; } = "";
        public string Riga3 { get; set; } = "";
        public int NumeroGalleria { get; set; }
    }

    // Conversione entità -> scheda e caricamento per Id.
    // Condivisa da PaginaFilm (righe di schede) e da PaginaDettaglio (scheda singola).
    public static class FabbricaSchede
    {
        public static List<SchedaDettaglio> PerId(TipoRicerca categoria, int id) => categoria switch
        {
            TipoRicerca.Personaggi => GestioneAPI.GetPersonaggiPerId(id).Select(x => Scheda(x)).ToList(),
            TipoRicerca.Pianeti    => GestioneAPI.GetPianetiPerId(id).Select(x => Scheda(x)).ToList(),
            TipoRicerca.Razze      => GestioneAPI.GetRazzePerId(id).Select(x => Scheda(x)).ToList(),
            TipoRicerca.Astronavi  => GestioneAPI.GetAstronaviPerId(id).Select(x => Scheda(x)).ToList(),
            _ => new List<SchedaDettaglio>()
        };

        public static SchedaDettaglio Scheda(Personaggio x) => new()
        {
            Id = x.Id, Nome = x.Name, Immagine = x.MainImage, NumeroGalleria = x.ImageGallery.Count,
            Riga1 = $"Altezza: {x.Height}", Riga2 = $"Genere: {x.Gender}", Riga3 = $"Nascita: {x.BirthYear}"
        };

        public static SchedaDettaglio Scheda(Pianeta x) => new()
        {
            Id = x.Id, Nome = x.Name, Immagine = x.MainImage, NumeroGalleria = x.ImageGallery.Count,
            Riga1 = $"Clima: {x.Climate}", Riga2 = $"Terreno: {x.Terrain}", Riga3 = $"Popolazione: {x.Population}"
        };

        public static SchedaDettaglio Scheda(Razza x) => new()
        {
            Id = x.Id, Nome = x.Name, Immagine = x.MainImage, NumeroGalleria = x.ImageGallery.Count,
            Riga1 = $"Classificazione: {x.Classification}", Riga2 = $"Lingua: {x.Language}", Riga3 = $"Altezza media: {x.AverageHeight}"
        };

        public static SchedaDettaglio Scheda(Astronave x) => new()
        {
            Id = x.Id, Nome = x.Name, Immagine = x.MainImage, NumeroGalleria = x.ImageGallery.Count,
            Riga1 = $"Modello: {x.Model}", Riga2 = $"Classe: {x.StarshipClass}", Riga3 = $"Costruttore: {x.Manufacturer}"
        };
    }

    // Pagina di UNA sola entità (un personaggio, un pianeta, una razza o un'astronave).
    // Riceve un ParametroDettaglio: usa Categoria + EntitaId (FilmId viene ignorato).
    public sealed partial class PaginaDettaglio : Page
    {
        public PaginaDettaglio()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not ParametroDettaglio p || p.EntitaId is not int id) return;

            Titolo.Text = NomeSingolare(p.Categoria);
            Caricamento.IsActive = true;

            try
            {
                // Download sincrono di GestioneAPI su thread in background
                var scheda = (await Task.Run(() => FabbricaSchede.PerId(p.Categoria, id))).FirstOrDefault();
                Mostra(scheda);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore PaginaDettaglio: {ex.Message}");
            }
            finally
            {
                Caricamento.IsActive = false;
            }
        }

        private void Mostra(SchedaDettaglio? s)
        {
            if (s == null)
            {
                NomeScheda.Text = "Elemento non trovato";
                Riga1.Text = Riga2.Text = Riga3.Text = Galleria.Text = "";
                ImmagineScheda.Source = null;
                return;
            }

            NomeScheda.Text = s.Nome;
            Riga1.Text = s.Riga1;
            Riga2.Text = s.Riga2;
            Riga3.Text = s.Riga3;
            Galleria.Text = $"Immagini in galleria: {s.NumeroGalleria}";
            ImmagineScheda.Source = PaginaFilm.ConvertiPoster(s.Immagine);
        }

        private static string NomeSingolare(TipoRicerca categoria) => categoria switch
        {
            TipoRicerca.Personaggi => "PERSONAGGIO",
            TipoRicerca.Pianeti    => "PIANETA",
            TipoRicerca.Razze      => "RAZZA",
            TipoRicerca.Astronavi  => "ASTRONAVE",
            _ => categoria.ToString().ToUpper()
        };

        private void Indietro_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }
    }
}
