using System.Text.Json.Serialization;
using ProgettoUSF12.BackEnd.Classi;

namespace ProgettoUSF12.BackEnd.Services
{
    // ============================================================
    // Context JSON generato a compile-time (source generator).
    // ------------------------------------------------------------
    // IMPORTANTE: il corpo di questa classe deve restare VUOTO.
    // Non implementare "a mano" i membri astratti di JsonSerializerContext
    // (GetTypeInfo, GeneratedSerializerOptions, costruttori): se lo fai,
    // vanno in conflitto col codice che il compilatore genera da solo
    // grazie a [JsonSerializable] + "partial". Se Visual Studio ti propone
    // la lampadina "Implementa membri astratti" su questa classe, NON
    // accettarla: è un suggerimento dell'IDE che non sa ancora che il
    // generatore riempirà tutto lui al build.
    //
    // In questo progetto la deserializzazione JSON basata su reflection
    // non funziona a runtime. QUALSIASI tipo che passa per
    // _http.GetFromJsonAsync<T>(url, _jsonOptions) deve essere elencato
    // qui sotto con [JsonSerializable] — se manca anche solo un tipo,
    // quella singola chiamata fallisce silenziosamente (catturata dal
    // try/catch) e ritorna null/vuoto senza nessun errore visibile.
    //
    // ScaricaTutto<T> in GestioneAPI è generica e viene chiamata con 5
    // tipi diversi (Film, Personaggio, Pianeta, Razza, Astronave), quindi
    // ognuno ha bisogno della propria riga per SwapiPagina<T>: il source
    // generator lavora solo su tipi chiusi/concreti, non sul generico T.
    // ============================================================
    [JsonSerializable(typeof(Film))]
    [JsonSerializable(typeof(Personaggio))]
    [JsonSerializable(typeof(Pianeta))]
    [JsonSerializable(typeof(Razza))]
    [JsonSerializable(typeof(Astronave))]
    [JsonSerializable(typeof(GestioneAPI.SwapiPagina<Film>))]
    [JsonSerializable(typeof(GestioneAPI.SwapiPagina<Personaggio>))]
    [JsonSerializable(typeof(GestioneAPI.SwapiPagina<Pianeta>))]
    [JsonSerializable(typeof(GestioneAPI.SwapiPagina<Razza>))]
    [JsonSerializable(typeof(GestioneAPI.SwapiPagina<Astronave>))]
    [JsonSerializable(typeof(GestioneAPI.OmdbResult))]
    [JsonSerializable(typeof(GestioneAPI.FandomQueryResult))]
    internal partial class GestioneApiJsonContext : JsonSerializerContext
    {
    }
}
