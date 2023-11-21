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
        private ICollectionView _filteredSintomiView;

        public MainWindow()
        {
            InitializeComponent();

            _listaSintomi = new ObservableCollection<Sintomi>();

            Task.Run(async () => await CaricaSintomiDalDatabaseAsync());

           
            cmbSintomi.ItemsSource = _listaSintomi;

            
            _filteredSintomiView = CollectionViewSource.GetDefaultView(_listaSintomi);
            _filteredSintomiView.Filter = FilterSintomi;
        }

        private async Task CaricaSintomiDalDatabaseAsync()
        {
            try
            {
                using (var dbContext = new IpotesiEntities())
                {
                    // Carica i sintomi dal database in modo asincrono utilizzando Task.Run
                    var sintomi = await Task.Run(() => dbContext.Sintomi.ToList());

                    // Aggiungi questo per il debug
                    foreach (var sintomo in sintomi)
                    {
                        Console.WriteLine(sintomo.Sintomo); // Assicurati di sostituire con il nome corretto della proprietà
                    }

                    // Esegui il reset della ObservableCollection
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _listaSintomi.Clear();
                        foreach (var sintomo in sintomi)
                        {
                            _listaSintomi.Add(sintomo);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il recupero dei sintomi dal database: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool FilterSintomi(object item)
        {
            if (string.IsNullOrEmpty(txtFilter.Text))
                return true;

            
            var sintomo = (Sintomi)item;
            return sintomo.Sintomo.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) != -1;
        }

       
        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _filteredSintomiView.Refresh();
        }
    }
}
