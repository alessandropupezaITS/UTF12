using ProgettoUSF12.BackEnd.Classi;
using ProgettoUSF12.BackEnd.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ProgettoUSF12
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            CaricaPagina();
        }

        private void CaricaPagina()
        {
            try
            {
                // Lista film ordinata per episodio, mostrata come griglia
                // di locandine cliccabili. Niente più ItemsSource di sole
                // stringhe (era quello a causare l'ArgumentException nel
                // marshalling WinRT/ComWrappers).
                var film = new ObservableCollection<Film>(
            GestioneAPI.GetFilms().OrderBy(f => f.EpisodeId));

                ListaFilm.ItemsSource = film;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore caricamento Home: {ex.Message}");
            }
        }

        // Il click su una locandina porta alla pagina Film già filtrata
        // sulla trilogia/categoria del film cliccato.
        private void ListaFilm_ItemClick(object sender, ItemClickEventArgs e)
        {
            var film = (Film)e.ClickedItem;
            var categoria = NomeCategoria(film.EpisodeId);
            Frame.Navigate(typeof(PaginaFilm), categoria);
        }

        private string NomeCategoria(int episodeId) => episodeId switch
        {
            >= 1 and <= 3 => "Prima Trilogia",
            >= 4 and <= 6 => "Seconda Trilogia",
            >= 7 and <= 9 => "Terza Trilogia",
            _ => "Altri"
        };
    }
}
