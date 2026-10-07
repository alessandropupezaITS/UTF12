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

        private static readonly HttpClient _http = CreaHttpClient();

        // Molte API (Fandom/MediaWiki in particolare) rifiutano le richieste senza User-Agent:
        // l'errore finiva nel try/catch e l'immagine restava semplicemente vuota.
        private static HttpClient CreaHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ProgettoUSF12/1.0 (app UWP didattica)");
            return client;
        }

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = GestioneApiJsonContext.Default
        };

        // ===================== FILM =====================
        // Ordine di ricerca per ogni tipo: memoria -> API SWAPI -> (se offline o SWAPI non risponde)
        // gli elementi che l'utente ha salvato in locale. Nel DB scrive solo il bottone
        // "Salva in locale" (vedi ArchivioLocale).

        public static List<Film> GetFilms()
        {
            return CaricaEntita<Film>(
                "films",
                film => Parallel.ForEach(film, f =>
                {
                    f.Id = SwapiIdConverter.ExtractId(f.Url);
                    f.PosterUrl = GetPosterOmdb(f.EpisodeId);
                }),
                ArchivioLocale.CaricaFilm);
        }

        // ===================== PERSONAGGI =====================

        public static List<Personaggio> GetPersonaggi(bool conImmagini = true)
        {
            var lista = CaricaEntita<Personaggio>(
                "people",
                l => l.ForEach(p => p.Id = SwapiIdConverter.ExtractId(p.Url)),
                ArchivioLocale.CaricaPersonaggi);

            if (conImmagini) ImmaginiPersonaggi(lista);
            return lista;
        }

        public static List<Personaggio> GetPersonaggiBy(Film film)
        {
            var lista = GetPersonaggi(conImmagini: false)
                .Where(p => film.PersonaggioIds.Contains(p.Id)).ToList();
            ImmaginiPersonaggi(lista);
            return lista.OrderBy(p => p.Name).ToList();
        }

        // Un solo elemento (usato dalla ricerca quando l'elemento non ha film collegati).
        public static List<Personaggio> GetPersonaggiPerId(int id)
        {
            var lista = GetPersonaggi(conImmagini: false).Where(x => x.Id == id).ToList();
            ImmaginiPersonaggi(lista);
            return lista;
        }

        private static void ImmaginiPersonaggi(List<Personaggio> lista) =>
            CompletaImmagini(lista, "characters",
                x => x.Name, x => x.Id,
                x => x.MainImage != null || x.ImageGallery.Count > 0,
                (x, main, gallery) => { x.MainImage = main; x.ImageGallery = gallery; });

        // ===================== PIANETI =====================

        public static List<Pianeta> GetPianeti(bool conImmagini = true)
        {
            var lista = CaricaEntita<Pianeta>(
                "planets",
                l => l.ForEach(p => p.Id = SwapiIdConverter.ExtractId(p.Url)),
                ArchivioLocale.CaricaPianeti);

            if (conImmagini) ImmaginiPianeti(lista);
            return lista;
        }

        public static List<Pianeta> GetPianetiBy(Film film)
        {
            var lista = GetPianeti(conImmagini: false)
                .Where(p => film.PianetaIds.Contains(p.Id)).ToList();
            ImmaginiPianeti(lista);
            return lista.OrderBy(p => p.Name).ToList();
        }

        public static List<Pianeta> GetPianetiPerId(int id)
        {
            var lista = GetPianeti(conImmagini: false).Where(x => x.Id == id).ToList();
            ImmaginiPianeti(lista);
            return lista;
        }

        private static void ImmaginiPianeti(List<Pianeta> lista) =>
            CompletaImmagini(lista, "planets",
                x => x.Name, x => x.Id,
                x => x.MainImage != null || x.ImageGallery.Count > 0,
                (x, main, gallery) => { x.MainImage = main; x.ImageGallery = gallery; });

        // ===================== RAZZE =====================

        public static List<Razza> GetRazze(bool conImmagini = true)
        {
            var lista = CaricaEntita<Razza>(
                "species",
                l => l.ForEach(r => r.Id = SwapiIdConverter.ExtractId(r.Url)),
                ArchivioLocale.CaricaRazze);

            if (conImmagini) ImmaginiRazze(lista);
            return lista;
        }

        public static List<Razza> GetRazzeBy(Film film)
        {
            var lista = GetRazze(conImmagini: false)
                .Where(r => film.RazzaIds.Contains(r.Id)).ToList();
            ImmaginiRazze(lista);
            return lista.OrderBy(r => r.Name).ToList();
        }

        public static List<Razza> GetRazzePerId(int id)
        {
            var lista = GetRazze(conImmagini: false).Where(x => x.Id == id).ToList();
            ImmaginiRazze(lista);
            return lista;
        }

        private static void ImmaginiRazze(List<Razza> lista) =>
            CompletaImmagini(lista, "species",
                x => x.Name, x => x.Id,
                x => x.MainImage != null || x.ImageGallery.Count > 0,
                (x, main, gallery) => { x.MainImage = main; x.ImageGallery = gallery; });

        // ===================== ASTRONAVI =====================

        public static List<Astronave> GetAstronavi(bool conImmagini = true)
        {
            var lista = CaricaEntita<Astronave>(
                "starships",
                l => l.ForEach(a => a.Id = SwapiIdConverter.ExtractId(a.Url)),
                ArchivioLocale.CaricaAstronavi);

            if (conImmagini) ImmaginiAstronavi(lista);
            return lista;
        }

        public static List<Astronave> GetAstronaviBy(Film film)
        {
            var lista = GetAstronavi(conImmagini: false)
                .Where(a => film.AstronaveIds.Contains(a.Id)).ToList();
            ImmaginiAstronavi(lista);
            return lista.OrderBy(a => a.Name).ToList();
        }

        public static List<Astronave> GetAstronaviPerId(int id)
        {
            var lista = GetAstronavi(conImmagini: false).Where(x => x.Id == id).ToList();
            ImmaginiAstronavi(lista);
            return lista;
        }

        private static void ImmaginiAstronavi(List<Astronave> lista) =>
            CompletaImmagini(lista, "starships",
                x => x.Name, x => x.Id,
                x => x.MainImage != null || x.ImageGallery.Count > 0,
                (x, main, gallery) => { x.MainImage = main; x.ImageGallery = gallery; });

        // ===================== HELPER =====================

        // ===================== CACHE IN MEMORIA =====================

        private static readonly ConcurrentDictionary<string, object> _cacheEntita = new();

        private static long _ultimoFallimento;   // UTC ticks dell'ultimo download SWAPI fallito

        // true se non c'è internet, oppure se SWAPI ha appena fallito (ogni tentativo a vuoto
        // costa secondi: per 30 secondi non si riprova e si usano i dati salvati).
        public static bool IsOffline
        {
            get
            {
                try
                {
                    var profilo = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
                    if (profilo == null ||
                        profilo.GetNetworkConnectivityLevel() != Windows.Networking.Connectivity.NetworkConnectivityLevel.InternetAccess)
                        return true;
                }
                catch { /* se non si riesce a saperlo, si prova comunque */ }

                var trascorsi = DateTime.UtcNow.Ticks - System.Threading.Interlocked.Read(ref _ultimoFallimento);
                return trascorsi < TimeSpan.FromSeconds(30).Ticks;
            }
        }

        // Memoria -> API -> salvataggi dell'utente. I dati salvati usati come ripiego NON vanno
        // in cache: appena torna la rete si scarica di nuovo la lista completa.
        private static List<T> CaricaEntita<T>(
            string endpoint,
            Action<List<T>> completaDaApi,       // imposta Id (e poster per i film) sui dati appena scaricati
            Func<string, List<T>> daLocale)      // elementi salvati dall'utente (parametro = chiave utente)
        {
            lock (_lockSwapi.GetOrAdd(endpoint, _ => new object()))
            {
                if (_cacheEntita.TryGetValue(endpoint, out var inMemoria))
                    return (List<T>)inMemoria;

                if (!IsOffline)
                {
                    var lista = ScaricaTutto<T>(endpoint);
                    if (lista.Count > 0)
                    {
                        completaDaApi(lista);

                        // ScaricaTutto mette in cache solo i download completi
                        if (_cacheSwapi.ContainsKey(endpoint))
                            _cacheEntita[endpoint] = lista;
                        else
                            System.Diagnostics.Debug.WriteLine($"'{endpoint}': download INCOMPLETO, non messo in cache");

                        return lista;
                    }

                    System.Threading.Interlocked.Exchange(ref _ultimoFallimento, DateTime.UtcNow.Ticks);
                }

                return Salvati(endpoint, daLocale);
            }
        }

        private static List<T> Salvati<T>(string endpoint, Func<string, List<T>> daLocale)
        {
            var utente = GestioneUtente.ChiaveCorrente;
            if (utente == null) return new List<T>();

            try
            {
                var salvati = daLocale(utente);
                System.Diagnostics.Debug.WriteLine($"[OFFLINE] '{endpoint}': {salvati.Count} elementi salvati dall'utente");
                return salvati;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DB locale non disponibile per '{endpoint}': {ex.Message}");
                return new List<T>();
            }
        }

        // Scarica le immagini (Visual Guide / Fandom) solo per gli elementi che non le hanno ancora.
        private static void CompletaImmagini<T>(
            List<T> lista, string tipo,
            Func<T, string> nome, Func<T, int> id, Func<T, bool> haImmagini,
            Action<T, string?, List<string>> imposta)
        {
            var daCompletare = lista.Where(x => !haImmagini(x)).ToList();
            if (daCompletare.Count == 0 || IsOffline) return;

            Parallel.ForEach(daCompletare, new ParallelOptions { MaxDegreeOfParallelism = 6 }, x =>
            {
                var (main, gallery) = GetImmagini(nome(x), tipo, id(x));
                imposta(x, main, gallery);
            });
        }


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
                        var pagina = ScaricaPagina<T>(url);

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

        // Fino a 3 tentativi per pagina: un errore momentaneo non deve rendere incompleto
        // l'intero download (e quindi impedire il salvataggio nel DB).
        private static SwapiPagina<T>? ScaricaPagina<T>(string url)
        {
            for (int tentativo = 1; ; tentativo++)
            {
                try
                {
                    return _http.GetFromJsonAsync<SwapiPagina<T>>(url, _jsonOptions)
                        .GetAwaiter().GetResult();
                }
                catch when (tentativo < 3)
                {
                    System.Threading.Thread.Sleep(500 * tentativo);
                }
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

            // Se l'utente ha già salvato questo elemento la foto è nota: niente rete.
            var utente = GestioneUtente.ChiaveCorrente;
            if (utente != null)
            {
                try
                {
                    var salvata = ArchivioLocale.ImmaginePrincipale(utente, categoria, id);
                    if (!string.IsNullOrEmpty(salvata)) return salvata;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Foto salvata non leggibile: {ex.Message}");
                }
            }
            if (IsOffline) return null;

            var urlGuide = $"{VisualGuideBaseUrl}{tipo}/{id}.jpg";
            return UrlEsiste(urlGuide) ? urlGuide : GetImmaginiFandom(nome).main;
        }

        // Galleria per il carosello di PaginaDettaglio.
        // ImageGallery contiene TITOLI di file Fandom ("File:Naboo.jpg"), non URL: qui ne
        // risolvo al massimo `max` in URL veri. Come per l'immagine principale tengo solo i file
        // che contengono il nome dell'elemento (meglio nessuna foto che una foto sbagliata).
        public static List<string> GetUrlGalleria(string nome, List<string> titoliFile, int max = 8)
        {
            if (IsOffline) return new List<string>();

            var scelti = titoliFile
                .Where(t => ContieneNome(t, nome))
                .Take(max)
                .ToList();

            var urls = new string?[scelti.Count];
            Parallel.For(0, scelti.Count, new ParallelOptions { MaxDegreeOfParallelism = 4 },
                i => urls[i] = RisolviUrlFile(scelti[i]));

            return urls.Where(u => !string.IsNullOrEmpty(u)).Select(u => u!).ToList();
        }

        // Scarica i byte di un'immagine con lo stesso HttpClient delle API (User-Agent incluso) e il
        // Referer di Fandom. Serve perché il caricatore di immagini di XAML (BitmapImage + Uri)
        // sui file di static.wikia.nocookie.net falliva con E_NETWORK_ERROR anche se l'URL era valido.
        public static byte[]? ScaricaBytes(string url)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Referrer = new Uri("https://starwars.fandom.com/");
                using var resp = _http.SendAsync(req).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[IMG] ScaricaBytes HTTP {(int)resp.StatusCode} {url}");
                    return null;
                }
                return resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[IMG] ScaricaBytes('{url}'): {ex.Message}");
                return null;
            }
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

        // Confronto tra titolo di un file Fandom e il nome dell'elemento ignorando spazi, "_", "-",
        // maiuscole e il prefisso "File:". Prima "File:BarrissOffee.jpg" o "File:Barriss_Offee_ROTS.png"
        // NON contenevano "Barriss Offee" (con lo spazio): tutti i file venivano scartati e
        // la scheda restava senza immagini anche se la galleria ne aveva decine.
        private static string Normalizza(string s) =>
            new string(s.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

        private static bool ContieneNome(string titoloFile, string nome)
        {
            var n = Normalizza(nome);
            if (n.Length == 0) return false;
            var t = titoloFile.StartsWith("File:", StringComparison.OrdinalIgnoreCase) ? titoloFile.Substring(5) : titoloFile;
            return Normalizza(t).Contains(n);
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
                    .OrderByDescending(t => ContieneNome(t, name))
                    .ToList();

                System.Diagnostics.Debug.WriteLine(
                    $"[IMG] Fandom '{name}': pagina={(pagina == null ? "NON TROVATA" : "ok")} | thumbnail={(main ?? "null")} | file in galleria={gallery.Count}");

                // Ripiego: la pagina non ha immagine principale ma c'è un file col suo nome
                // in galleria -> ne ricavo l'URL. (Se nessun file contiene il nome preferisco
                // NON mostrare nulla piuttosto che una foto sbagliata.)
                if (main == null)
                {
                    var candidato = gallery.FirstOrDefault(t => ContieneNome(t, name));
                    System.Diagnostics.Debug.WriteLine($"[IMG] '{name}': nessun thumbnail, candidato dalla galleria = {candidato ?? "NESSUNO"}");
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
