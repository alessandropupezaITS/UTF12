using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgettoUSF12.BackEnd.DbModels
{
    [Table("Utenti")]
    public class UtenteDbModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime DataRegistrazione { get; set; } = DateTime.Now;

        public DateTime? UltimoAccesso { get; set; }

        // Relazione 1-a-Molti con il tracciamento delle operazioni
        public virtual ICollection<LogOperazioneDbModel> LogOperazioni { get; set; } = new List<LogOperazioneDbModel>();
    }
}