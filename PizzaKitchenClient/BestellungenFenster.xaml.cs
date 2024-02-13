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

        public BestellungenFenster(List<Order> orders)
        {
            InitializeComponent();
            _apiSevice = new ApiService();
            _orders = new ObservableCollection<Order>(orders);
            BestellungenListView.ItemsSource = _orders;

            Dispatcher.BeginInvoke(new Action(async () => await LoadCustomerDataAsync(_orders)));
        }


        private async Task LoadCustomerDataAsync(IEnumerable<Order> orders)
        {
            foreach (var order in orders)
            {
                if (order.CustomerPhoneNumber == "1" || order.CustomerPhoneNumber == "2")
                {
                    order.Customer = new Customer(); // Für Selbstabholer und Mitnehmer
                }
                else
                {
                    order.Customer = await _apiSevice.GetCustomerByPhoneNumberAsync(order.CustomerPhoneNumber);
                }
            }

            // Aktualisiere die UI, um die geladenen Kundendaten anzuzeigen
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
                  
                    await UpdateOrderListAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Zuweisen der Bestellung: " + ex.Message);
                }
            }
        }



        private async void BestellungenListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var contextMenu = (sender as ListView)?.ContextMenu;
            contextMenu?.Items.Clear();

            try
            {
                var drivers = await _apiSevice.GetAllDriversAsync();
                foreach (var driver in drivers)
                {
                    // Überspringe bestimmte Fahrer, falls notwendig
                    if (driver.Id == 0 || driver.Id <= -1) continue;

                    var menuItem = new MenuItem
                    {
                        Header = driver.Name, // Verwende den Namen des Fahrers als Menütext
                        CommandParameter = driver // Setze das Driver-Objekt als CommandParameter, um es später zu verwenden
                    };
                    menuItem.Click += DriverMenuItem_Click; // Verknüpfe jedes MenuItem mit einem Event Handler
                    contextMenu?.Items.Add(menuItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Fahrer: " + ex.Message);
            }
        }
   
        private async Task UpdateOrderListAsync()
        {
            try
            {
                var ordersWithAssignedDrivers = await _apiSevice.GetOrdersWithAssignedDriversAsync();
                var allDrivers = await _apiSevice.GetAllDriversAsync();

                // Aktualisiere die UI direkt, anstatt ein neues Fenster zu öffnen
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _orders.Clear(); // Bestehende Einträge löschen

                    foreach (var order in ordersWithAssignedDrivers)
                    {
                        if (order.DriverId.HasValue && order.DriverId > 0)
                        {
                            var driver = allDrivers.FirstOrDefault(d => d.Id == order.DriverId.Value);
                            if (driver != null)
                            {
                                order.Name = driver.Name; // Dies löst das PropertyChanged-Ereignis aus
                            }
                            Dispatcher.BeginInvoke(new Action(async () => await LoadCustomerDataAsync(_orders)));

                            _orders.Add(order);

                        }

                     }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Bestellungen und Fahrer: {ex.Message}");
            }
        }


    }
}
