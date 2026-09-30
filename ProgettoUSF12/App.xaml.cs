using System;
using Microsoft.Extensions.DependencyInjection;
using ProgettoUSF12.BackEnd.Services;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ProgettoUSF12
{
    public sealed partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        // Variabile globale per memorizzare il nome dell'utente loggato
        public static string UsernameLoggato { get; set; }

        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;

            var servizi = new ServiceCollection();
            servizi.AddSingleton<GestioneUtente>();
            ServiceProvider = servizi.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (Window.Current.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state
                }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                Window.Current.Activate();
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load page '{e.SourcePageType.FullName}'.");
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}