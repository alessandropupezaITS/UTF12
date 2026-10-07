using ProgettoUSF12.BackEnd.Classi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgettoUSF12.BackEnd.Services
{
    // L'ordine dei valori DEVE coincidere con l'ordine delle voci del ComboBox in BarraSuperiore.xaml.
    public enum TipoRicerca
    {
        Tutto = 0,
        Film = 1,
        Personaggi = 2,
        Pianeti = 3,
        Razze = 4,
        Astronavi = 5
    }

    // Una riga dei suggerimenti della barra di ricerca.
    public class RisultatoRicerca
    {
        public TipoRicerca Categoria { get; set; }
        public string Nome { get; set; } = "";
        public string Tipo { get; set; } = "";      // etichetta mostrata a destra ("Personaggio", ...)
        public int Id { get; set; }
        public int? FilmId { get; set; }            // primo film in cui compare (null = nessun film)

        public override string ToString() => Nome;

        // Riga di servizio ("Caricamento...", "Nessun risultato"): non è cliccabile.
        public static RisultatoRicerca Messaggio(string testo) =>
            new RisultatoRicerca { Categoria = TipoRicerca.Tutto, Nome = testo };
    }

    // Parametro di navigazione per PaginaDettaglio.
    //  - Categoria: cosa mostrare (Personaggi, Pianeti, Razze o Astronavi)
    //  - FilmId valorizzato: mostra tutti gli elementi di quel film
    //  - EntitaId valorizzato: porta in vista (o mostra da solo, se non ha film) quell'elemento
    public class ParametroDettaglio
    {
        public TipoRicerca Categoria { get; set; }
        public int? FilmId { get; set; }
        public int? EntitaId { get; set; }
    }

    // Indice di ricerca in memoria: scarica UNA volta sola (in background) film, personaggi,
    // pianeti, razze e astronavi (senza immagini) e poi cerca sul posto, senza rete.
    public static class RicercaGlobale
    {
        private static Task? _caricamento;
        private static bool _completo;
        private static List<RisultatoRicerca> _indice = new();
        private static List<Film> _film = new();

        public static bool Pronta =>
            _caricamento != null && _caricamento.Status == TaskStatus.RanToCompletion && _indice.Count > 0;

        public static Task CaricaAsync()
        {
            // Se un tentativo precedente è finito con dati incompleti (rete assente...) riprova.
            if (_caricamento != null && _caricamento.IsCompleted && !_completo)
                _caricamento = null;

            return _caricamento ??= Task.Run(Costruisci);
        }

        // Tutti gli elementi dell'indice (o solo una categoria), ordinati per categoria e nome.
        // Usato dall'Archivio: condivide lo stesso download della barra di ricerca.
        public static List<RisultatoRicerca> Elenco(TipoRicerca filtro) =>
            _indice
                .Where(r => filtro == TipoRicerca.Tutto || r.Categoria == filtro)
                .OrderBy(r => r.Categoria)
                .ThenBy(r => r.Nome)
                .ToList();

        public static Film? TrovaFilm(int id) => _film.FirstOrDefault(f => f.Id == id);

        private static void Costruisci()
        {
            var tFilm = Task.Run(() => GestioneAPI.GetFilms());
            var tPers = Task.Run(() => GestioneAPI.GetPersonaggi(conImmagini: false));
            var tPian = Task.Run(() => GestioneAPI.GetPianeti(conImmagini: false));
            var tRazze = Task.Run(() => GestioneAPI.GetRazze(conImmagini: false));
            var tAstro = Task.Run(() => GestioneAPI.GetAstronavi(conImmagini: false));
            Task.WaitAll(tFilm, tPers, tPian, tRazze, tAstro);

            var indice = new List<RisultatoRicerca>();

            foreach (var f in tFilm.Result)
                indice.Add(new RisultatoRicerca
                {
                    Categoria = TipoRicerca.Film, Tipo = "Film", Nome = f.Title, Id = f.Id
                });

            foreach (var p in tPers.Result)
                indice.Add(Crea(TipoRicerca.Personaggi, "Personaggio", p.Name, p.Id, p.FilmIds));
            foreach (var p in tPian.Result)
                indice.Add(Crea(TipoRicerca.Pianeti, "Pianeta", p.Name, p.Id, p.FilmIds));
            foreach (var r in tRazze.Result)
                indice.Add(Crea(TipoRicerca.Razze, "Razza", r.Name, r.Id, r.FilmIds));
            foreach (var a in tAstro.Result)
                indice.Add(Crea(TipoRicerca.Astronavi, "Astronave", a.Name, a.Id, a.FilmIds));

            _film = tFilm.Result;
            _indice = indice;
            _completo = tFilm.Result.Count > 0 && tPers.Result.Count > 0 && tPian.Result.Count > 0
                        && tRazze.Result.Count > 0 && tAstro.Result.Count > 0
                        && !GestioneAPI.IsOffline;   // dati salvati usati offline = incompleti: riprova online
        }

        private static RisultatoRicerca Crea(TipoRicerca cat, string tipo, string nome, int id, List<int> filmIds) =>
            new RisultatoRicerca
            {
                Categoria = cat,
                Tipo = tipo,
                Nome = nome,
                Id = id,
                FilmId = filmIds.Count > 0 ? filmIds.Min() : (int?)null
            };

        // Cerca per nome (contiene, senza distinguere maiuscole). I nomi che INIZIANO col testo vengono prima.
        public static List<RisultatoRicerca> Cerca(string testo, TipoRicerca filtro, int max = 30)
        {
            testo = (testo ?? "").Trim();
            if (testo.Length == 0) return new List<RisultatoRicerca>();

            return _indice
                .Where(r => (filtro == TipoRicerca.Tutto || r.Categoria == filtro)
                            && r.Nome.Contains(testo, StringComparison.CurrentCultureIgnoreCase))
                .OrderByDescending(r => r.Nome.StartsWith(testo, StringComparison.CurrentCultureIgnoreCase))
                .ThenBy(r => r.Categoria)
                .ThenBy(r => r.Nome)
                .Take(max)
                .ToList();
        }
    }
}
