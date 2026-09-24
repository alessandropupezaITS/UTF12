using ProgettoUSF12.BackEnd.Classi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    // ============================================================
    public static class GestioneAPI
    {
        private const string SwapiBaseUrl = "https://swapi.dev/api/";
        private const string OmdbBaseUrl = "https://www.omdbapi.com/";
        private const string OmdbApiKey = "LA_TUA_CHIAVE_OMDB_QUI"; // <-- inserisci qui la chiave ricevuta via email
        private const string FandomBaseUrl = "https://starwars.fandom.com/api.php";

        private static readonly HttpClient _http = new HttpClient();

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = GestioneApiJsonContext.Default
        };

        // ===================== FILM =====================

        public static List<Film> GetFilms()
        {
            var film = ScaricaTutto<Film>("films");

            foreach (var f in film)
            {
                f.Id = SwapiIdConverter.ExtractId(f.Url);
                f.PosterUrl = GetPosterOmdb(f.Title);
            }

            return film;
        }

        // ===================== PERSONAGGI =====================

        public static List<Personaggio> GetPersonaggi()
        {
            var personaggi = ScaricaTutto<Personaggio>("people");

            foreach (var p in personaggi)
            {
                p.Id = SwapiIdConverter.ExtractId(p.Url);
                var (main, gallery) = GetImmaginiFandom(p.Name);
                p.MainImage = main;
                p.ImageGallery = gallery;
            }

            return personaggi;
        }

        // Solo i personaggi che compaiono in un film specifico
        public static List<Personaggio> GetPersonaggiBy(Film film) =>
            GetPersonaggi().Where(p => film.PersonaggioIds.Contains(p.Id)).ToList();

        // ===================== PIANETI =====================

        public static List<Pianeta> GetPianeti()
        {
            var pianeti = ScaricaTutto<Pianeta>("planets");

            foreach (var p in pianeti)
            {
                p.Id = SwapiIdConverter.ExtractId(p.Url);
                var (main, gallery) = GetImmaginiFandom(p.Name);
                p.MainImage = main;
                p.ImageGallery = gallery;
            }

            return pianeti;
        }

        // Solo i pianeti che compaiono in un film specifico
        public static List<Pianeta> GetPianetiBy(Film film) =>
            GetPianeti().Where(p => film.PianetaIds.Contains(p.Id)).ToList();

        // ===================== RAZZE =====================

        public static List<Razza> GetRazze()
        {
            var razze = ScaricaTutto<Razza>("species");

            foreach (var r in razze)
            {
                r.Id = SwapiIdConverter.ExtractId(r.Url);
                var (main, gallery) = GetImmaginiFandom(r.Name);
                r.MainImage = main;
                r.ImageGallery = gallery;
            }

            return razze;
        }

        // Solo le razze che compaiono in un film specifico
        public static List<Razza> GetRazzeBy(Film film) =>
            GetRazze().Where(r => film.RazzaIds.Contains(r.Id)).ToList();

        // ===================== ASTRONAVI =====================

        public static List<Astronave> GetAstronavi()
        {
            var astronavi = ScaricaTutto<Astronave>("starships");

            foreach (var a in astronavi)
            {
                a.Id = SwapiIdConverter.ExtractId(a.Url);
                var (main, gallery) = GetImmaginiFandom(a.Name);
                a.MainImage = main;
                a.ImageGallery = gallery;
            }

            return astronavi;
        }

        // Solo le astronavi che compaiono in un film specifico
        public static List<Astronave> GetAstronaviBy(Film film) =>
            GetAstronavi().Where(a => film.AstronaveIds.Contains(a.Id)).ToList();

        // ===================== HELPER =====================

        // Scarica tutte le pagine di un endpoint SWAPI (bloccante:
        // aspetta subito il risultato con GetAwaiter().GetResult(),
        // così il metodo pubblico non deve essere async)
        private static List<T> ScaricaTutto<T>(string endpointSwapi)
        {
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

            return risultati;
        }

        // Recupera l'URL del poster del film da OMDb (ricerca per titolo).
        private static string? GetPosterOmdb(string title)
        {
            try
            {
                var url = $"{OmdbBaseUrl}?apikey={OmdbApiKey}&t={Uri.EscapeDataString(title)}";
                var risposta = _http.GetFromJsonAsync<OmdbResult>(url).GetAwaiter().GetResult();

                // OMDb ritorna la stringa "N/A" quando non ha un poster disponibile
                return string.IsNullOrEmpty(risposta?.Poster) || risposta.Poster == "N/A"
                    ? null
                    : risposta.Poster;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore GetPosterOmdb('{title}'): {ex.Message}");
                return null;
            }
        }

        // Placeholder: nessuna chiamata reale ancora, ritorna liste vuote.
        private static (string? main, List<string> gallery) GetImmaginiFandom(string name)
        {
            try
            {
                var url = $"{FandomBaseUrl}?action=query&titles={Uri.EscapeDataString(name)}" +
                          "&prop=pageimages|images&format=json&pithumbsize=500";

                var risposta = _http.GetFromJsonAsync<FandomQueryResult>(url).GetAwaiter().GetResult();

                var pagina = risposta?.Query?.Pages?.Values.FirstOrDefault();
                var main = pagina?.Thumbnail?.Source;
                var gallery = pagina?.Images?
                    .Select(i => i.Title)
                    .Where(t => t != null && (t.EndsWith(".jpg") || t.EndsWith(".png")))
                    .ToList() ?? new List<string>();

                return (main, gallery);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore GetImmaginiFandom('{name}'): {ex.Message}");
                return (null, new List<string>());
            }
        }

        // DTO minimo per deserializzare la risposta di OMDb
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