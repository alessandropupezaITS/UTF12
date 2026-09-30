using ProgettoUSF12.BackEnd.Classi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProgettoUSF12.BackEnd.Services
{
    // ============================================================
    // GestioneAPI - versione semplificata
    // ------------------------------------------------------------
    // Solo i metodi che servono davvero:
    //   - GetFilms / GetPersonaggi / GetPianeti / GetRazze / GetAstronavi
    //     -> tutte le entità di quel tipo (da SWAPI)
    //   - GetPersonaggiBy / GetPianetiBy / GetRazzeBy / GetAstronaviBy
    //     -> solo le entità collegate a un film specifico
    //     (Film non ha una versione "By": non avrebbe senso
    //     filtrare i film per un altro film)
    //
    // Tutti i metodi sono SINCRONI e ritornano List<T> direttamente,
    // non Task<List<T>>: chi li chiama non scrive "await".
    //
    // NB IMPORTANTE: ogni chiamata _http.GetFromJsonAsync<T>(...) deve
    // SEMPRE passare _jsonOptions come secondo parametro. Senza, usa la
    // deserializzazione a reflection, che in questo progetto fallisce
    // silenziosamente (try/catch la cattura, ritorna null/vuoto senza
    // nessun errore visibile) — è il bug che ci ha già fatto perdere
    // tempo sia per SWAPI che per il poster OMDb.
    // ============================================================
    public static class GestioneAPI
    {
        private const string SwapiBaseUrl = "https://swapi.dev/api/";
        private const string OmdbBaseUrl = "https://www.omdbapi.com/";
        private const string OmdbApiKey = "e6ceb7e7";  //e6ceb7e7 // <-- inserisci qui la chiave ricevuta via email
        private const string FandomBaseUrl = "https://starwars.fandom.com/api.php";

        private static readonly HttpClient _http = new HttpClient();

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = GestioneApiJsonContext.Default
        };

        // ===================== FILM =====================

        private static List<Film>? _cacheFilm;
        private static readonly object _lockFilm = new();

        // I film (e i 6 poster OMDb) si scaricano UNA volta per sessione, poster in parallelo.
        public static List<Film> GetFilms()
        {
            lock (_lockFilm)
            {
                if (_cacheFilm != null) return _cacheFilm;

                var film = ScaricaTutto<Film>("films");

                Parallel.ForEach(film, f =>
                {
                    f.Id = SwapiIdConverter.ExtractId(f.Url);
                    f.PosterUrl = GetPosterOmdb(f.EpisodeId);
                });

                if (film.Count > 0) _cacheFilm = film;
                return film;
            }
        }

        // ===================== PERSONAGGI =====================

        public static List<Personaggio> GetPersonaggi(bool conImmagini = true)
        {
            var personaggi = ScaricaTutto<Personaggio>("people");

            foreach (var p in personaggi)
            {
                p.Id = SwapiIdConverter.ExtractId(p.Url);
                if (!conImmagini) continue;   // salta le chiamate Fandom (lente)
                var (main, gallery) = GetImmagini(p.Name, "characters", p.Id);
                p.MainImage = main;
                p.ImageGallery = gallery;
            }

            return personaggi;
        }

        public static List<Personaggio> GetPersonaggiBy(Film film)
        {
            // Prima filtro (veloce, senza immagini), poi scarico le immagini
            // Fandom SOLO per le entità di questo film, in parallelo.
            var lista = GetPersonaggi(conImmagini: false)
                .Where(p => film.PersonaggioIds.Contains(p.Id)).ToList();

            Parallel.ForEach(lista, new ParallelOptions { MaxDegreeOfParallelism = 6 }, p =>
            {
                var (main, gallery) = GetImmagini(p.Name, "characters", p.Id);
                p.MainImage = main;
                p.ImageGallery = gallery;
            });

            return lista.OrderBy(p => p.Name).ToList();
        }

        // Un solo elemento (usato dalla ricerca quando l'elemento non ha film collegati).
        public static List<Personaggio> GetPersonaggiPerId(int id)
        {
            var lista = GetPersonaggi(conImmagini: false).Where(x => x.Id == id).ToList();
            foreach (var x in lista)
            {
                var (main, gallery) = GetImmagini(x.Name, "characters", x.Id);
                x.MainImage = main;
                x.ImageGallery = gallery;
            }
            return lista;
        }

        // ===================== PIANETI =====================

        public static List<Pianeta> GetPianeti(bool conImmagini = true)
        {
            var pianeti = ScaricaTutto<Pianeta>("planets");

            foreach (var p in pianeti)
            {
                p.Id = SwapiIdConverter.ExtractId(p.Url);
                if (!conImmagini) continue;   // salta le chiamate Fandom (lente)
                var (main, gallery) = GetImmagini(p.Name, "planets", p.Id);
                p.MainImage = main;
                p.ImageGallery = gallery;
            }

            return pianeti;
        }

        public static List<Pianeta> GetPianetiBy(Film film)
        {
            // Prima filtro (veloce, senza immagini), poi scarico le immagini
            // Fandom SOLO per le entità di questo film, in parallelo.
            var lista = GetPianeti(conImmagini: false)
                .Where(p => film.PianetaIds.Contains(p.Id)).ToList();

            Parallel.ForEach(lista, new ParallelOptions { MaxDegreeOfParallelism = 6 }, p =>
            {
                var (main, gallery) = GetImmagini(p.Name, "planets", p.Id);
                p.MainImage = main;
                p.ImageGallery = gallery;
            });

            return lista.OrderBy(p => p.Name).ToList();
        }

        // Un solo elemento (usato dalla ricerca quando l'elemento non ha film collegati).
        public static List<Pianeta> GetPianetiPerId(int id)
        {
            var lista = GetPianeti(conImmagini: false).Where(x => x.Id == id).ToList();
            foreach (var x in lista)
            {
                var (main, gallery) = GetImmagini(x.Name, "planets", x.Id);
                x.MainImage = main;
                x.ImageGallery = gallery;
            }
            return lista;
        }

        // ===================== RAZZE =====================

        public static List<Razza> GetRazze(bool conImmagini = true)
        {
            var razze = ScaricaTutto<Razza>("species");

            foreach (var r in razze)
            {
                r.Id = SwapiIdConverter.ExtractId(r.Url);
                if (!conImmagini) continue;   // salta le chiamate Fandom (lente)
                var (main, gallery) = GetImmagini(r.Name, "species", r.Id);
                r.MainImage = main;
                r.ImageGallery = gallery;
            }

            return razze;
        }

        public static List<Razza> GetRazzeBy(Film film)
        {
            // Prima filtro (veloce, senza immagini), poi scarico le immagini
            // Fandom SOLO per le entità di questo film, in parallelo.
            var lista = GetRazze(conImmagini: false)
                .Where(r => film.RazzaIds.Contains(r.Id)).ToList();

            Parallel.ForEach(lista, new ParallelOptions { MaxDegreeOfParallelism = 6 }, r =>
            {
                var (main, gallery) = GetImmagini(r.Name, "species", r.Id);
                r.MainImage = main;
                r.ImageGallery = gallery;
            });

            return lista.OrderBy(r => r.Name).ToList();
        }

        // Un solo elemento (usato dalla ricerca quando l'elemento non ha film collegati).
        public static List<Razza> GetRazzePerId(int id)
        {
            var lista = GetRazze(conImmagini: false).Where(x => x.Id == id).ToList();
            foreach (var x in lista)
            {
                var (main, gallery) = GetImmagini(x.Name, "species", x.Id);
                x.MainImage = main;
                x.ImageGallery = gallery;
            }
            return lista;
        }

        // ===================== ASTRONAVI =====================

        public static List<Astronave> GetAstronavi(bool conImmagini = true)
        {
            var astronavi = ScaricaTutto<Astronave>("starships");

            foreach (var a in astronavi)
            {
                a.Id = SwapiIdConverter.ExtractId(a.Url);
                if (!conImmagini) continue;   // salta le chiamate Fandom (lente)
                var (main, gallery) = GetImmagini(a.Name, "starships", a.Id);
                a.MainImage = main;
                a.ImageGallery = gallery;
            }

            return astronavi;
        }

        public static List<Astronave> GetAstronaviBy(Film film)
        {
            // Prima filtro (veloce, senza immagini), poi scarico le immagini
            // Fandom SOLO per le entità di questo film, in parallelo.
            var lista = GetAstronavi(conImmagini: false)
                .Where(a => film.AstronaveIds.Contains(a.Id)).ToList();

            Parallel.ForEach(lista, new ParallelOptions { MaxDegreeOfParallelism = 6 }, a =>
            {
                var (main, gallery) = GetImmagini(a.Name, "starships", a.Id);
                a.MainImage = main;
                a.ImageGallery = gallery;
            });

            return lista.OrderBy(a => a.Name).ToList();
        }

        // Un solo elemento (usato dalla ricerca quando l'elemento non ha film collegati).
        public static List<Astronave> GetAstronaviPerId(int id)
        {
            var lista = GetAstronavi(conImmagini: false).Where(x => x.Id == id).ToList();
            foreach (var x in lista)
            {
                var (main, gallery) = GetImmagini(x.Name, "starships", x.Id);
                x.MainImage = main;
                x.ImageGallery = gallery;
            }
            return lista;
        }

        // ===================== HELPER =====================

        // Cache: ogni endpoint SWAPI si scarica UNA volta per sessione. Prima veniva riscaricato
        // per intero a ogni chiamata (anche solo per filtrare le entità di un film).
        private static readonly ConcurrentDictionary<string, object> _cacheSwapi = new();
        private static readonly ConcurrentDictionary<string, object> _lockSwapi = new();

        private static List<T> ScaricaTutto<T>(string endpointSwapi)
        {
            // Un solo download alla volta per endpoint: gli altri thread aspettano e riusano il risultato.
            lock (_lockSwapi.GetOrAdd(endpointSwapi, _ => new object()))
            {
                if (_cacheSwapi.TryGetValue(endpointSwapi, out var inCache))
                    return (List<T>)inCache;

                var risultati = new List<T>();
                string? url = $"{SwapiBaseUrl}{endpointSwapi}/";

                try
                {
                    while (!string.IsNullOrEmpty(url))
                    {
                        var pagina = _http.GetFromJsonAsync<SwapiPagina<T>>(url, _jsonOptions)
                            .GetAwaiter().GetResult();

                        if (pagina?.Results == null) break;

                        risultati.AddRange(pagina.Results);
                        url = pagina.Next;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore nel recupero di '{endpointSwapi}': {ex.Message}");
                }

                // In cache solo se sono arrivate TUTTE le pagine (url vuoto = finito senza errori).
                if (string.IsNullOrEmpty(url) && risultati.Count > 0)
                    _cacheSwapi[endpointSwapi] = risultati;

                return risultati;
            }
        }

        // ID IMDb di ciascun episodio: usarli invece del titolo evita mancati
        // match quando il titolo salvato su OMDb non è identico a quello di
        // SWAPI (es. "A New Hope" vs "Star Wars: Episode IV - A New Hope").
        private static readonly Dictionary<int, string> _imdbIdPerEpisodio = new()
        {
            { 1, "tt0120915" }, // The Phantom Menace
            { 2, "tt0121765" }, // Attack of the Clones
            { 3, "tt0121766" }, // Revenge of the Sith
            { 4, "tt0076759" }, // A New Hope
            { 5, "tt0080684" }, // The Empire Strikes Back
            { 6, "tt0086190" }, // Return of the Jedi
        };

        // Recupera l'URL del poster del film da OMDb, cercando per ID IMDb
        // (univoco) invece che per titolo (poteva non trovare match esatti).
        private static string? GetPosterOmdb(int episodeId)
        {
            if (!_imdbIdPerEpisodio.TryGetValue(episodeId, out var imdbId))
                return null;

            try
            {
                var url = $"{OmdbBaseUrl}?apikey={OmdbApiKey}&i={imdbId}";

                var risposta = _http.GetFromJsonAsync<OmdbResult>(url, _jsonOptions)
                    .GetAwaiter().GetResult();

                return string.IsNullOrEmpty(risposta?.Poster) || risposta.Poster == "N/A"
                    ? null
                    : risposta.Poster;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore GetPosterOmdb(episodio {episodeId}): {ex.Message}");
                return null;
            }
        }

        // Immagine principale: prima Star Wars Visual Guide (URL fisso per Id SWAPI,
        // nessuna ricerca per nome -> niente match sbagliati); se il file non esiste
        // ripiega sulla thumbnail di Fandom. La galleria arriva sempre da Fandom.
        private const string VisualGuideBaseUrl = "https://starwars-visualguide.com/assets/img/";

        private static (string? main, List<string> gallery) GetImmagini(string name, string tipo, int id)
        {
            var (mainFandom, gallery) = GetImmaginiFandom(name);

            var urlGuide = $"{VisualGuideBaseUrl}{tipo}/{id}.jpg";
            var guideOk = UrlEsiste(urlGuide);
            var main = guideOk ? urlGuide : mainFandom;

            System.Diagnostics.Debug.WriteLine(
                $"[IMG] {name}: visualguide={(guideOk ? "OK" : "NO")} | fandom={(mainFandom ?? "null")} | scelta={(main ?? "NESSUNA")}");

            return (main, gallery);
        }

        // Versione leggera per l'Archivio: UNA sola immagine per elemento, senza galleria.
        // Prima prova Visual Guide (1 richiesta); Fandom solo se manca.
        public static string? GetImmaginePrincipale(TipoRicerca categoria, string nome, int id)
        {
            string? tipo = categoria switch
            {
                TipoRicerca.Personaggi => "characters",
                TipoRicerca.Pianeti => "planets",
                TipoRicerca.Razze => "species",
                TipoRicerca.Astronavi => "starships",
                _ => null
            };
            if (tipo == null) return null;

            var urlGuide = $"{VisualGuideBaseUrl}{tipo}/{id}.jpg";
            return UrlEsiste(urlGuide) ? urlGuide : GetImmaginiFandom(nome).main;
        }

        private static bool UrlEsiste(string url)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                using var resp = _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead)
                    .GetAwaiter().GetResult();
                System.Diagnostics.Debug.WriteLine($"[IMG] HTTP {(int)resp.StatusCode} {url}");
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UrlEsiste('{url}'): {ex.Message}");
                return false;
            }
        }

        // Parole che indicano immagini "di contorno" del sito (loghi, favicon, Disney+...)
        // e non foto del personaggio/pianeta/razza/astronave.
        private static readonly string[] _paroleEscluse =
            { "logo", "favicon", "icon", "dplus", "disney", "wordmark", "stub", "placeholder" };

        // Cerca su Fandom: immagine principale (thumbnail) + elenco file della pagina.
        private static (string? main, List<string> gallery) GetImmaginiFandom(string name)
        {
            try
            {
                // imlimit=max: senza, Fandom ritorna solo i primi 10 file in ordine alfabetico
                // (loghi del sito: "AllStars", "Andor", "Disney"...) e le foto vere restano fuori.
                var url = $"{FandomBaseUrl}?action=query&titles={Uri.EscapeDataString(name)}" +
                          "&redirects=1&prop=pageimages|images&pilicense=any&imlimit=max&format=json&pithumbsize=500";

                var risposta = _http.GetFromJsonAsync<FandomQueryResult>(url, _jsonOptions)
                    .GetAwaiter().GetResult();

                var pagina = risposta?.Query?.Pages?.Values.FirstOrDefault();
                var main = pagina?.Thumbnail?.Source;

                var gallery = (pagina?.Images ?? new List<FandomImage>())
                    .Select(i => i.Title)
                    .Where(t => t != null
                                && (t.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                    || t.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                                    || t.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                                && !_paroleEscluse.Any(w => t.Contains(w, StringComparison.OrdinalIgnoreCase)))
                    .Select(t => t!)
                    // i file che contengono il nome cercato vanno per primi
                    .OrderByDescending(t => t.Contains(name, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Ripiego: la pagina non ha immagine principale ma c'è un file col suo nome
                // in galleria -> ne ricavo l'URL. (Se nessun file contiene il nome preferisco
                // NON mostrare nulla piuttosto che una foto sbagliata.)
                if (main == null)
                {
                    var candidato = gallery.FirstOrDefault(t => t.Contains(name, StringComparison.OrdinalIgnoreCase));
                    if (candidato != null)
                        main = RisolviUrlFile(candidato);
                }

                return (main, gallery);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore GetImmaginiFandom('{name}'): {ex.Message}");
                return (null, new List<string>());
            }
        }

        // Da titolo file ("File:Naboo.jpg") all'URL vero dell'immagine.
        private static string? RisolviUrlFile(string titoloFile)
        {
            try
            {
                var url = $"{FandomBaseUrl}?action=query&titles={Uri.EscapeDataString(titoloFile)}" +
                          "&prop=imageinfo&iiprop=url&iiurlwidth=500&format=json";

                var risposta = _http.GetFromJsonAsync<FandomQueryResult>(url, _jsonOptions)
                    .GetAwaiter().GetResult();

                var info = risposta?.Query?.Pages?.Values.FirstOrDefault()?.ImageInfo?.FirstOrDefault();
                return info?.ThumbUrl ?? info?.Url;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore RisolviUrlFile('{titoloFile}'): {ex.Message}");
                return null;
            }
        }

        internal class OmdbResult
        {
            [JsonPropertyName("Poster")]
            public string? Poster { get; set; }
        }

        internal class FandomQueryResult
        {
            [JsonPropertyName("query")]
            public FandomQuery? Query { get; set; }
        }
        internal class FandomQuery
        {
            [JsonPropertyName("pages")]
            public Dictionary<string, FandomPage>? Pages { get; set; }
        }
        internal class FandomPage
        {
            [JsonPropertyName("thumbnail")]
            public FandomThumbnail? Thumbnail { get; set; }
            [JsonPropertyName("images")]
            public List<FandomImage>? Images { get; set; }
            [JsonPropertyName("imageinfo")]
            public List<FandomImageInfo>? ImageInfo { get; set; }
        }
        internal class FandomImageInfo
        {
            [JsonPropertyName("url")]
            public string? Url { get; set; }
            [JsonPropertyName("thumburl")]
            public string? ThumbUrl { get; set; }
        }
        internal class FandomThumbnail
        {
            [JsonPropertyName("source")]
            public string? Source { get; set; }
        }
        internal class FandomImage
        {
            [JsonPropertyName("title")]
            public string? Title { get; set; }
        }

        // ===================== DTO per la paginazione SWAPI =====================

        internal class SwapiPagina<T>
        {
            [JsonPropertyName("count")]
            public int Count { get; set; }

            [JsonPropertyName("next")]
            public string? Next { get; set; }

            [JsonPropertyName("previous")]
            public string? Previous { get; set; }

            [JsonPropertyName("results")]
            public List<T> Results { get; set; } = new();
        }
    }
}
