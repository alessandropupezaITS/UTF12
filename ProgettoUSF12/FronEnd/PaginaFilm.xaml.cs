using ProgettoUSF12.BackEnd.Classi;
using ProgettoUSF12.BackEnd.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ProgettoUSF12
{
    public sealed partial class PaginaFilm : Page
    {
        // Categoria attiva su questa pagina. "Tutti" = nessun filtro.
        private string _categoriaAttiva = "Tutti";

        public PaginaFilm()
        {
            InitializeComponent();
        }

        // Riceve la categoria passata da MainPage (es. Frame.Navigate(typeof(PaginaFilm), "Prima Trilogia")).
        // Se non arriva nulla (o arriva "Tutti"), la pagina mostra tutti i film senza filtro.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _categoriaAttiva = e.Parameter as string ?? "Tutti";
            CaricaPagina();
        }

        private void CaricaPagina()
        {
            try
            {
                ListaCategorie.ItemsSource = new List<string>
                {
                    "Tutti", "Prima Trilogia", "Seconda Trilogia", "Terza Trilogia"
                };

                ListaFilm.ItemsSource = FiltraPerCategoria(_categoriaAttiva);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore caricamento PaginaFilm: {ex.Message}");
            }
        }

        // Scarica tutti i film da SWAPI, li raggruppa per trilogia e,
        // se serve, tiene solo il gruppo della categoria scelta.
        private List<GruppoFilm> FiltraPerCategoria(string categoria)
        {
            var gruppi = GestioneAPI.GetFilms()
                .GroupBy(f => NomeCategoria(f.EpisodeId))
                .Select(g => new GruppoFilm
                {
                    Titolo = g.Key,
                    Film = g.OrderBy(f => f.EpisodeId).ToList()
                })
                .OrderBy(g => g.Film.Min(f => f.EpisodeId))
                .ToList();

            return categoria == "Tutti"
                ? gruppi
                : gruppi.Where(g => g.Titolo == categoria).ToList();
        }

        // Cambiare chip qui dentro la pagina filtra sul posto (non naviga di nuovo)
        private void ListaCategorie_ItemClick(object sender, ItemClickEventArgs e)
        {
            _categoriaAttiva = (string)e.ClickedItem;
            ListaFilm.ItemsSource = FiltraPerCategoria(_categoriaAttiva);
        }

        // TODO: qui andrà la navigazione verso la pagina di dettaglio
        // del singolo film, quando la creerai.
        private void ListaFilm_ItemClick(object sender, ItemClickEventArgs e)
        {
            var film = (Film)e.ClickedItem;
            System.Diagnostics.Debug.WriteLine($"Apri dettaglio film id={film.Id} (pagina dettaglio non ancora creata)");
        }

        // Stessa logica di categorizzazione già usata in MainPage.xaml.cs
        private string NomeCategoria(int episodeId) => episodeId switch
        {
            >= 1 and <= 3 => "Prima Trilogia",
            >= 4 and <= 6 => "Seconda Trilogia",
            >= 7 and <= 9 => "Terza Trilogia",
            _ => "Altri"
        };
    }
}
