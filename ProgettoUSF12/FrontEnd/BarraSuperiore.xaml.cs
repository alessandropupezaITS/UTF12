using ProgettoUSF12.BackEnd.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ProgettoUSF12.FrontEnd
{
    public sealed partial class BarraSuperiore : UserControl
    {
        public BarraSuperiore()
        {
            this.InitializeComponent();
        }

        // Torna alla Home. Se siamo già sulla MainPage non fa nulla,
        // così non si accumulano pagine identiche nello stack di navigazione.
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.Content is not MainPage)
            {
                rootFrame.Navigate(typeof(MainPage));
            }
        }

        private void BtnFilm_Click(object sender, RoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.Content is not PaginaFilm)
                rootFrame.Navigate(typeof(PaginaFilm));
        }

        private void BtnArchivio_Click(object sender, RoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.Content is not PaginaArchivio)
                rootFrame.Navigate(typeof(PaginaArchivio));
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // In UWP non si creano finestre separate per le pagine: si naviga nel Frame principale.
            var rootFrame = Window.Current.Content as Frame;
            rootFrame?.Navigate(typeof(Login));
        }

        // ===================== RICERCA =====================

        // L'ordine delle voci del ComboBox coincide con l'enum TipoRicerca.
        private TipoRicerca FiltroCorrente =>
            (TipoRicerca)Math.Max(0, FiltroRicerca.SelectedIndex);

        // Il primo click sulla barra avvia (una sola volta per tutta l'app) il download
        // in background dei dati su cui cercare.
        private async void CampoRicerca_GotFocus(object sender, RoutedEventArgs e)
        {
            await CaricaIndiceAsync();
        }

        private async Task CaricaIndiceAsync()
        {
            try
            {
                await RicercaGlobale.CaricaAsync();
                AggiornaSuggerimenti();   // se nel frattempo l'utente ha già scritto
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore caricamento indice ricerca: {ex.Message}");
            }
        }

        private void CampoRicerca_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;

            if (!RicercaGlobale.Pronta)
            {
                sender.ItemsSource = new List<RisultatoRicerca> { RisultatoRicerca.Messaggio("Caricamento dati in corso...") };
                _ = CaricaIndiceAsync();
                return;
            }

            AggiornaSuggerimenti();
        }

        // Cambiare filtro rifà la ricerca con lo stesso testo.
        private void FiltroRicerca_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CampoRicerca == null || !RicercaGlobale.Pronta) return;
            AggiornaSuggerimenti();
        }

        private void AggiornaSuggerimenti()
        {
            var testo = CampoRicerca.Text?.Trim() ?? "";
            if (testo.Length == 0)
            {
                CampoRicerca.ItemsSource = null;
                return;
            }
            if (!RicercaGlobale.Pronta) return;

            var risultati = RicercaGlobale.Cerca(testo, FiltroCorrente);
            CampoRicerca.ItemsSource = risultati.Count > 0
                ? risultati
                : new List<RisultatoRicerca> { RisultatoRicerca.Messaggio("Nessun risultato") };
            CampoRicerca.IsSuggestionListOpen = true;
        }

        // Click su un suggerimento, oppure Invio: senza scelta apre il primo risultato.
        private void CampoRicerca_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var scelto = args.ChosenSuggestion as RisultatoRicerca;
            if (scelto == null && RicercaGlobale.Pronta)
                scelto = RicercaGlobale.Cerca(args.QueryText, FiltroCorrente).FirstOrDefault();

            if (scelto == null || scelto.Categoria == TipoRicerca.Tutto) return;   // riga di servizio
            ApriRisultato(scelto);
        }

        private void ApriRisultato(RisultatoRicerca r)
        {
            var frame = Window.Current.Content as Frame;
            if (frame == null) return;

            if (r.Categoria == TipoRicerca.Film)
            {
                frame.Navigate(typeof(PaginaFilm), r.Nome);   // il parametro è il titolo
                return;
            }

            frame.Navigate(typeof(PaginaDettaglio),
                new ParametroDettaglio { Categoria = r.Categoria, FilmId = r.FilmId, EntitaId = r.Id });
        }
    }
}
