using ProgettoUSF12.BackEnd.Classi;
using ProgettoUSF12.BackEnd.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ProgettoUSF12
{
    // Un film con i NOMI (solo testo, niente immagini) di personaggi, pianeti, razze e astronavi.
    // Le schede con l'immagine stanno nell'Archivio, il dettaglio di un elemento in PaginaDettaglio.
    public class SchedaFilm
    {
        public Film Film { get; set; } = null!;
        public List<string> Personaggi { get; set; } = new();
        public List<string> Pianeti { get; set; } = new();
        public List<string> Razze { get; set; } = new();
        public List<string> Astronavi { get; set; } = new();

        public string TitoloPersonaggi => $"PERSONAGGI  ({Personaggi.Count})";
        public string TitoloPianeti => $"PIANETI  ({Pianeti.Count})";
        public string TitoloRazze => $"RAZZE  ({Razze.Count})";
        public string TitoloAstronavi => $"ASTRONAVI  ({Astronavi.Count})";

        public string TestoPersonaggi => Unisci(Personaggi);
        public string TestoPianeti => Unisci(Pianeti);
        public string TestoRazze => Unisci(Razze);
        public string TestoAstronavi => Unisci(Astronavi);

        private static string Unisci(List<string> nomi) => nomi.Count == 0 ? "—" : string.Join(", ", nomi);
    }

    // Pagina Film: di base mostra TUTTI i film; i chip in alto filtrano su un singolo film.
    // Se arrivi dalla Home o dalla ricerca, il parametro di navigazione è il titolo
    // e il filtro è già attivo.
    public sealed partial class PaginaFilm : Page
    {
        // Film attivo nel filtro. "Tutti" = nessun filtro.
        private string _filmAttivo = "Tutti";

        // Cache condivise tra le visite.
        private static List<Film>? _film;
        private static readonly Dictionary<int, SchedaFilm> _cacheSchede = new();

        // Lista mostrata: i film compaiono uno alla volta man mano che sono pronti.
        private readonly ObservableCollection<SchedaFilm> _visibili = new();

        // Serve a ignorare un caricamento vecchio se nel frattempo l'utente cambia chip.
        private int _versione;

        public PaginaFilm()
        {
            InitializeComponent();
            ListaFilm.ItemsSource = _visibili;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _filmAttivo = e.Parameter as string ?? "Tutti";
            await AggiornaAsync();
        }

        private async Task AggiornaAsync()
        {
            int versione = ++_versione;
            Caricamento.IsActive = true;
            _visibili.Clear();

            try
            {
                if (_film == null)
                {
                    // Scarica in parallelo (e mette in cache) tutti i dati: è lo stesso indice
                    // della barra di ricerca, quindi il download avviene una volta sola.
                    await RicercaGlobale.CaricaAsync();
                    _film = await Task.Run(() => GestioneAPI.GetFilms().OrderBy(f => f.EpisodeId).ToList());
                }

                if (versione != _versione) return;

                // Chip: "Tutti" + i titoli dei film, in ordine di episodio
                ListaCategorie.ItemsSource = new List<string> { "Tutti" }
                    .Concat(_film.Select(f => f.Title))
                    .ToList();

                var daMostrare = _filmAttivo == "Tutti"
                    ? _film
                    : _film.Where(f => f.Title == _filmAttivo).ToList();

                foreach (var f in daMostrare)
                {
                    if (!_cacheSchede.TryGetValue(f.Id, out var scheda))
                    {
                        scheda = await Task.Run(() => CostruisciScheda(f));
                        _cacheSchede[f.Id] = scheda;
                    }

                    if (versione != _versione) return;

                    _visibili.Add(scheda);
                    Caricamento.IsActive = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore caricamento PaginaFilm: {ex.Message}");
            }
            finally
            {
                if (versione == _versione) Caricamento.IsActive = false;
            }
        }

        // Solo i nomi: i dati sono già in cache (GestioneAPI), nessuna chiamata per le immagini.
        private static SchedaFilm CostruisciScheda(Film film) => new()
        {
            Film = film,
            Personaggi = GestioneAPI.GetPersonaggi(conImmagini: false)
                .Where(x => film.PersonaggioIds.Contains(x.Id)).Select(x => x.Name).OrderBy(n => n).ToList(),
            Pianeti = GestioneAPI.GetPianeti(conImmagini: false)
                .Where(x => film.PianetaIds.Contains(x.Id)).Select(x => x.Name).OrderBy(n => n).ToList(),
            Razze = GestioneAPI.GetRazze(conImmagini: false)
                .Where(x => film.RazzaIds.Contains(x.Id)).Select(x => x.Name).OrderBy(n => n).ToList(),
            Astronavi = GestioneAPI.GetAstronavi(conImmagini: false)
                .Where(x => film.AstronaveIds.Contains(x.Id)).Select(x => x.Name).OrderBy(n => n).ToList(),
        };

        // Cambiare chip qui dentro la pagina filtra sul posto (non naviga di nuovo)
        private async void ListaCategorie_ItemClick(object sender, ItemClickEventArgs e)
        {
            _filmAttivo = (string)e.ClickedItem;
            await AggiornaAsync();
        }

        // Ritrova il film dall'Id (usato anche da altre parti dell'app).
        public static Film? TrovaFilm(int id) =>
            _film?.FirstOrDefault(f => f.Id == id) ?? RicercaGlobale.TrovaFilm(id);

        // Usato da {x:Bind local:PaginaFilm.ConvertiPoster(...)}.
        // Se l'URL è null/vuoto o non valido ritorna null: nessuna immagine, ma l'app non crasha.
        public static BitmapImage? ConvertiPoster(string? posterUrl) => CreaImmagine(posterUrl, null);

        // Come ConvertiPoster ma decodifica l'immagine piccola (usata dall'Archivio):
        // con decine di schede si risparmia molta memoria.
        public static BitmapImage? ConvertiMiniatura(string? url) => CreaImmagine(url, 200);

        private static BitmapImage? CreaImmagine(string? url, int? larghezzaDecodifica)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                var bmp = new BitmapImage();
                if (larghezzaDecodifica is int w) bmp.DecodePixelWidth = w;
                bmp.ImageFailed += (s, e) =>
                    System.Diagnostics.Debug.WriteLine($"[IMG] CARICAMENTO FALLITO {url} -> {e.ErrorMessage}");
                bmp.UriSource = new Uri(url);
                return bmp;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Url immagine non valido ('{url}'): {ex.Message}");
                return null;
            }
        }
    }
}
