using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgettoUSF12.DBModels
{
    internal class SwapiContext : DbContext
    {
        public DbSet<Personaggi> Personaggi { get; set; }

        public DbSet<Astronavi> Astronavi { get; set; }

        public DbSet<Film> Film { get; set; }

        public DbSet<Utenti> Utenti { get; set; }

        public DbSet<Pianeti> Pianeti { get; set; }

        public DbSet<Razza> Razza { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=swapi.db");
        }
    }
}
