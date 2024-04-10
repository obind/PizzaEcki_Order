using PizzaEcki.Database;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace PizzaEcki.Pages
{
    public partial class Bestellungen : Window
    {
        private DatabaseManager _apiService;
        private ObservableCollection<Order> _orders;
        public bool IsEditEnabled { get; set; }

        // Hier definieren wir _bearbeitenFenster korrekt
        private BestellungBearbeiten _bearbeitenFenster;

        public Bestellungen(List<Order> orders, bool isEditEnabled)
        {
            InitializeComponent();
            _apiService = new DatabaseManager();
            _orders = new ObservableCollection<Order>(orders);
            BestellungenListView.ItemsSource = _orders;
            this.Loaded += async (sender, e) => await LoadCustomerDataAsync(_orders);
            this.IsEditEnabled = isEditEnabled;
        }

        private async Task LoadCustomerDataAsync(IEnumerable<Order> orders)
        {
            foreach (var order in orders)
            {
                if (!string.IsNullOrEmpty(order.CustomerPhoneNumber))
                {
                    order.Customer =  _apiService.GetCustomerByPhoneNumber(order.CustomerPhoneNumber);
                }
            }
            Dispatcher.Invoke(() => BestellungenListView.Items.Refresh());
        }

        private void BestellungenListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsEditEnabled)
            {
                return;
            }

            var item = ((FrameworkElement)e.OriginalSource).DataContext as Order;
            if (item != null && (_bearbeitenFenster == null || !_bearbeitenFenster.IsVisible))
            {
                // Hier initialisieren wir _bearbeitenFenster korrekt
                _bearbeitenFenster = new BestellungBearbeiten(item); // Stellen Sie sicher, dass ein passender Konstruktor in BestellungBearbeiten existiert
                _bearbeitenFenster.ShowDialog();
            }
        }
    }
}
