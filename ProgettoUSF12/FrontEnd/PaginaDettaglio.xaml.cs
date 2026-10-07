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
    // Una riga "Etichetta: valore" della scheda.
    public class RigaInfo
    {
        public string Etichetta { get; set; } = "";
        public string Valore { get; set; } = "";
    }

    // Un gruppo di nomi cliccabili con il suo titolo (es. "FILM (3)").
    public class GruppoLink
    {
        public string Intestazione { get; set; } = "";
        public List<VoceNome> Voci { get; set; } = new();
    }

    // Scheda "neutra": qualunque entità (personaggio, pianeta, razza, astronave)
    // viene convertita in questa, così il XAML è uno solo.
    public class SchedaDettaglio
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string? Immagine { get; set; }                 // immagine principale (URL)
        public List<string> Galleria { get; set; } = new();   // TITOLI di file Fandom, non URL
        public List<RigaInfo> Info { get; set; } = new();            // tutte le info (etichetta: valore)
        public List<GruppoLink> Collegamenti { get; set; } = new();  // gruppi di nomi cliccabili (film, ...)
        public int NumeroGalleria { get; set; }
        public object? Entita { get; set; }                   // l'entità originale (serve per salvarla)
    }

    // Conversione entità -> scheda, caricamento per Id, salvataggio/lettura locale.
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

        // Copia salvata in locale da QUESTO utente (null se non l'ha salvata).
        public static SchedaDettaglio? DaLocale(string utente, TipoRicerca categoria, int id)
        {
            switch (categoria)
            {
                case TipoRicerca.Personaggi:
                    { var x = ArchivioLocale.CaricaPersonaggio(utente, id); return x == null ? null : Scheda(x); }
                case TipoRicerca.Pianeti:
                    { var x = ArchivioLocale.CaricaPianeta(utente, id); return x == null ? null : Scheda(x); }
                case TipoRicerca.Razze:
                    { var x = ArchivioLocale.CaricaRazza(utente, id); return x == null ? null : Scheda(x); }
                case TipoRicerca.Astronavi:
                    { var x = ArchivioLocale.CaricaAstronave(utente, id); return x == null ? null : Scheda(x); }
                default:
                    return null;
            }
        }

        public static void Salva(string utente, SchedaDettaglio scheda)
        {
            if (scheda.Entita != null) ArchivioLocale.Salva(utente, scheda.Entita);
        }

        // Righe "Etichetta: valore": i campi vuoti vengono saltati.
        private static List<RigaInfo> Righe(int numeroGalleria, params (string Etichetta, string? Valore)[] campi)
        {
            var righe = campi
                .Where(c => !string.IsNullOrWhiteSpace(c.Valore))
                .Select(c => new RigaInfo { Etichetta = c.Etichetta, Valore = c.Valore! })
                .ToList();
            if (numeroGalleria > 0)
                righe.Add(new RigaInfo { Etichetta = "Immagini in galleria", Valore = numeroGalleria.ToString() });
            return righe;
        }

        public static SchedaDettaglio Scheda(Personaggio x) => new()
        {
            Id = x.Id, Nome = x.Name, Immagine = x.MainImage, Galleria = x.ImageGallery,
            NumeroGalleria = x.ImageGallery.Count, Entita = x,
            Info = Righe(x.ImageGallery.Count,
                ("Altezza", x.Height), ("Peso", x.Mass), ("Genere", x.Gender), ("Nascita", x.BirthYear),
                ("Capelli", x.HairColor), ("Occhi", x.EyeColor))
        };

        public static SchedaDettaglio Scheda(Pianeta x) => new()
        {
            Id = x.Id, Nome = x.Name, Immagine = x.MainImage, Galleria = x.ImageGallery,
            NumeroGalleria = x.ImageGallery.Count, Entita = x,
            Info = Righe(x.ImageGallery.Count,
                ("Clima", x.Climate), ("Terreno", x.Terrain), ("Popolazione", x.Population),
                ("Diametro", x.Diameter), ("Gravità", x.Gravity), ("Acqua in superficie", x.SurfaceWater),
                ("Periodo di rotazione", x.RotationPeriod))
        };

        public static SchedaDettaglio Scheda(Razza x) => new()
        {
            Id = x.Id, Nome = x.Name, Immagine = x.MainImage, Galleria = x.ImageGallery,
            NumeroGalleria = x.ImageGallery.Count, Entita = x,
            Info = Righe(x.ImageGallery.Count,
                ("Classificazione", x.Classification), ("Lingua", x.Language),
                ("Altezza media", x.AverageHeight), ("Durata media della vita", x.AverageLifespan))
        };

        public static SchedaDettaglio Scheda(Astronave x) => new()
        {
            Id = x.Id, Nome = x.Name, Immagine = x.MainImage, Galleria = x.ImageGallery,
            NumeroGalleria = x.ImageGallery.Count, Entita = x,
            Info = Righe(x.ImageGallery.Count,
                ("Modello", x.Model), ("Classe", x.StarshipClass), ("Costruttore", x.Manufacturer),
                ("Equipaggio", x.Crew), ("Passeggeri", x.Passengers),
                ("Hyperdrive", x.HyperdriveRating), ("MGLT", x.Mglt))
        };

        // ===== Collegamenti cliccabili (nomi che aprono la pagina dell'elemento) =====
        // Dati già in cache (GestioneAPI): da chiamare su un thread in background.

        private static GruppoLink? Gruppo(string titolo, List<VoceNome> voci) =>
            voci.Count == 0 ? null : new GruppoLink { Intestazione = $"{titolo}  ({voci.Count})", Voci = voci };

        private static List<VoceNome> VociFilm(List<int> ids) =>
            GestioneAPI.GetFilms().Where(f => ids.Contains(f.Id)).OrderBy(f => f.EpisodeId)
                .Select(f => new VoceNome { Id = f.Id, Nome = f.Title, Categoria = TipoRicerca.Film }).ToList();

        private static List<VoceNome> VociPersonaggi(Func<Personaggio, bool> filtro) =>
            GestioneAPI.GetPersonaggi(conImmagini: false).Where(filtro).OrderBy(x => x.Name)
                .Select(x => new VoceNome { Id = x.Id, Nome = x.Name, Categoria = TipoRicerca.Personaggi }).ToList();

        private static List<VoceNome> VociPianeti(Func<Pianeta, bool> filtro) =>
            GestioneAPI.GetPianeti(conImmagini: false).Where(filtro).OrderBy(x => x.Name)
                .Select(x => new VoceNome { Id = x.Id, Nome = x.Name, Categoria = TipoRicerca.Pianeti }).ToList();

        private static List<VoceNome> VociRazze(Func<Razza, bool> filtro) =>
            GestioneAPI.GetRazze(conImmagini: false).Where(filtro).OrderBy(x => x.Name)
                .Select(x => new VoceNome { Id = x.Id, Nome = x.Name, Categoria = TipoRicerca.Razze }).ToList();

        private static List<VoceNome> VociAstronavi(Func<Astronave, bool> filtro) =>
            GestioneAPI.GetAstronavi(conImmagini: false).Where(filtro).OrderBy(x => x.Name)
                .Select(x => new VoceNome { Id = x.Id, Nome = x.Name, Categoria = TipoRicerca.Astronavi }).ToList();

        public static List<GruppoLink> Collegamenti(SchedaDettaglio s)
        {
            var g = new List<GruppoLink?>();

            switch (s.Entita)
            {
                case Personaggio p:
                    g.Add(Gruppo("PIANETA NATALE", VociPianeti(x => x.Id == p.HomeworldId)));
                    g.Add(Gruppo("RAZZA", VociRazze(x => p.SpeciesIds.Contains(x.Id) || x.PersonaggioIds.Contains(p.Id))));
                    g.Add(Gruppo("ASTRONAVI", VociAstronavi(x => p.StarshipIds.Contains(x.Id) || x.PilotIds.Contains(p.Id))));
                    g.Add(Gruppo("FILM", VociFilm(p.FilmIds)));
                    break;

                case Pianeta pl:
                    g.Add(Gruppo("ABITANTI", VociPersonaggi(x => x.HomeworldId == pl.Id)));
                    g.Add(Gruppo("RAZZE ORIGINARIE", VociRazze(x => x.HomeworldId == pl.Id)));
                    g.Add(Gruppo("FILM", VociFilm(pl.FilmIds)));
                    break;

                case Razza r:
                    g.Add(Gruppo("PIANETA NATALE", VociPianeti(x => x.Id == r.HomeworldId)));
                    g.Add(Gruppo("PERSONAGGI", VociPersonaggi(x => r.PersonaggioIds.Contains(x.Id) || x.SpeciesIds.Contains(r.Id))));
                    g.Add(Gruppo("FILM", VociFilm(r.FilmIds)));
                    break;

                case Astronave a:
                    g.Add(Gruppo("PILOTI", VociPersonaggi(x => a.PilotIds.Contains(x.Id) || x.StarshipIds.Contains(a.Id))));
                    g.Add(Gruppo("FILM", VociFilm(a.FilmIds)));
                    break;
            }

            return g.Where(x => x != null).Select(x => x!).ToList();
        }
    }

    // Pagina di UNA sola entità (un personaggio, un pianeta, una razza o un'astronave).
    // Riceve un ParametroDettaglio: usa Categoria + EntitaId (FilmId viene ignorato).
    public sealed partial class PaginaDettaglio : Page
    {
        private readonly ObservableCollection<BitmapImage> _immagini = new();
        private readonly HashSet<string> _urlMostrati = new();

        private bool _haPrincipale;   // true se la foto principale (a sinistra) è impostata

        private SchedaDettaglio? _scheda;
        private TipoRicerca _categoria;
        private int _id;
        private bool _salvato;   // true se l'utente loggato ha già salvato questo elemento

        public PaginaDettaglio()
        {
            InitializeComponent();
            GalleriaFoto.ItemsSource = _immagini;
            _immagini.CollectionChanged += (s, e) => AggiornaImmagini();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not ParametroDettaglio p || p.EntitaId is not int id) return;

            _categoria = p.Categoria;
            _id = id;
            Titolo.Text = NomeSingolare(p.Categoria);
            Caricamento.IsActive = true;

            var utente = GestioneUtente.ChiaveCorrente;

            try
            {
                // Lettura DB / download API (sincroni) su thread in background
                _scheda = await Task.Run(() =>
                {
                    var sc = CaricaScheda(p.Categoria, id, utente);
                    if (sc != null)
                    {
                        try { sc.Collegamenti = FabbricaSchede.Collegamenti(sc); }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Collegamenti falliti: {ex.Message}"); }
                    }
                    return sc;
                });
                Mostra(_scheda);

                _salvato = _scheda != null && utente != null
                           && await Task.Run(() => StaSalvato(utente, p.Categoria, id));
                AggiornaPulsante();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore PaginaDettaglio: {ex.Message}");
            }
            finally
            {
                Caricamento.IsActive = false;
            }

            // Il resto del carosello (galleria) arriva dopo: la pagina è già utilizzabile.
            if (_scheda != null) await CaricaGalleriaAsync(_scheda);
        }

        // 1) copia salvata dall'utente (nessuna rete)  2) altrimenti API.
        private static SchedaDettaglio? CaricaScheda(TipoRicerca categoria, int id, string? utente)
        {
            if (utente != null)
            {
                try
                {
                    var locale = FabbricaSchede.DaLocale(utente, categoria, id);
                    if (locale != null) return locale;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lettura DB locale fallita: {ex.Message}");
                }
            }
            return FabbricaSchede.PerId(categoria, id).FirstOrDefault();
        }

        private static bool StaSalvato(string utente, TipoRicerca categoria, int id)
        {
            try { return ArchivioLocale.Esiste(utente, categoria, id); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Controllo salvataggio fallito: {ex.Message}");
                return false;
            }
        }

        private void Mostra(SchedaDettaglio? s)
        {
            _immagini.Clear();
            _urlMostrati.Clear();
            _haPrincipale = false;
            ImgPrincipale.Source = null;

            if (s == null)
            {
                NomeScheda.Text = "Elemento non trovato";
                ListaInfo.ItemsSource = null;
                ListaCollegamenti.ItemsSource = null;
                BtnSalva.Visibility = Visibility.Collapsed;
                AggiornaImmagini();
                return;
            }

            NomeScheda.Text = s.Nome;
            ListaInfo.ItemsSource = s.Info;
            ListaCollegamenti.ItemsSource = s.Collegamenti;
            BtnSalva.Visibility = Visibility.Visible;

            AggiungiImmagine(s.Immagine);   // la prima immagine diventa la foto principale
            AggiornaImmagini();
        }

        // ===================== FOTO =====================
        // La prima immagine è la foto principale (a sinistra, ImgPrincipale); le altre vanno nella
        // griglia sotto le info (GalleriaFoto), ognuna intera.

        private async Task CaricaGalleriaAsync(SchedaDettaglio s)
        {
            if (s.Galleria.Count == 0) return;

            try
            {
                var urls = await Task.Run(() => GestioneAPI.GetUrlGalleria(s.Nome, s.Galleria, 12));
                foreach (var url in urls) AggiungiImmagine(url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore galleria ({s.Nome}): {ex.Message}");
            }
        }

        private void AggiungiImmagine(string? url)
        {
            // niente url o già presente -> non aggiungo nulla
            if (string.IsNullOrWhiteSpace(url) || !_urlMostrati.Add(url)) return;

            var bmp = PaginaFilm.ConvertiPoster(url);
            if (bmp == null) return;

            // Nessuna foto principale ancora: questa diventa la principale.
            if (!_haPrincipale)
            {
                ImpostaPrincipale(bmp);
                AggiornaImmagini();
                return;
            }

            // Se l'immagine non si carica la tolgo: meglio niente che un riquadro vuoto.
            bmp.ImageFailed += (s, e) => _immagini.Remove(bmp);
            _immagini.Add(bmp);
        }

        private void ImpostaPrincipale(BitmapImage bmp)
        {
            bmp.ImageFailed += (s, e) =>
            {
                // La principale non si carica: al suo posto metto la prima della galleria, se c'è.
                _haPrincipale = false;
                ImgPrincipale.Source = null;
                PromuoviPrimaDellaGalleria();
            };
            ImgPrincipale.Source = bmp;
            _haPrincipale = true;
        }

        private void PromuoviPrimaDellaGalleria()
        {
            if (_haPrincipale || _immagini.Count == 0) { AggiornaImmagini(); return; }

            var prima = _immagini[0];
            _immagini.RemoveAt(0);
            ImpostaPrincipale(prima);
            AggiornaImmagini();
        }

        private void AggiornaImmagini()
        {
            NessunaImmagine.Visibility = _haPrincipale ? Visibility.Collapsed : Visibility.Visible;
            SezioneGalleria.Visibility = _immagini.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            TitoloGalleria.Text = $"ALTRE FOTO  ({_immagini.Count})";
        }

        // ===================== SALVA IN LOCALE =====================

        private void AggiornaPulsante()
        {
            BtnSalva.Content = _salvato ? "Rimuovi dai salvati" : "Salva in locale";

            if (GestioneUtente.ChiaveCorrente == null)
                StatoSalvataggio.Text = "Accedi per poter salvare in locale.";
            else if (_salvato && string.IsNullOrEmpty(StatoSalvataggio.Text))
                StatoSalvataggio.Text = "Elemento salvato nel tuo archivio locale.";
        }

        private async void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            var utente = GestioneUtente.ChiaveCorrente;
            if (utente == null)
            {
                StatoSalvataggio.Text = "Accedi per poter salvare in locale.";
                return;
            }

            var scheda = _scheda;
            if (scheda == null) return;

            var categoria = _categoria;
            var id = _id;

            BtnSalva.IsEnabled = false;
            try
            {
                if (_salvato)
                {
                    await Task.Run(() => ArchivioLocale.Rimuovi(utente, categoria, id));
                    _salvato = false;
                    StatoSalvataggio.Text = "Rimosso dai tuoi salvati.";
                }
                else
                {
                    await Task.Run(() => FabbricaSchede.Salva(utente, scheda));
                    _salvato = true;
                    StatoSalvataggio.Text = "Salvato nel tuo archivio locale.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Salvataggio locale fallito: {ex.Message}");
                StatoSalvataggio.Text = "Errore durante il salvataggio.";
            }
            finally
            {
                BtnSalva.IsEnabled = true;
                BtnSalva.Content = _salvato ? "Rimuovi dai salvati" : "Salva in locale";
            }
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
