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
    // ArchivioLocale - cache su DB (EF Core + SQLite) dei dati SWAPI.
    // ------------------------------------------------------------
    // Usato SOLO da GestioneAPI:
    //   Carica*  -> legge dal DB (lista vuota se non c'è nulla)
    //   Salva*   -> UPSERT (inserisce i nuovi, aggiorna quelli con lo stesso Id)
    //
    // Usa un file separato (archivio.db) con un proprio contesto, quindi non
    // interferisce con AppDbContext (utenti e log). Un contesto NUOVO per ogni
    // chiamata: RicercaGlobale scarica 5 tipi in parallelo.
    // Le liste (ID e immagini) sono salvate come testo: "1,2,3" e "url1|url2".
    // ============================================================
    public static class ArchivioLocale
    {
        // ---------------- Film ----------------
        public static List<Film> CaricaFilm()
        {
            using var db = NuovoContesto();
            return db.Film.AsNoTracking().ToList().Select(r => new Film
            {
                Id = r.Id, Url = r.Url, Title = r.Title, EpisodeId = r.EpisodeId,
                OpeningCrawl = r.OpeningCrawl, Director = r.Director, ReleaseDate = r.ReleaseDate,
                PersonaggioIds = Ids(r.PersonaggioIds), PianetaIds = Ids(r.PianetaIds),
                RazzaIds = Ids(r.RazzaIds), AstronaveIds = Ids(r.AstronaveIds),
                PosterUrl = r.PosterUrl
            }).ToList();
        }

        public static void SalvaFilm(List<Film> lista)
        {
            using var db = NuovoContesto();
            Upsert(db, lista.Select(f => new FilmRecord
            {
                Id = f.Id, Url = f.Url ?? "", Title = f.Title ?? "", EpisodeId = f.EpisodeId,
                OpeningCrawl = f.OpeningCrawl ?? "", Director = f.Director ?? "", ReleaseDate = f.ReleaseDate ?? "",
                PersonaggioIds = Testo(f.PersonaggioIds), PianetaIds = Testo(f.PianetaIds),
                RazzaIds = Testo(f.RazzaIds), AstronaveIds = Testo(f.AstronaveIds),
                PosterUrl = f.PosterUrl
            }));
        }

        // ---------------- Personaggi ----------------
        public static List<Personaggio> CaricaPersonaggi()
        {
            using var db = NuovoContesto();
            return db.Personaggi.AsNoTracking().ToList().Select(r => new Personaggio
            {
                Id = r.Id, Url = r.Url, Name = r.Name, Height = r.Height, Mass = r.Mass,
                BirthYear = r.BirthYear, EyeColor = r.EyeColor, HairColor = r.HairColor, Gender = r.Gender,
                HomeworldId = r.HomeworldId, SpeciesIds = Ids(r.SpeciesIds), FilmIds = Ids(r.FilmIds),
                StarshipIds = Ids(r.StarshipIds),
                MainImage = r.MainImage, ImageGallery = Gallery(r.ImageGallery)
            }).ToList();
        }

        public static void SalvaPersonaggi(List<Personaggio> lista)
        {
            using var db = NuovoContesto();
            Upsert(db, lista.Select(p => new PersonaggioRecord
            {
                Id = p.Id, Url = p.Url ?? "", Name = p.Name ?? "", Height = p.Height ?? "", Mass = p.Mass ?? "",
                BirthYear = p.BirthYear ?? "", EyeColor = p.EyeColor ?? "", HairColor = p.HairColor ?? "",
                Gender = p.Gender ?? "", HomeworldId = p.HomeworldId,
                SpeciesIds = Testo(p.SpeciesIds), FilmIds = Testo(p.FilmIds), StarshipIds = Testo(p.StarshipIds),
                MainImage = p.MainImage, ImageGallery = Testo(p.ImageGallery)
            }));
        }

        // ---------------- Pianeti ----------------
        public static List<Pianeta> CaricaPianeti()
        {
            using var db = NuovoContesto();
            return db.Pianeti.AsNoTracking().ToList().Select(r => new Pianeta
            {
                Id = r.Id, Url = r.Url, Name = r.Name, Diameter = r.Diameter, RotationPeriod = r.RotationPeriod,
                Gravity = r.Gravity, Population = r.Population, Climate = r.Climate,
                SurfaceWater = r.SurfaceWater, Terrain = r.Terrain, FilmIds = Ids(r.FilmIds),
                MainImage = r.MainImage, ImageGallery = Gallery(r.ImageGallery)
            }).ToList();
        }

        public static void SalvaPianeti(List<Pianeta> lista)
        {
            using var db = NuovoContesto();
            Upsert(db, lista.Select(p => new PianetaRecord
            {
                Id = p.Id, Url = p.Url ?? "", Name = p.Name ?? "", Diameter = p.Diameter ?? "",
                RotationPeriod = p.RotationPeriod ?? "", Gravity = p.Gravity ?? "", Population = p.Population ?? "",
                Climate = p.Climate ?? "", SurfaceWater = p.SurfaceWater ?? "", Terrain = p.Terrain ?? "",
                FilmIds = Testo(p.FilmIds),
                MainImage = p.MainImage, ImageGallery = Testo(p.ImageGallery)
            }));
        }

        // ---------------- Razze ----------------
        public static List<Razza> CaricaRazze()
        {
            using var db = NuovoContesto();
            return db.Razze.AsNoTracking().ToList().Select(r => new Razza
            {
                Id = r.Id, Url = r.Url, Name = r.Name, Classification = r.Classification,
                AverageHeight = r.AverageHeight, AverageLifespan = r.AverageLifespan, Language = r.Language,
                HomeworldId = r.HomeworldId, PersonaggioIds = Ids(r.PersonaggioIds), FilmIds = Ids(r.FilmIds),
                MainImage = r.MainImage, ImageGallery = Gallery(r.ImageGallery)
            }).ToList();
        }

        public static void SalvaRazze(List<Razza> lista)
        {
            using var db = NuovoContesto();
            Upsert(db, lista.Select(r => new RazzaRecord
            {
                Id = r.Id, Url = r.Url ?? "", Name = r.Name ?? "", Classification = r.Classification ?? "",
                AverageHeight = r.AverageHeight ?? "", AverageLifespan = r.AverageLifespan ?? "",
                Language = r.Language ?? "", HomeworldId = r.HomeworldId,
                PersonaggioIds = Testo(r.PersonaggioIds), FilmIds = Testo(r.FilmIds),
                MainImage = r.MainImage, ImageGallery = Testo(r.ImageGallery)
            }));
        }

        // ---------------- Astronavi ----------------
        public static List<Astronave> CaricaAstronavi()
        {
            using var db = NuovoContesto();
            return db.Astronavi.AsNoTracking().ToList().Select(r => new Astronave
            {
                Id = r.Id, Url = r.Url, Name = r.Name, Model = r.Model, StarshipClass = r.StarshipClass,
                Manufacturer = r.Manufacturer, Crew = r.Crew, Passengers = r.Passengers, Mglt = r.Mglt,
                HyperdriveRating = r.HyperdriveRating, FilmIds = Ids(r.FilmIds), PilotIds = Ids(r.PilotIds),
                MainImage = r.MainImage, ImageGallery = Gallery(r.ImageGallery)
            }).ToList();
        }

        public static void SalvaAstronavi(List<Astronave> lista)
        {
            using var db = NuovoContesto();
            Upsert(db, lista.Select(a => new AstronaveRecord
            {
                Id = a.Id, Url = a.Url ?? "", Name = a.Name ?? "", Model = a.Model ?? "",
                StarshipClass = a.StarshipClass ?? "", Manufacturer = a.Manufacturer ?? "", Crew = a.Crew ?? "",
                Passengers = a.Passengers ?? "", Mglt = a.Mglt ?? "", HyperdriveRating = a.HyperdriveRating ?? "",
                FilmIds = Testo(a.FilmIds), PilotIds = Testo(a.PilotIds),
                MainImage = a.MainImage, ImageGallery = Testo(a.ImageGallery)
            }));
        }

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
                        PulisciScaduti(db);            // dati più vecchi di 3 mesi -> eliminati
                        _creato = true;
                    }
                }
            }
            return db;
        }

        // Scadenza: se una tabella contiene dati salvati da più di 3 mesi, viene svuotata per intero
        // (mai a metà, altrimenti la cache risulterebbe incompleta). Al prossimo uso online
        // i dati si riscaricano dall'API e si risalvano. Controllo fatto una volta per avvio app.
        private static void PulisciScaduti(ArchivioDbContext db)
        {
            try
            {
                var limite = DateTime.UtcNow.AddMonths(-3);
                if (db.Film.Any(x => x.DataSalvataggio < limite)) db.Film.RemoveRange(db.Film.ToList());
                if (db.Personaggi.Any(x => x.DataSalvataggio < limite)) db.Personaggi.RemoveRange(db.Personaggi.ToList());
                if (db.Pianeti.Any(x => x.DataSalvataggio < limite)) db.Pianeti.RemoveRange(db.Pianeti.ToList());
                if (db.Razze.Any(x => x.DataSalvataggio < limite)) db.Razze.RemoveRange(db.Razze.ToList());
                if (db.Astronavi.Any(x => x.DataSalvataggio < limite)) db.Astronavi.RemoveRange(db.Astronavi.ToList());
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] pulizia dati scaduti fallita: {ex.Message}");
            }
        }

        private static void Upsert<TR>(ArchivioDbContext db, IEnumerable<TR> records) where TR : class, IRecord
        {
            var set = db.Set<TR>();
            foreach (var r in records)
            {
                var esistente = set.Find(r.Id);
                if (esistente == null) set.Add(r);
                else
                {
                    r.DataSalvataggio = esistente.DataSalvataggio;   // la scadenza parte dal primo salvataggio
                    db.Entry(esistente).CurrentValues.SetValues(r);
                }
            }
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
        int Id { get; set; }
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
            var percorso = Path.Combine(ApplicationData.Current.LocalFolder.Path, "archivio2.db");
            options.UseSqlite($"Data Source={percorso}");
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // L'Id è quello di SWAPI: non va generato dal DB.
            mb.Entity<FilmRecord>().Property(x => x.Id).ValueGeneratedNever();
            mb.Entity<PersonaggioRecord>().Property(x => x.Id).ValueGeneratedNever();
            mb.Entity<PianetaRecord>().Property(x => x.Id).ValueGeneratedNever();
            mb.Entity<RazzaRecord>().Property(x => x.Id).ValueGeneratedNever();
            mb.Entity<AstronaveRecord>().Property(x => x.Id).ValueGeneratedNever();
        }
    }

    internal class FilmRecord : IRecord
    {
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
