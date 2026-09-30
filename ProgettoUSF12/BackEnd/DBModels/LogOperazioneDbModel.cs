using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgettoUSF12.BackEnd.DbModels
{
    [Table("LogOperazioni")]
    public class LogOperazioneDbModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UtenteId { get; set; }

        [ForeignKey(nameof(UtenteId))]
        public virtual UtenteDbModel Utente { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string TipoOperazione { get; set; } = string.Empty; // es. "Ricerca", "VisitaPagina", "Login", "Logout"

        [Required]
        public string Dettaglio { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}