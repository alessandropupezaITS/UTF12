using Microsoft.EntityFrameworkCore;
using ProgettoUSF12.BackEnd.Classi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace ProgettoUSF12.BackEnd.Services
{
    // ============================================================
    // ArchivioLocale - elementi salvati IN LOCALE dall'utente (EF Core + SQLite).
    // ------------------------------------------------------------
    // Niente più cache automatica delle chiamate API: un elemento finisce qui
    // solo quando l'utente preme "Salva in locale" in PaginaDettaglio.
    //
    // Ogni riga appartiene a un utente: la chiave è (UtenteKey, Id), quindi
    // utenti diversi possono salvare lo stesso personaggio e ognuno vede
    // solo i propri elementi. UtenteKey = GestioneUtente.ChiaveCorrente.
    //
    // Le liste (ID e immagini) sono salvate come testo: "1,2,3" e "url1|url2".
    // ============================================================
    public static class ArchivioLocale
    {
        // ---------------- Operazioni generiche ----------------

        public static bool Esiste(string utente, TipoRicerca categoria, int id)
        {
            using var db = NuovoContesto();
            return categoria switch
            {
                TipoRicerca.Film       => db.Film.Any(x => x.UtenteKey == utente && x.Id == id),
                TipoRicerca.Personaggi => db.Personaggi.Any(x => x.UtenteKey == utente && x.Id == id),
                TipoRicerca.Pianeti    => db.Pianeti.Any(x => x.UtenteKey == utente && x.Id == id),
                TipoRicerca.Razze      => db.Razze.Any(x => x.UtenteKey == utente && x.Id == id),
                TipoRicerca.Astronavi  => db.Astronavi.Any(x => x.UtenteKey == utente && x.Id == id),
                _ => false
            };
        }

        public static void Rimuovi(string utente, TipoRicerca categoria, int id)
        {
            using var db = NuovoContesto();
            switch (categoria)
            {
                case TipoRicerca.Film:       Elimina<FilmRecord>(db, utente, id); break;
                case TipoRicerca.Personaggi: Elimina<PersonaggioRecord>(db, utente, id); break;
                case TipoRicerca.Pianeti:    Elimina<PianetaRecord>(db, utente, id); break;
                case TipoRicerca.Razze:      Elimina<RazzaRecord>(db, utente, id); break;
                case TipoRicerca.Astronavi:  Elimina<AstronaveRecord>(db, utente, id); break;
            }
        }

        // Salva (o aggiorna) per l'utente un Film, Personaggio, Pianeta, Razza o Astronave.
        public static void Salva(string utente, object entita) => SalvaMolti(utente, new[] { entita });

        // Salva più elementi in un colpo solo (un'unica scrittura sul DB).
        // sovrascrivi=false: gli elementi già salvati dall'utente restano come sono (foto comprese).
        public static void SalvaMolti(string utente, IEnumerable<object> elementi, bool sovrascrivi = true)
        {
            using var db = NuovoContesto();
            foreach (var e in elementi)
            {
                if (!sovrascrivi && GiaSalvato(db, utente, e)) continue;
                Aggiungi(db, utente, e);
            }
            db.SaveChanges();
        }

        private static void Aggiungi(ArchivioDbContext db, string utente, object entita)
        {
            switch (entita)
            {
                case Film fi:
                    Upsert(db, new FilmRecord
                    {
                        UtenteKey = utente,
                        Id = fi.Id, Url = fi.Url ?? "", Title = fi.Title ?? "", EpisodeId = fi.EpisodeId,
                        OpeningCrawl = fi.OpeningCrawl ?? "", Director = fi.Director ?? "", ReleaseDate = fi.ReleaseDate ?? "",
                        PersonaggioIds = Testo(fi.PersonaggioIds), PianetaIds = Testo(fi.PianetaIds),
                        RazzaIds = Testo(fi.RazzaIds), AstronaveIds = Testo(fi.AstronaveIds),
                        PosterUrl = fi.PosterUrl
                    });
                    break;

                case Personaggio pe:
                    Upsert(db, new PersonaggioRecord
                    {
                        UtenteKey = utente,
                        Id = pe.Id, Url = pe.Url ?? "", Name = pe.Name ?? "", Height = pe.Height ?? "", Mass = pe.Mass ?? "",
                        BirthYear = pe.BirthYear ?? "", EyeColor = pe.EyeColor ?? "", HairColor = pe.HairColor ?? "",
                        Gender = pe.Gender ?? "", HomeworldId = pe.HomeworldId,
                        SpeciesIds = Testo(pe.SpeciesIds), FilmIds = Testo(pe.FilmIds), StarshipIds = Testo(pe.StarshipIds),
                        MainImage = pe.MainImage, ImageGallery = Testo(pe.ImageGallery)
                    });
                    break;

                case Pianeta pi:
                    Upsert(db, new PianetaRecord
                    {
                        UtenteKey = utente,
                        Id = pi.Id, Url = pi.Url ?? "", Name = pi.Name ?? "", Diameter = pi.Diameter ?? "",
                        RotationPeriod = pi.RotationPeriod ?? "", Gravity = pi.Gravity ?? "", Population = pi.Population ?? "",
                        Climate = pi.Climate ?? "", SurfaceWater = pi.SurfaceWater ?? "", Terrain = pi.Terrain ?? "",
                        FilmIds = Testo(pi.FilmIds),
                        MainImage = pi.MainImage, ImageGallery = Testo(pi.ImageGallery)
                    });
                    break;

                case Razza ra:
                    Upsert(db, new RazzaRecord
                    {
                        UtenteKey = utente,
                        Id = ra.Id, Url = ra.Url ?? "", Name = ra.Name ?? "", Classification = ra.Classification ?? "",
                        AverageHeight = ra.AverageHeight ?? "", AverageLifespan = ra.AverageLifespan ?? "",
                        Language = ra.Language ?? "", HomeworldId = ra.HomeworldId,
                        PersonaggioIds = Testo(ra.PersonaggioIds), FilmIds = Testo(ra.FilmIds),
                        MainImage = ra.MainImage, ImageGallery = Testo(ra.ImageGallery)
                    });
                    break;

                case Astronave a:
                    Upsert(db, new AstronaveRecord
                    {
                        UtenteKey = utente,
                        Id = a.Id, Url = a.Url ?? "", Name = a.Name ?? "", Model = a.Model ?? "",
                        StarshipClass = a.StarshipClass ?? "", Manufacturer = a.Manufacturer ?? "", Crew = a.Crew ?? "",
                        Passengers = a.Passengers ?? "", Mglt = a.Mglt ?? "", HyperdriveRating = a.HyperdriveRating ?? "",
                        FilmIds = Testo(a.FilmIds), PilotIds = Testo(a.PilotIds),
                        MainImage = a.MainImage, ImageGallery = Testo(a.ImageGallery)
                    });
                    break;

                default:
                    throw new ArgumentException($"Tipo non salvabile: {entita?.GetType().Name}");
            }
        }

        // ---------------- Lettura (solo gli elementi di QUESTO utente) ----------------
        // Usate anche offline: GestioneAPI le usa al posto di SWAPI quando la rete non c'è.

        public static List<Film> CaricaFilm(string utente)
        {
            using var db = NuovoContesto();
            return db.Film.AsNoTracking().Where(x => x.UtenteKey == utente).ToList().Select(ToFilm).ToList();
        }

        public static List<Personaggio> CaricaPersonaggi(string utente)
        {
            using var db = NuovoContesto();
            return db.Personaggi.AsNoTracking().Where(x => x.UtenteKey == utente).ToList().Select(ToPersonaggio).ToList();
        }

        public static List<Pianeta> CaricaPianeti(string utente)
        {
            using var db = NuovoContesto();
            return db.Pianeti.AsNoTracking().Where(x => x.UtenteKey == utente).ToList().Select(ToPianeta).ToList();
        }

        public static List<Razza> CaricaRazze(string utente)
        {
            using var db = NuovoContesto();
            return db.Razze.AsNoTracking().Where(x => x.UtenteKey == utente).ToList().Select(ToRazza).ToList();
        }

        public static List<Astronave> CaricaAstronavi(string utente)
        {
            using var db = NuovoContesto();
            return db.Astronavi.AsNoTracking().Where(x => x.UtenteKey == utente).ToList().Select(ToAstronave).ToList();
        }

        // ---------------- Lettura di un singolo elemento salvato ----------------

        public static Personaggio? CaricaPersonaggio(string utente, int id)
        {
            using var db = NuovoContesto();
            var r = db.Personaggi.AsNoTracking().FirstOrDefault(x => x.UtenteKey == utente && x.Id == id);
            return r == null ? null : ToPersonaggio(r);
        }

        public static Pianeta? CaricaPianeta(string utente, int id)
        {
            using var db = NuovoContesto();
            var r = db.Pianeti.AsNoTracking().FirstOrDefault(x => x.UtenteKey == utente && x.Id == id);
            return r == null ? null : ToPianeta(r);
        }

        public static Razza? CaricaRazza(string utente, int id)
        {
            using var db = NuovoContesto();
            var r = db.Razze.AsNoTracking().FirstOrDefault(x => x.UtenteKey == utente && x.Id == id);
            return r == null ? null : ToRazza(r);
        }

        public static Astronave? CaricaAstronave(string utente, int id)
        {
            using var db = NuovoContesto();
            var r = db.Astronavi.AsNoTracking().FirstOrDefault(x => x.UtenteKey == utente && x.Id == id);
            return r == null ? null : ToAstronave(r);
        }

        // Foto principale già salvata (null se l'elemento non è salvato o non ha foto).
        public static string? ImmaginePrincipale(string utente, TipoRicerca categoria, int id)
        {
            using var db = NuovoContesto();
            return categoria switch
            {
                TipoRicerca.Personaggi => db.Personaggi.Where(x => x.UtenteKey == utente && x.Id == id).Select(x => x.MainImage).FirstOrDefault(),
                TipoRicerca.Pianeti    => db.Pianeti.Where(x => x.UtenteKey == utente && x.Id == id).Select(x => x.MainImage).FirstOrDefault(),
                TipoRicerca.Razze      => db.Razze.Where(x => x.UtenteKey == utente && x.Id == id).Select(x => x.MainImage).FirstOrDefault(),
                TipoRicerca.Astronavi  => db.Astronavi.Where(x => x.UtenteKey == utente && x.Id == id).Select(x => x.MainImage).FirstOrDefault(),
                _ => null
            };
        }

        // ---------------- Da riga del DB a classe ----------------

        private static Film ToFilm(FilmRecord r) => new Film
        {
            Id = r.Id, Url = r.Url, Title = r.Title, EpisodeId = r.EpisodeId,
            OpeningCrawl = r.OpeningCrawl, Director = r.Director, ReleaseDate = r.ReleaseDate,
            PersonaggioIds = Ids(r.PersonaggioIds), PianetaIds = Ids(r.PianetaIds),
            RazzaIds = Ids(r.RazzaIds), AstronaveIds = Ids(r.AstronaveIds),
            PosterUrl = r.PosterUrl
        };

        private static Personaggio ToPersonaggio(PersonaggioRecord r) => new Personaggio
        {
            Id = r.Id, Url = r.Url, Name = r.Name, Height = r.Height, Mass = r.Mass,
            BirthYear = r.BirthYear, EyeColor = r.EyeColor, HairColor = r.HairColor, Gender = r.Gender,
            HomeworldId = r.HomeworldId, SpeciesIds = Ids(r.SpeciesIds), FilmIds = Ids(r.FilmIds),
            StarshipIds = Ids(r.StarshipIds),
            MainImage = r.MainImage, ImageGallery = Gallery(r.ImageGallery)
        };

        private static Pianeta ToPianeta(PianetaRecord r) => new Pianeta
        {
            Id = r.Id, Url = r.Url, Name = r.Name, Diameter = r.Diameter, RotationPeriod = r.RotationPeriod,
            Gravity = r.Gravity, Population = r.Population, Climate = r.Climate,
            SurfaceWater = r.SurfaceWater, Terrain = r.Terrain, FilmIds = Ids(r.FilmIds),
            MainImage = r.MainImage, ImageGallery = Gallery(r.ImageGallery)
        };

        private static Razza ToRazza(RazzaRecord r) => new Razza
        {
            Id = r.Id, Url = r.Url, Name = r.Name, Classification = r.Classification,
            AverageHeight = r.AverageHeight, AverageLifespan = r.AverageLifespan, Language = r.Language,
            HomeworldId = r.HomeworldId, PersonaggioIds = Ids(r.PersonaggioIds), FilmIds = Ids(r.FilmIds),
            MainImage = r.MainImage, ImageGallery = Gallery(r.ImageGallery)
        };

        private static Astronave ToAstronave(AstronaveRecord r) => new Astronave
        {
            Id = r.Id, Url = r.Url, Name = r.Name, Model = r.Model, StarshipClass = r.StarshipClass,
            Manufacturer = r.Manufacturer, Crew = r.Crew, Passengers = r.Passengers, Mglt = r.Mglt,
            HyperdriveRating = r.HyperdriveRating, FilmIds = Ids(r.FilmIds), PilotIds = Ids(r.PilotIds),
            MainImage = r.MainImage, ImageGallery = Gallery(r.ImageGallery)
        };

        // ============================================================
        // Helper
        // ============================================================
        private static readonly object _lockCreazione = new();
        private static bool _creato;

        private static ArchivioDbContext NuovoContesto()
        {
            var db = new ArchivioDbContext();
            if (!_creato)
            {
                lock (_lockCreazione)
                {
                    if (!_creato)
                    {
                        db.Database.EnsureCreated();   // crea il file e le tabelle se mancano
                        _creato = true;
                    }
                }
            }
            return db;
        }

        private static void Upsert<TR>(ArchivioDbContext db, TR record) where TR : class, IRecord
        {
            var set = db.Set<TR>();
            var esistente = set.Find(record.UtenteKey, record.Id);   // chiave composta: (UtenteKey, Id)
            if (esistente == null) set.Add(record);
            else
            {
                record.DataSalvataggio = esistente.DataSalvataggio;
                db.Entry(esistente).CurrentValues.SetValues(record);
            }
        }

        private static bool GiaSalvato(ArchivioDbContext db, string utente, object e) => e switch
        {
            Film f        => db.Film.Any(x => x.UtenteKey == utente && x.Id == f.Id),
            Personaggio p => db.Personaggi.Any(x => x.UtenteKey == utente && x.Id == p.Id),
            Pianeta p     => db.Pianeti.Any(x => x.UtenteKey == utente && x.Id == p.Id),
            Razza r       => db.Razze.Any(x => x.UtenteKey == utente && x.Id == r.Id),
            Astronave a   => db.Astronavi.Any(x => x.UtenteKey == utente && x.Id == a.Id),
            _ => false
        };

        private static void Elimina<TR>(ArchivioDbContext db, string utente, int id) where TR : class, IRecord
        {
            var set = db.Set<TR>();
            var r = set.Find(utente, id);
            if (r == null) return;
            set.Remove(r);
            db.SaveChanges();
        }

        private static string Testo(List<int>? l) => l == null ? "" : string.Join(",", l);
        private static string Testo(List<string>? l) => l == null ? "" : string.Join("|", l);

        private static List<int> Ids(string? s) =>
            string.IsNullOrEmpty(s)
                ? new List<int>()
                : s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.TryParse(x, out var n) ? n : 0).ToList();

        private static List<string> Gallery(string? s) =>
            string.IsNullOrEmpty(s)
                ? new List<string>()
                : s.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    // ============================================================
    // Contesto e tabelle (interni a questo file)
    // ============================================================
    internal interface IRecord
    {
        string UtenteKey { get; set; }           // a chi appartiene il salvataggio
        int Id { get; set; }                     // Id SWAPI
        DateTime DataSalvataggio { get; set; }   // quando è stato salvato la prima volta
    }

    internal class ArchivioDbContext : DbContext
    {
        public DbSet<FilmRecord> Film => Set<FilmRecord>();
        public DbSet<PersonaggioRecord> Personaggi => Set<PersonaggioRecord>();
        public DbSet<PianetaRecord> Pianeti => Set<PianetaRecord>();
        public DbSet<RazzaRecord> Razze => Set<RazzaRecord>();
        public DbSet<AstronaveRecord> Astronavi => Set<AstronaveRecord>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // Ogni volta che cambia lo schema (tabelle/colonne) va cambiato il nome del file:
            // EnsureCreated non modifica un file già esistente. archivio4.db = con tabella Film.
            var percorso = Path.Combine(ApplicationData.Current.LocalFolder.Path, "archivio4.db");
            options.UseSqlite($"Data Source={percorso}");
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // Chiave = utente + Id SWAPI (l'Id non va generato dal DB).
            mb.Entity<FilmRecord>().HasKey(x => new { x.UtenteKey, x.Id });
            mb.Entity<PersonaggioRecord>().HasKey(x => new { x.UtenteKey, x.Id });
            mb.Entity<PianetaRecord>().HasKey(x => new { x.UtenteKey, x.Id });
            mb.Entity<RazzaRecord>().HasKey(x => new { x.UtenteKey, x.Id });
            mb.Entity<AstronaveRecord>().HasKey(x => new { x.UtenteKey, x.Id });

            mb.Entity<FilmRecord>().Property(x => x.Id).ValueGeneratedNever();
            mb.Entity<PersonaggioRecord>().Property(x => x.Id).ValueGeneratedNever();
            mb.Entity<PianetaRecord>().Property(x => x.Id).ValueGeneratedNever();
            mb.Entity<RazzaRecord>().Property(x => x.Id).ValueGeneratedNever();
            mb.Entity<AstronaveRecord>().Property(x => x.Id).ValueGeneratedNever();
        }
    }

    internal class FilmRecord : IRecord
    {
        public string UtenteKey { get; set; } = "";
        public DateTime DataSalvataggio { get; set; } = DateTime.UtcNow;
        public int Id { get; set; }
        public string Url { get; set; } = "";
        public string Title { get; set; } = "";
        public int EpisodeId { get; set; }
        public string OpeningCrawl { get; set; } = "";
        public string Director { get; set; } = "";
        public string ReleaseDate { get; set; } = "";
        public string PersonaggioIds { get; set; } = "";
        public string PianetaIds { get; set; } = "";
        public string RazzaIds { get; set; } = "";
        public string AstronaveIds { get; set; } = "";
        public string? PosterUrl { get; set; }
    }

    internal class PersonaggioRecord : IRecord
    {
        public string UtenteKey { get; set; } = "";
        public DateTime DataSalvataggio { get; set; } = DateTime.UtcNow;
        public int Id { get; set; }
        public string Url { get; set; } = "";
        public string Name { get; set; } = "";
        public string Height { get; set; } = "";
        public string Mass { get; set; } = "";
        public string BirthYear { get; set; } = "";
        public string EyeColor { get; set; } = "";
        public string HairColor { get; set; } = "";
        public string Gender { get; set; } = "";
        public int HomeworldId { get; set; }
        public string SpeciesIds { get; set; } = "";
        public string FilmIds { get; set; } = "";
        public string StarshipIds { get; set; } = "";
        public string? MainImage { get; set; }
        public string ImageGallery { get; set; } = "";
    }

    internal class PianetaRecord : IRecord
    {
        public string UtenteKey { get; set; } = "";
        public DateTime DataSalvataggio { get; set; } = DateTime.UtcNow;
        public int Id { get; set; }
        public string Url { get; set; } = "";
        public string Name { get; set; } = "";
        public string Diameter { get; set; } = "";
        public string RotationPeriod { get; set; } = "";
        public string Gravity { get; set; } = "";
        public string Population { get; set; } = "";
        public string Climate { get; set; } = "";
        public string SurfaceWater { get; set; } = "";
        public string Terrain { get; set; } = "";
        public string FilmIds { get; set; } = "";
        public string? MainImage { get; set; }
        public string ImageGallery { get; set; } = "";
    }

    internal class RazzaRecord : IRecord
    {
        public string UtenteKey { get; set; } = "";
        public DateTime DataSalvataggio { get; set; } = DateTime.UtcNow;
        public int Id { get; set; }
        public string Url { get; set; } = "";
        public string Name { get; set; } = "";
        public string Classification { get; set; } = "";
        public string AverageHeight { get; set; } = "";
        public string AverageLifespan { get; set; } = "";
        public string Language { get; set; } = "";
        public int HomeworldId { get; set; }
        public string PersonaggioIds { get; set; } = "";
        public string FilmIds { get; set; } = "";
        public string? MainImage { get; set; }
        public string ImageGallery { get; set; } = "";
    }

    internal class AstronaveRecord : IRecord
    {
        public string UtenteKey { get; set; } = "";
        public DateTime DataSalvataggio { get; set; } = DateTime.UtcNow;
        public int Id { get; set; }
        public string Url { get; set; } = "";
        public string Name { get; set; } = "";
        public string Model { get; set; } = "";
        public string StarshipClass { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Crew { get; set; } = "";
        public string Passengers { get; set; } = "";
        public string Mglt { get; set; } = "";
        public string HyperdriveRating { get; set; } = "";
        public string FilmIds { get; set; } = "";
        public string PilotIds { get; set; } = "";
        public string? MainImage { get; set; }
        public string ImageGallery { get; set; } = "";
    }
}
