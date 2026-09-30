using System;
using System.Collections.Generic;

namespace ProgettoUSF12.BackEnd.Classi
{
    public class Utente
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DataRegistrazione { get; set; } = DateTime.Now;
        public DateTime? UltimoAccesso { get; set; }

        // Storico delle operazioni effettuate
        public List<LogOperazione> StoricoOperazioni { get; set; } = new();
    }

    public class LogOperazione
    {
        public int Id { get; set; }
        public string TipoOperazione { get; set; } = string.Empty; // es. "Ricerca", "VisualizzazioneFilm", "Login"
        public string Dettaglio { get; set; } = string.Empty;      // es. "Cercato personaggio: Luke Skywalker"
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}