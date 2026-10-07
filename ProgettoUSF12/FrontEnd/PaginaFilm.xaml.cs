using ProgettoUSF12.BackEnd.Classi;
using ProgettoUSF12.BackEnd.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ProgettoUSF12
{
    // Un nome cliccabile (personaggio, pianeta, razza o astronave) con il suo Id SWAPI.
    public class VoceNome
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public TipoRicerca Categoria { get; set; }
    }

    // Un film con i NOMI di personaggi, pianeti, razze e astronavi: ogni nome è un link
    // che apre la PaginaDettaglio di quell'elemento. Le schede con l'immagine stanno nell'Archivio.
    public class SchedaFilm : INotifyPropertyChanged
    {
        public Film Film { get; set; } = null!;
        public List<VoceNome> Personaggi { get; set; } = new();
        public List<VoceNome> Pianeti { get; set; } = new();
        public List<VoceNome> Razze { get; set; } = new();
        public List<VoceNome> Astronavi { get; set; } = new();

        // SWAPI manda l'introduzione con un "a capo" ogni ~30 caratteri: nel XAML il testo restava
        // in una colonna stretta. Qui unisco le righe di uno stesso paragrafo e tengo
        // solo gli a capo veri (riga vuota tra un paragrafo e l'altro).
        public string Introduzione
        {
            get
            {
                var t = (Film?.OpeningCrawl ?? "").Replace("\r\n", "\n");
                var paragrafi = t.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(p => string.Join(" ", p.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                                                .Select(r => r.Trim())));
                return string.Join("\n\n", paragrafi);
            }
        }

        public string TitoloPersonaggi => $"PERSONAGGI  ({Personaggi.Count})";
        public string TitoloPianeti => $"PIANETI  ({Pianeti.Count})";
        public string TitoloRazze => $"RAZZE  ({Razze.Count})";
        public string TitoloAstronavi => $"ASTRONAVI  ({Astronavi.Count})";

        // true se l'utente loggato ha salvato questo film in locale (si aggiorna a ogni visita della pagina).
        private bool _salvato;
        public bool Salvato
        {
            get => _salvato;
            set
            {
                if (_salvato == value) return;
                _salvato = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Salvato)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TestoSalva)));
            }
        }

        public string TestoSalva => Salvato ? "Rimuovi dai salvati" : "Salva in locale";

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    // Proprietà attaccata per i RichTextBlock: riceve una lista di nomi e la trasforma in
    // link separati da virgole che vanno a capo da soli. Uso nel XAML:
    //   <RichTextBlock local:NomiCliccabili.Voci="{x:Bind Personaggi}"/>
    public static class NomiCliccabili
    {
        public static readonly DependencyProperty VociProperty =
            DependencyProperty.RegisterAttached("Voci", typeof(List<VoceNome>), typeof(NomiCliccabili),
                new PropertyMetadata(null, OnVociChanged));

        public static List<VoceNome>? GetVoci(DependencyObject d) => (List<VoceNome>?)d.GetValue(VociProperty);
        public static void SetVoci(DependencyObject d, List<VoceNome>? v) => d.SetValue(VociProperty, v);

        private static readonly SolidColorBrush _coloreLink =
            new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 217, 102));

        private static void OnVociChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not RichTextBlock rtb) return;

            rtb.Blocks.Clear();
            var paragrafo = new Paragraph();
            var voci = e.NewValue as List<VoceNome>;

            if (voci == null || voci.Count == 0)
            {
                paragrafo.Inlines.Add(new Run { Text = "—" });
            }
            else
            {
                for (int i = 0; i < voci.Count; i++)
                {
                    if (i > 0) paragrafo.Inlines.Add(new Run { Text = ", " });

                    var voce = voci[i];
                    var link = new Hyperlink { UnderlineStyle = UnderlineStyle.None, Foreground = _coloreLink };
                    link.Inlines.Add(new Run { Text = voce.Nome });
                    link.Click += (s, a) => ApriDettaglio(voce);
                    paragrafo.Inlines.Add(link);
                }
            }

            rtb.Blocks.Add(paragrafo);
        }

        private static void ApriDettaglio(VoceNome v)
        {
            var frame = Window.Current.Content as Frame;
            if (frame == null) return;

            // Un film si apre nella PaginaFilm (il parametro è il titolo, come dalla Home);
            // tutto il resto nella PaginaDettaglio.
            if (v.Categoria == TipoRicerca.Film)
                frame.Navigate(typeof(PaginaFilm), v.Nome);
            else
                frame.Navigate(typeof(PaginaDettaglio),
                    new ParametroDettaglio { Categoria = v.Categoria, EntitaId = v.Id });
        }
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
            var utente = GestioneUtente.ChiaveCorrente;
            Caricamento.IsActive = true;
            _visibili.Clear();

            try
            {
                var elencoFilm = _film;
                if (elencoFilm == null)
                {
                    // Scarica in parallelo (e mette in cache) tutti i dati: è lo stesso indice
                    // della barra di ricerca, quindi il download avviene una volta sola.
                    await RicercaGlobale.CaricaAsync();
                    elencoFilm = await Task.Run(() => GestioneAPI.GetFilms().OrderBy(f => f.EpisodeId).ToList());

                    // Offline la lista contiene solo i film salvati: non va tenuta in cache,
                    // altrimenti resterebbe incompleta anche quando torna la rete.
                    if (elencoFilm.Count > 0 && !GestioneAPI.IsOffline) _film = elencoFilm;
                }

                if (versione != _versione) return;

                // Chip: "Tutti" + i titoli dei film, in ordine di episodio
                ListaCategorie.ItemsSource = new List<string> { "Tutti" }
                    .Concat(elencoFilm.Select(f => f.Title))
                    .ToList();

                var daMostrare = _filmAttivo == "Tutti"
                    ? elencoFilm
                    : elencoFilm.Where(f => f.Title == _filmAttivo).ToList();

                foreach (var f in daMostrare)
                {
                    // Offline le liste dei nomi sono incomplete: la scheda non va messa in cache.
                    bool offline = GestioneAPI.IsOffline;
                    SchedaFilm? scheda = null;
                    if (offline || !_cacheSchede.TryGetValue(f.Id, out scheda))
                    {
                        scheda = await Task.Run(() => CostruisciScheda(f));
                        if (!offline) _cacheSchede[f.Id] = scheda;
                    }

                    // La scheda è in cache e condivisa: lo stato "salvato" dipende dall'utente corrente.
                    scheda.Salvato = utente != null && await Task.Run(() => FilmSalvato(utente, f.Id));

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

        // Solo i nomi (con Id e categoria, per i link): i dati sono già in cache (GestioneAPI).
        private static SchedaFilm CostruisciScheda(Film film) => new()
        {
            Film = film,
            Personaggi = GestioneAPI.GetPersonaggi(conImmagini: false)
                .Where(x => film.PersonaggioIds.Contains(x.Id)).OrderBy(x => x.Name)
                .Select(x => new VoceNome { Id = x.Id, Nome = x.Name, Categoria = TipoRicerca.Personaggi }).ToList(),
            Pianeti = GestioneAPI.GetPianeti(conImmagini: false)
                .Where(x => film.PianetaIds.Contains(x.Id)).OrderBy(x => x.Name)
                .Select(x => new VoceNome { Id = x.Id, Nome = x.Name, Categoria = TipoRicerca.Pianeti }).ToList(),
            Razze = GestioneAPI.GetRazze(conImmagini: false)
                .Where(x => film.RazzaIds.Contains(x.Id)).OrderBy(x => x.Name)
                .Select(x => new VoceNome { Id = x.Id, Nome = x.Name, Categoria = TipoRicerca.Razze }).ToList(),
            Astronavi = GestioneAPI.GetAstronavi(conImmagini: false)
                .Where(x => film.AstronaveIds.Contains(x.Id)).OrderBy(x => x.Name)
                .Select(x => new VoceNome { Id = x.Id, Nome = x.Name, Categoria = TipoRicerca.Astronavi }).ToList(),
        };

        // Salva il film e anche i suoi personaggi, pianeti, razze e astronavi (solo dati, quelli già
        // salvati restano come sono): offline la pagina del film e i link ai dettagli funzionano.
        private static void SalvaFilmCompleto(string utente, Film film)
        {
            ArchivioLocale.Salva(utente, film);

            var collegati = new List<object>();
            collegati.AddRange(GestioneAPI.GetPersonaggi(conImmagini: false).Where(x => film.PersonaggioIds.Contains(x.Id)));
            collegati.AddRange(GestioneAPI.GetPianeti(conImmagini: false).Where(x => film.PianetaIds.Contains(x.Id)));
            collegati.AddRange(GestioneAPI.GetRazze(conImmagini: false).Where(x => film.RazzaIds.Contains(x.Id)));
            collegati.AddRange(GestioneAPI.GetAstronavi(conImmagini: false).Where(x => film.AstronaveIds.Contains(x.Id)));

            ArchivioLocale.SalvaMolti(utente, collegati, sovrascrivi: false);
        }

        private static bool FilmSalvato(string utente, int filmId)
        {
            try { return ArchivioLocale.Esiste(utente, TipoRicerca.Film, filmId); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Controllo film salvato fallito: {ex.Message}");
                return false;
            }
        }

        // Bottone "Salva in locale" / "Rimuovi dai salvati" di un film (Tag = la SchedaFilm).
        private async void BtnSalvaFilm_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not SchedaFilm scheda) return;

            var utente = GestioneUtente.ChiaveCorrente;
            if (utente == null)
            {
                await new ContentDialog
                {
                    Title = "Accesso richiesto",
                    Content = "Accedi per poter salvare in locale.",
                    CloseButtonText = "OK"
                }.ShowAsync();
                return;
            }

            var film = scheda.Film;
            btn.IsEnabled = false;
            try
            {
                if (scheda.Salvato)
                {
                    await Task.Run(() => ArchivioLocale.Rimuovi(utente, TipoRicerca.Film, film.Id));
                    scheda.Salvato = false;
                }
                else
                {
                    await Task.Run(() => SalvaFilmCompleto(utente, film));
                    scheda.Salvato = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Salvataggio film fallito: {ex.Message}");
            }
            finally
            {
                btn.IsEnabled = true;
            }
        }

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

                var uri = new Uri(url);
                if (DaScaricare(uri))
                    _ = CaricaConHttpAsync(bmp, uri);   // Fandom: scarico io i byte (vedi sotto)
                else
                    bmp.UriSource = uri;
                return bmp;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Url immagine non valido ('{url}'): {ex.Message}");
                return null;
            }
        }

        // I file di Fandom con BitmapImage+Uri danno E_NETWORK_ERROR: li scarico con l'HttpClient
        // dell'app (User-Agent + Referer) e li passo a XAML come stream.
        private static bool DaScaricare(Uri uri) =>
            uri.Host.EndsWith("wikia.nocookie.net", StringComparison.OrdinalIgnoreCase)
            || uri.Host.EndsWith("fandom.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.EndsWith("wikia.com", StringComparison.OrdinalIgnoreCase);

        private static async Task CaricaConHttpAsync(BitmapImage bmp, Uri uri)
        {
            try
            {
                var bytes = await Task.Run(() => GestioneAPI.ScaricaBytes(uri.ToString()));
                if (bytes != null && bytes.Length > 0)
                {
                    using var stream = new InMemoryRandomAccessStream();
                    await stream.WriteAsync(bytes.AsBuffer());
                    stream.Seek(0);
                    await bmp.SetSourceAsync(stream);
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[IMG] CaricaConHttpAsync('{uri}'): {ex.Message}");
            }

            // Ultima possibilità: il caricamento normale (se fallisce scatta ImageFailed come prima,
            // così le pagine che tolgono le immagini rotte continuano a funzionare).
            bmp.UriSource = uri;
        }
    }
}
