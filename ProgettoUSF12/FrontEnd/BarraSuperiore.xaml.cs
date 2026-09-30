using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ProgettoUSF12;
using ProgettoUSF12.BackEnd.Services;

namespace ProgettoUSF12.FrontEnd
{
    public sealed partial class BarraSuperiore : UserControl
    {
        public BarraSuperiore()
        {
            this.InitializeComponent();
            AggiornaStatoUtente();
        }

        // ===================== GESTIONE UTENTE & LOGOUT =====================

        private void AggiornaStatoUtente()
        {
            if (!string.IsNullOrWhiteSpace(App.UsernameLoggato))
            {
                TxtNomeUtente.Text = App.UsernameLoggato;
                TxtNomeUtente.Visibility = Visibility.Visible;
                BtnLogout.Visibility = Visibility.Visible;
            }
            else
            {
                TxtNomeUtente.Text = string.Empty;
                TxtNomeUtente.Visibility = Visibility.Collapsed;
                BtnLogout.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            if (string.IsNullOrEmpty(App.UsernameLoggato))
            {
                rootFrame?.Navigate(typeof(Login));
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            App.UsernameLoggato = null;
            AggiornaStatoUtente();

            var rootFrame = Window.Current.Content as Frame;
            rootFrame?.Navigate(typeof(Login));
        }

        // ===================== NAVIGAZIONE PAGINE =====================

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
            {
                rootFrame.Navigate(typeof(PaginaFilm));
            }
        }

        private void BtnArchivio_Click(object sender, RoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.Content is not PaginaArchivio)
            {
                rootFrame.Navigate(typeof(PaginaArchivio));
            }
        }

        // ===================== RICERCA GLOBALE =====================

        private TipoRicerca FiltroCorrente =>
            (TipoRicerca)Math.Max(0, FiltroRicerca.SelectedIndex);

        private async void CampoRicerca_GotFocus(object sender, RoutedEventArgs e)
        {
            await CaricaIndiceAsync();
        }

        private async Task CaricaIndiceAsync()
        {
            try
            {
                await RicercaGlobale.CaricaAsync();
                AggiornaSuggerimenti();
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

        private void CampoRicerca_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var scelto = args.ChosenSuggestion as RisultatoRicerca;
            if (scelto == null && RicercaGlobale.Pronta)
            {
                scelto = RicercaGlobale.Cerca(args.QueryText, FiltroCorrente).FirstOrDefault();
            }

            if (scelto == null || scelto.Categoria == TipoRicerca.Tutto) return;
            ApriRisultato(scelto);
        }

        private void ApriRisultato(RisultatoRicerca r)
        {
            var frame = Window.Current.Content as Frame;
            if (frame == null) return;

            if (r.Categoria == TipoRicerca.Film)
            {
                frame.Navigate(typeof(PaginaFilm), r.Nome);
                return;
            }

            frame.Navigate(typeof(PaginaDettaglio),
                new ParametroDettaglio { Categoria = r.Categoria, FilmId = r.FilmId, EntitaId = r.Id });
        }
    }
}