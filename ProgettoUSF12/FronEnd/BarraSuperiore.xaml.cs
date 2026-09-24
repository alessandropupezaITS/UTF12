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

       
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // In UWP non si creano finestre separate per le pagine: si naviga nel Frame principale.
            var rootFrame = Window.Current.Content as Frame;
            rootFrame?.Navigate(typeof(Login));
        }
    }
}
