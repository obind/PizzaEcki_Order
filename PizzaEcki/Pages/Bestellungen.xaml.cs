using PizzaEcki.Database;
using PizzaEcki.Models;
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
            var item = ((FrameworkElement)e.OriginalSource).DataContext as Order;
            if (item != null && (_bearbeitenFenster == null || !_bearbeitenFenster.IsVisible))
            {
                _bearbeitenFenster = new BestellungBearbeiten(item);
                _bearbeitenFenster.OrderUpdated += BestellungAktualisieren; // Ereignis abonnieren
                _bearbeitenFenster.Closed += (s, e) => _bearbeitenFenster.OrderUpdated -= BestellungAktualisieren; // Ereignis abmelden beim Schließen
                _bearbeitenFenster.ShowDialog();
            }
        }

        private async void BestellungBearbeiten_OrderUpdated(object sender, EventArgs e)
        {
            // Aktualisiere deine BestellungenListView
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            // Lade die aktualisierten Daten und aktualisiere die ListView
            var ordersWithAssignedDrivers = await _apiService.GetOrdersWithAssignedDrivers();
            _orders.Clear();
            foreach (var order in ordersWithAssignedDrivers)
            {
                _orders.Add(order);
            }
            BestellungenListView.Items.Refresh();
        }
        private void ApiService_OrderChanged(object sender, OrderChangedEventArgs e)
        {
            // Finde und aktualisiere die Order in der Liste _orders
            var orderToUpdate = _orders.FirstOrDefault(o => o.OrderId == e.Order.OrderId);
            if (orderToUpdate != null)
            {
                var index = _orders.IndexOf(orderToUpdate);
                _orders[index] = e.Order;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    BestellungenListView.Items.Refresh();
                });
            }
        }

        private async void BestellungAktualisieren()
        {
            try
            {
                // Hole die aktuellen Daten aus der Datenbank
                var ordersWithAssignedDrivers = await _apiService.GetOrdersWithAssignedDrivers();
                var allDrivers = await _apiService.GetAllDriversAsync();
                var updatedOrders = new List<Order>();

                foreach (var order in ordersWithAssignedDrivers)
                {
                    if (order.DriverId.HasValue && order.DriverId.Value != -1)
                    {
                        var driver = allDrivers.FirstOrDefault(d => d.Id == order.DriverId.Value);
                        order.Name = driver?.Name ?? "Nicht zugewiesen";
                    }
                    else
                    {
                        order.Name = "Nicht zugewiesen";
                    }

                    updatedOrders.Add(order);
                }

                // Aktualisiere die ObservableCollection
                _orders.Clear();
                foreach (var order in updatedOrders)
                {
                    _orders.Add(order);
                }

                // Aktualisiere die ListView
                BestellungenListView.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Aktualisieren der Bestellungen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



    }
}
