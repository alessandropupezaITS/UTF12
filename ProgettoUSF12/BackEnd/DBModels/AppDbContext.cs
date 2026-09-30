using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text.Json;
using Windows.UI;

namespace ProgettoUSF12.BackEnd.DbModels
{
    public class AppDbContext : DbContext
    {
        public DbSet<UtenteDbModel> Utenti { get; set; } = null!;
        public DbSet<LogOperazioneDbModel> LogOperazioni { get; set; } = null!;
        public DbSet<FilmDbModel> Film { get; set; } = null!;
        public DbSet<PersonaggioDbModel> Personaggi { get; set; } = null!;
        public DbSet<PianetaDbModel> Pianeti { get; set; } = null!;
        public DbSet<RazzaDbModel> Razze { get; set; } = null!;
        public DbSet<AstronaveDbModel> Astronavi { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Configurazione di fallback per SQLite locale
                optionsBuilder.UseSqlite("Data Source=starwars_app.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Conversione automatica delle liste in stringhe JSON per SQLite/SQL Server
            modelBuilder.Entity<FilmDbModel>(entity =>
            {
                entity.Property(e => e.PersonaggioIds)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new());
                entity.Property(e => e.PianetaIds)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new());
                entity.Property(e => e.RazzaIds)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new());
                entity.Property(e => e.AstronaveIds)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new());
            });

            modelBuilder.Entity<PersonaggioDbModel>(entity =>
            {
                entity.Property(e => e.SpeciesIds)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new());
                entity.Property(e => e.FilmIds)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new());
                entity.Property(e => e.StarshipIds)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new());
                entity.Property(e => e.ImageGallery)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
            });

            modelBuilder.Entity<PianetaDbModel>(entity =>
            {
                entity.Property(e => e.FilmIds)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new());
                entity.Property(e => e.ImageGallery)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
            });

            modelBuilder.Entity<RazzaDbModel>(entity =>
            {
                entity.Property(e => e.PersonaggioIds)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new());
                entity.Property(e => e.FilmIds)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new());
                entity.Property(e => e.ImageGallery)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
            });

            modelBuilder.Entity<AstronaveDbModel>(entity =>
            {
                entity.Property(e => e.FilmIds)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new());
                entity.Property(e => e.PilotIds)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new());
                entity.Property(e => e.ImageGallery)
                      .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new());
            });
        }
    }
}
