using Ipotesi.Db;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public MainWindow()
        {
            InitializeComponent();

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
                using (var dbContext = new IpotesiEntities())
                {
                    var sintomi = await Task.Run(() => dbContext.Sintomi.ToList());

                    _listaSintomi.Clear();
                    foreach (var sintomo in sintomi)
                    {
                        _listaSintomi.Add(sintomo);
                    }

                    // Chiamata per filtrare dopo l'aggiornamento della lista
                    FilterSintomi();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il recupero dei sintomi dal database: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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


        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterSintomi();
        }
    }
}
