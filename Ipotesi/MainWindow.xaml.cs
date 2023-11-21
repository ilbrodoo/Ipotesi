using Ipotesi.Db;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ipotesi
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Sintomi> _listaSintomi;
        private ObservableCollection<Sintomi> _filteredSintomi;
        private IpotesiEntities _dbContext;

        public MainWindow()
        {
            InitializeComponent();
            _dbContext = new IpotesiEntities();
            _listaSintomi = new ObservableCollection<Sintomi>();
            _filteredSintomi = new ObservableCollection<Sintomi>();

            // Carica i sintomi dal database asincronamente
            Task.Run(async () => await CaricaSintomiDalDatabaseAsync());

            // Assegna direttamente la ObservableCollection al ListBox
            lstSintomi.ItemsSource = _filteredSintomi;
        }

        private async Task CaricaSintomiDalDatabaseAsync()
        {
            try
            {
                // Usa il dbContext inizializzato all'inizio
                var sintomi = await _dbContext.Sintomi.ToListAsync();

                _listaSintomi.Clear();
                foreach (var sintomo in sintomi)
                {
                    _listaSintomi.Add(sintomo);
                }

                // Chiamata per filtrare dopo l'aggiornamento della lista
                FilterSintomi();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il recupero dei sintomi dal database: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RicercaMalattia_Click(object sender, RoutedEventArgs e)
        {
            var sintomiSelezionati = lstSintomi.SelectedItems.Cast<Sintomi>().ToList();

            Console.WriteLine("Sintomi selezionati:");
            foreach (var sintomo in sintomiSelezionati)
            {
                Console.WriteLine($"Sintomo: {sintomo.Sintomo}, Codice: {sintomo.Numeri}");
            }

            // Estrai i codici dai sintomi selezionati
            List<string> codiciSintomi = sintomiSelezionati
                .Select(s => s.Numeri)
                .SelectMany(c => c.Split(' ')) // Utilizza SelectMany per "appiattire" gli array di stringhe
                .Where(c => !string.IsNullOrWhiteSpace(c)) // Filtra gli spazi vuoti
                .ToList();

            Console.WriteLine("Codici sintomi estratti:");
            foreach (var codiceSintomo in codiciSintomi)
            {
                Console.WriteLine($"Codice sintomo: {codiceSintomo}");
            }

            // Dictionary per tenere traccia delle frequenze dei codici delle malattie
            Dictionary<string, int> frequenzeMalattie = new Dictionary<string, int>();

            // Ora hai una lista di codici singoli da confrontare con i codici delle malattie
            // Esegui la tua logica di confronto qui con i codici delle malattie
            foreach (var codiceSintomo in codiciSintomi)
            {
                // Controlla con i codici delle malattie
                // Ad esempio:
                var malattieCorrispondenti = _dbContext.Malattie
                .AsEnumerable()
                .Where(m => m.Codice != null && m.Codice.Split(' ').Any(num => num == codiceSintomo))
                .Select(m => m.Malattia)
                .ToList();

                foreach (var malattia in malattieCorrispondenti)
                {
                    // Aggiorna la frequenza delle malattie nel dizionario
                    if (frequenzeMalattie.ContainsKey(malattia))
                    {
                        frequenzeMalattie[malattia]++;
                    }
                    else
                    {
                        frequenzeMalattie.Add(malattia, 1);
                    }
                }
            }
          

            var malattiaPiùProbabile = frequenzeMalattie.OrderByDescending(kv => kv.Value).FirstOrDefault();

            if (malattiaPiùProbabile.Value > 0)
            {
                Console.WriteLine($"Malattia più probabile: {malattiaPiùProbabile.Key} (Frequenza: {malattiaPiùProbabile.Value})");

                // Aggiorna la TextBox per mostrare la malattia più probabile
                txtMalattieTrovate.Text = malattiaPiùProbabile.Key;
            }
            else
            {
                Console.WriteLine("Nessuna malattia trovata.");
                // Se vuoi gestire il caso in cui non è stata trovata alcuna malattia probabile, puoi
                // inserire qui la logica appropriata.
            }

            // Pulisci la selezione dopo la ricerca
            lstSintomi.SelectedItems.Clear();
            FilterSintomi();
        }


        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterSintomi();
        }

        private void FilterSintomi()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _filteredSintomi.Clear();

                foreach (var sintomo in _listaSintomi)
                {
                    // Aggiungi qui la logica di filtro, ad esempio, cerca nel campo 'Sintomo'
                    if (string.IsNullOrEmpty(txtFilter.Text) || sintomo.Sintomo.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        _filteredSintomi.Add(sintomo);
                    }
                }
            });
        }
    }
}
