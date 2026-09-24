using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ProgettoUSF12.BackEnd.Services;

using ProgettoUSF12.BackEnd.Classi;

namespace ProgettoUSF12.FrontEnd
{
    public sealed partial class Login : Page
    {
        private GestioneUtente _gestioneUtente;

        public Login()
        {
            InitializeComponent();
            
        }

        private void BtnConferma_Click(object sender, RoutedEventArgs e)
        {
            TxtErrore.Visibility = Visibility.Collapsed;

            string username = TxtUsername.Text?.Trim() ?? string.Empty;
            string password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MostraErrore("Inserisci username e password.");
                return;
            }

            bool salvaOffline = ChkSalvaOffline.IsChecked ?? false;

            try
            {
                _gestioneUtente.Login(username, salvaOffline);
                Frame.Navigate(typeof(MainPage));
            }
            catch (Exception ex)
            {
                MostraErrore($"Errore durante l'accesso: {ex.Message}");
            }
        }

        private void MostraErrore(string messaggio)
        {
            TxtErrore.Text = messaggio;
            TxtErrore.Visibility = Visibility.Visible;
        }

        private void LinkToggleModalita_Click(object sender, RoutedEventArgs e)
        {
            // Toggle semplice tra modalità Accedi e Registrati
            bool isRegister = LblConfermaPassword.Visibility == Visibility.Collapsed;

            if (isRegister)
            {
                LblConfermaPassword.Visibility = Visibility.Visible;
                TxtConfermaPassword.Visibility = Visibility.Visible;
                BtnConferma.Content = "Registrati";
                TxtTitolo.Text = "Registrati";
                TxtLinkToggle.Text = "Hai già un account? Accedi";
            }
            else
            {
                LblConfermaPassword.Visibility = Visibility.Collapsed;
                TxtConfermaPassword.Visibility = Visibility.Collapsed;
                BtnConferma.Content = "Accedi";
                TxtTitolo.Text = "Accedi";
                TxtLinkToggle.Text = "Non hai un account? Registrati";
            }
        }
    }
}
