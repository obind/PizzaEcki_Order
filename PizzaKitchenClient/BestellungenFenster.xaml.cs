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

namespace PizzaKitchenClient
{
    /// <summary>
    /// Interaktionslogik für BestellungenFenster.xaml
    /// </summary>
    public partial class BestellungenFenster : Window
    {
        private ApiService _apiSevice;
        private ObservableCollection<SharedLibrary.Order> _orders;

        public BestellungenFenster(List<SharedLibrary.Order> orders)
        {
            InitializeComponent();
            _apiSevice = new ApiService();
            _orders = new ObservableCollection<SharedLibrary.Order>(orders);
            BestellungenListView.ItemsSource = _orders;

            // Asynchron die Kundeninformationen nach dem Laden des Fensters abrufen
            LoadCustomerDataAsync(_orders);
        }

        private async Task LoadCustomerDataAsync(ObservableCollection<SharedLibrary.Order> orders)
        {
            foreach (var order in orders)
            {
                if (order.CustomerPhoneNumber == "1" || order.CustomerPhoneNumber == "2")
                {
                    // Für Selbstabholer und Mitnehmer - setze ein leeres Customer-Objekt oder handle es anders
                    order.Customer = new SharedLibrary.Customer();
                }
                else
                {
                    // Für normale Bestellungen mit einer gültigen Telefonnummer
                    order.Customer = await _apiSevice.GetCustomerByPhoneNumberAsync(order.CustomerPhoneNumber);
                }
            }

            // Dies könnte notwendig sein, um die UI zu aktualisieren, da die Änderungen asynchron erfolgen
            BestellungenListView.Items.Refresh();
        }
    







    private async void DriverMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.CommandParameter is Driver selectedDriver
                && BestellungenListView.SelectedItem is Order selectedOrder)
            {
                // Prüfe auf spezielle Bedingungen wie in deinem Code
                if (selectedOrder.CustomerPhoneNumber == "1" || selectedOrder.CustomerPhoneNumber == "2")
                {
                    MessageBox.Show("Diese Bestellung kann nicht zugewiesen werden.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!selectedOrder.IsDelivery)
                {
                    MessageBox.Show("Fahrer können keine Abholungen übernehmen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                double orderPrice = selectedOrder.OrderItems.Sum(item => item.Gesamt);

                try
                {
                    await _apiSevice.SaveOrderAssignmentAsync(selectedOrder.OrderId.ToString(), selectedDriver.Id, orderPrice);
                    // Hier Logik zum Entfernen der Bestellung aus der Liste oder zur Aktualisierung der UI
                    MessageBox.Show($"Fahrer '{selectedDriver.Name}' wurde der Bestellung {selectedOrder.BonNumber} zugewiesen.", "Zuweisung erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Zuweisen der Bestellung: " + ex.Message);
                }
            }
        }

        public async void LoadOrdersWithDrivers()
        {
            try
            {
                var ordersWithDrivers = await _apiSevice.GetAllOrders();
                BestellungenListView.ItemsSource = ordersWithDrivers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Bestellungen: {ex.Message}");
            }
        }


        private async void BestellungenListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var drivers = await _apiSevice.GetAllDriversAsync();
            var contextMenu = (sender as ListView)?.ContextMenu;
            contextMenu?.Items.Clear();

            foreach (var driver in drivers)
            {
                // Überspringe Fahrer mit ID 0 oder -1
                if (driver.Id == 0 || driver.Id <= -1)
                    continue;

                var menuItem = new MenuItem
                {
                    Header = driver.Name, // Verwende den Namen des Fahrers
                    CommandParameter = driver
                };
                menuItem.Click += DriverMenuItem_Click; // Event Handler für Klick auf Fahrer
                contextMenu?.Items.Add(menuItem);
            }
        }

    }
}
