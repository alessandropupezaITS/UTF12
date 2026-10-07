using ProgettoUSF12.BackEnd.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ProgettoUSF12
{
    // Una scheda dell'Archivio: solo nome + immagine. L'immagine arriva dopo,
    // quando la scheda entra nella vista, e la scheda si aggiorna da sola.
    public class VoceArchivio : INotifyPropertyChanged
    {
        public TipoRicerca Categoria { get; set; }
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Tipo { get; set; } = "";
        public bool ImmagineRichiesta { get; set; }

        private BitmapImage? _miniatura;
        public BitmapImage? Miniatura
        {
            get => _miniatura;
            set
            {
                _miniatura = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Miniatura)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    // Pagina con TUTTO (film, personaggi, pianeti, razze, astronavi) in forma leggera.
    // Il filtro a chip riduce il numero di schede, come il filtro della barra di ricerca.
    public sealed partial class PaginaArchivio : Page
    {
        // Stesso ordine dell'enum TipoRicerca (e del ComboBox della barra di ricerca).
        private static readonly string[] Filtri = { "Tutto", "Film", "Personaggi", "Pianeti", "Razze", "Astronavi" };

        private TipoRicerca _filtro = TipoRicerca.Tutto;
        private int _versione;

        // URL delle immagini già trovate ("categoria:id"): rivisitando la pagina sono istantanee.
        private static readonly ConcurrentDictionary<string, string?> _cacheImmagini = new();

        // Massimo 6 ricerche di immagini contemporanee.
        private static readonly SemaphoreSlim _limite = new(6);

        public PaginaArchivio()
        {
            InitializeComponent();
            ListaFiltri.ItemsSource = Filtri;
            Griglia.Loaded += (s, e) => AdattaLarghezzaSchede();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await AggiornaAsync();
        }

        private async Task AggiornaAsync()
        {
            int versione = ++_versione;
            Caricamento.IsActive = true;
            Vuoto.Visibility = Visibility.Collapsed;
            Griglia.ItemsSource = null;
            Conteggio.Text = "";

            try
            {
                // Stesso indice della barra di ricerca: scaricato una volta sola per sessione.
                await RicercaGlobale.CaricaAsync();
                if (versione != _versione) return;

                var voci = RicercaGlobale.Elenco(_filtro).Select(r => Crea(r)).ToList();

                Griglia.ItemsSource = voci;
                AdattaLarghezzaSchede();
                Conteggio.Text = $"{Filtri[(int)_filtro]}  •  {voci.Count} elementi";
                Vuoto.Visibility = voci.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore caricamento Archivio: {ex.Message}");
                Vuoto.Visibility = Visibility.Visible;
            }
            finally
            {
                if (versione == _versione) Caricamento.IsActive = false;
            }
        }

        private static VoceArchivio Crea(RisultatoRicerca r)
        {
            var voce = new VoceArchivio { Categoria = r.Categoria, Id = r.Id, Nome = r.Nome, Tipo = r.Tipo };

            // I film hanno già la locandina: nessuna chiamata in più.
            if (r.Categoria == TipoRicerca.Film)
            {
                voce.ImmagineRichiesta = true;
                voce.Miniatura = PaginaFilm.ConvertiMiniatura(RicercaGlobale.TrovaFilm(r.Id)?.PosterUrl);
            }
            return voce;
        }

        // Scatta quando una scheda entra nella vista: solo allora cerchiamo la sua immagine.
        private void Griglia_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue) return;

            if (args.Item is VoceArchivio v && !v.ImmagineRichiesta)
            {
                v.ImmagineRichiesta = true;
                _ = CaricaImmagineAsync(v, _versione);
            }
        }

        private async Task CaricaImmagineAsync(VoceArchivio v, int versione)
        {
            try
            {
                var chiave = $"{(int)v.Categoria}:{v.Id}";

                if (!_cacheImmagini.TryGetValue(chiave, out var url))
                {
                    await _limite.WaitAsync();
                    try
                    {
                        // Se nel frattempo è cambiato il filtro, la scheda non serve più.
                        if (versione != _versione) return;

                        url = await Task.Run(() => GestioneAPI.GetImmaginePrincipale(v.Categoria, v.Nome, v.Id));
                        _cacheImmagini[chiave] = url;
                    }
                    finally
                    {
                        _limite.Release();
                    }
                }

                v.Miniatura = PaginaFilm.ConvertiMiniatura(url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore immagine Archivio ({v.Nome}): {ex.Message}");
            }
        }

        // Divide la larghezza disponibile in colonne intere (cella minima 176 px) e allarga ogni cella
        // in modo che le schede riempiano la riga fino al bordo destro.
        private void Griglia_SizeChanged(object sender, SizeChangedEventArgs e) => AdattaLarghezzaSchede();

        private void AdattaLarghezzaSchede()
        {
            if (Griglia.ItemsPanelRoot is not ItemsWrapGrid pannello) return;

            double disponibile = Griglia.ActualWidth - 20;   // margine per la barra di scorrimento
            if (disponibile <= 0) return;

            int colonne = Math.Max(1, (int)(disponibile / 176));
            pannello.ItemWidth = Math.Floor(disponibile / colonne);
        }

        private async void ListaFiltri_ItemClick(object sender, ItemClickEventArgs e)
        {
            var indice = Array.IndexOf(Filtri, (string)e.ClickedItem);
            if (indice < 0) return;

            _filtro = (TipoRicerca)indice;
            await AggiornaAsync();
        }

        // Film -> pagina Film già filtrata; tutto il resto -> pagina Dettaglio.
        private void Griglia_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not VoceArchivio v) return;

            if (v.Categoria == TipoRicerca.Film)
                Frame.Navigate(typeof(PaginaFilm), v.Nome);
            else
                Frame.Navigate(typeof(PaginaDettaglio),
                    new ParametroDettaglio { Categoria = v.Categoria, EntitaId = v.Id });
        }
    }
}
