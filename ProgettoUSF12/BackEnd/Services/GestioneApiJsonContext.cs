using ProgettoUSF12.BackEnd.Classi;
using System.Text.Json.Serialization;

namespace ProgettoUSF12.BackEnd.Services
{
    // ============================================================
    // Context JSON generato a compile-time (source generator).
    // ------------------------------------------------------------
    // La classe resta vuota di proposito: costruttore, GetTypeInfo
    // e GeneratedSerializerOptions li scrive automaticamente il
    // compilatore in base agli attributi [JsonSerializable] qui
    // sotto. Scriverli a mano crea un doppione col codice generato
    // (causa degli errori CS0102/CS0111 che avevi).
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
    internal partial class GestioneApiJsonContext : JsonSerializerContext
    {
    }
}