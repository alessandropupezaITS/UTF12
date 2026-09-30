using ProgettoUSF12.BackEnd.Classi;
using ProgettoUSF12.BackEnd.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ProgettoUSF12
{
    public sealed partial class MainPage : Page
    {
        // Tutti i film scaricati UNA volta sola da SWAPI: i chip filtrano questa
        // lista in memoria, senza rifare le chiamate di rete a ogni click.
        private List<Film> _tuttiIFilm = new();

        // Categoria attiva. "Tutti" = nessun filtro.
        private string _categoriaAttiva = "Tutti";

        // Quante locandine per riga nella griglia.
        private const int FilmPerRiga = 3;

        public MainPage()
        {
            InitializeComponent();
            CaricaPagina();
        }

        private void CaricaPagina()
        {
            try
            {
                // Chip categorie (gli stessi di PaginaFilm)
                ListaCategorie.ItemsSource = new List<string>
                {
                    "Tutti", "Prima Trilogia", "Seconda Trilogia", "Terza Trilogia"
                };

                _tuttiIFilm = GestioneAPI.GetFilms().OrderBy(f => f.EpisodeId).ToList();

                AggiornaGriglia();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore caricamento Home: {ex.Message}");
            }
        }

        // Applica la categoria attiva e raggruppa i film per trilogia:
        // una sezione (titolo + griglia) per ogni trilogia che ha almeno un film.
        private void AggiornaGriglia()
        {
            var filtrati = _tuttiIFilm.Where(f =>
                _categoriaAttiva == "Tutti" || NomeCategoria(f.EpisodeId) == _categoriaAttiva);

            var sezioni = filtrati
                .GroupBy(f => NomeCategoria(f.EpisodeId))
                .OrderBy(g => g.Min(f => f.EpisodeId))
                .Select(g => new SezioneFilm { Titolo = g.Key, Films = g.OrderBy(f => f.EpisodeId).ToList() })
                .ToList();

            ListaSezioni.ItemsSource = sezioni;
            NessunRisultato.Visibility = sezioni.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Divide la larghezza della griglia di una sezione in FilmPerRiga celle uguali;
        // l'altezza segue la proporzione della locandina (circa 2:3) più il titolo.
        private void GrigliaSezione_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var griglia = (GridView)sender;
            if (griglia.ItemsPanelRoot is not ItemsWrapGrid pannello || griglia.ActualWidth <= 0)
                return;

            double larghezzaCella = Math.Floor((griglia.ActualWidth - 20) / FilmPerRiga); // 20 = margine di sicurezza
            double larghezzaLocandina = larghezzaCella - 12;                              // 12 = margine destro della card
            double altezzaCella = larghezzaLocandina * 1.5 + 60;                          // locandina 2:3 + titolo

            pannello.ItemWidth = larghezzaCella;
            pannello.ItemHeight = altezzaCella;
        }

        // Click su un chip: filtra sul posto, non naviga.
        private void ListaCategorie_ItemClick(object sender, ItemClickEventArgs e)
        {
            _categoriaAttiva = (string)e.ClickedItem;
            AggiornaGriglia();
        }

        // Il click su una locandina porta alla pagina Film già filtrata
        // sul film cliccato (passiamo il titolo, che è anche il testo del chip).
        private void ListaFilm_ItemClick(object sender, ItemClickEventArgs e)
        {
            var film = (Film)e.ClickedItem;
            Frame.Navigate(typeof(PaginaFilm), film.Title);
        }

        private string NomeCategoria(int episodeId) => episodeId switch
        {
            >= 1 and <= 3 => "Prima Trilogia",
            >= 4 and <= 6 => "Seconda Trilogia",
            >= 7 and <= 9 => "Terza Trilogia",
            _ => "Altri"
        };

        // Usato da {x:Bind local:MainPage.ConvertiPoster(PosterUrl)} nel DataTemplate del film.
        // {x:Bind PosterUrl} diretto su Image.Source va in eccezione quando
        // PosterUrl è null (poster non trovato, chiave OMDb mancante o non
        // valida) o quando la stringa non è un URL valido.
        // Qui invece: nessun URL valido -> nessuna immagine, il resto
        // del tile (titolo) resta visibile normalmente.
        public static BitmapImage? ConvertiPoster(string? posterUrl)
        {
            if (string.IsNullOrWhiteSpace(posterUrl))
                return null;

            try
            {
                var bmp = new BitmapImage();
                bmp.ImageFailed += (s, e) =>
                    System.Diagnostics.Debug.WriteLine($"[IMG] CARICAMENTO FALLITO {posterUrl} -> {e.ErrorMessage}");
                bmp.ImageOpened += (s, e) =>
                    System.Diagnostics.Debug.WriteLine($"[IMG] caricata {posterUrl}");
                bmp.UriSource = new Uri(posterUrl);
                return bmp;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PosterUrl non valido ('{posterUrl}'): {ex.Message}");
                return null;
            }
        }
    }

    // Una sezione della Home: titolo della trilogia + i suoi film.
    public class SezioneFilm
    {
        public string Titolo { get; set; } = "";
        public List<Film> Films { get; set; } = new();
    }
}
