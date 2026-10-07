namespace ProgettoUSF12.BackEnd.Services
{
    // Esempio del pattern che vuoi usare in tutto il progetto:
    // il bottone nel .xaml.cs chiama solo questo metodo, la logica vera sta qui.
    public class GestioneUtente
    {
        // Chiave con cui ArchivioLocale separa i salvataggi di ogni utente.
        // null = nessun utente loggato (non si può salvare).
        // Se in futuro UtenteDbModel avrà un Id, basta restituire quello (come testo) al posto dello username.
        public static string? ChiaveCorrente =>
            string.IsNullOrWhiteSpace(App.UsernameLoggato)
                ? null
                : App.UsernameLoggato.Trim().ToLowerInvariant();

        public void ApriProfilo()
        {
            // TODO: qui andrà la logica di apertura della pagina/profilo utente.
        }

        public void Login(string username, bool salvaOffline)
        {
            // TODO: qui andrà la logica vera di accesso
            // (validare lo username, eventualmente salvare la sessione
            // se salvaOffline è true, ecc.)
        }

        public bool VerificaUtente()
        {
            // TODO: qui andrà la logica di verifica dell'utente loggato.
            return false;
        }
    }
}
