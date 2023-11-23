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

            Task.Run(async () => await CaricaSintomiDalDatabaseAsync());

        
            lstSintomi.ItemsSource = _filteredSintomi;
        }

        private async Task CaricaSintomiDalDatabaseAsync()
        {
            try
            {
                
                var sintomi = await _dbContext.Sintomi.ToListAsync();

                _listaSintomi.Clear();
                foreach (var sintomo in sintomi)
                {
                    _listaSintomi.Add(sintomo);
                }

                
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

       
            List<string> codiciSintomi = sintomiSelezionati
                .Select(s => s.Numeri)
                .SelectMany(c => c.Split(' ')) 
                .Where(c => !string.IsNullOrWhiteSpace(c)) 
                .ToList();

            Console.WriteLine("Codici sintomi estratti:");
            foreach (var codiceSintomo in codiciSintomi)
            {
                Console.WriteLine($"Codice sintomo: {codiceSintomo}");
            }

            Dictionary<string, int> frequenzeMalattie = new Dictionary<string, int>();

          
            foreach (var codiceSintomo in codiciSintomi)
            {
                
                var malattieCorrispondenti = _dbContext.Malattie
                .AsEnumerable()
                .Where(m => m.Codice != null && m.Codice.Split(' ').Any(num => num == codiceSintomo))
                .Select(m => m.Malattia)
                .ToList();

                foreach (var malattia in malattieCorrispondenti)
                {
                  
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


            var malattiePiùProbabili = frequenzeMalattie
             .Where(kv => kv.Value == frequenzeMalattie.Values.Max())
             .ToList();

            if (malattiePiùProbabili.Any())
            {
                Console.WriteLine("Malattie più probabili:");
                foreach (var malattiaPiùProbabile in malattiePiùProbabili)
                {
                    Console.WriteLine($"{malattiaPiùProbabile.Key} (Frequenza: {malattiaPiùProbabile.Value})");
                }

               
                txtMalattieTrovate.Text = $"Malattia/e più probabile in base ai sintomi: {string.Join(Environment.NewLine, malattiePiùProbabili.Select(kv => kv.Key))}";

            }
            else
            {
                Console.WriteLine("Nessuna malattia trovata.");
                
            }

            tabControl.SelectedIndex = 1;
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
               
                var sintomiSelezionati = lstSintomi.SelectedItems.Cast<Sintomi>().ToList();

                _filteredSintomi.Clear();

                foreach (var sintomo in _listaSintomi)
                {
                  
                    if (string.IsNullOrEmpty(txtFilter.Text) || sintomo.Sintomo.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        _filteredSintomi.Add(sintomo);
                    }
                }

               
                foreach (var sintomoSelezionato in sintomiSelezionati)
                {
                    if (_filteredSintomi.Contains(sintomoSelezionato))
                    {
                        lstSintomi.SelectedItems.Add(sintomoSelezionato);
                    }
                }
            });
        }

    }
}
