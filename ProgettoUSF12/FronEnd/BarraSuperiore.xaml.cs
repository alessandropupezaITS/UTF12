using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ProgettoUSF12.FrontEnd;
namespace ProgettoUSF12.FrontEnd
{
    public sealed partial class BarraSuperiore : UserControl
    {
        public BarraSuperiore()
        {
            this.InitializeComponent();
        }

        // NUOVO: torna alla Home. Se siamo già sulla MainPage non fa nulla,
        // così non si accumulano pagine identiche nello stack di navigazione.
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.Content is not MainPage)
            {
                rootFrame.Navigate(typeof(MainPage));
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // In UWP non si creano finestre separate per le pagine: si naviga nel Frame principale.
            var rootFrame = Window.Current.Content as Frame;
            rootFrame?.Navigate(typeof(Login));
        }
    }
}
