using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using SharedLibrary;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Controls;

namespace PizzaKitchenClient
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Order> UnassignedOrders = new ObservableCollection<Order>();

       // private readonly SignalRService signalRService = new SignalRService();
        private ApiService _apiService = new ApiService();
        private DispatcherTimer refreshTimer = new DispatcherTimer();
        private Order _selectedOrder;
        private Order order;
        private bool isErrorMessageDisplayed = false;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            OrdersList.ItemsSource = UnassignedOrders;
            OrdersList.SelectionChanged += OrdersList_SelectionChanged_1;

            refreshTimer.Interval = TimeSpan.FromSeconds(1); // Aktualisiere alle 30 Sekunden
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
            CheckServerConnection();
            
        }
        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            await LoadUnassignedOrdersAsync();
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
           // await signalRService.HubConnection.StartAsync();
        }

        private async Task LoadUnassignedOrdersAsync()
        {
            try
            {
                List<Order> unassignedOrdersFromApi = await _apiService.GetUnassignedOrdersAsync();

                // Entferne alle Bestellungen aus UnassignedOrders, die nicht mehr in unassignedOrdersFromApi sind
                foreach (var order in UnassignedOrders.ToList())
                {
                    if (!unassignedOrdersFromApi.Any(o => o.OrderId == order.OrderId))
                    {
                        UnassignedOrders.Remove(order);
                    }
                }

                // Füge neue Bestellungen aus unassignedOrdersFromApi hinzu, die nicht in UnassignedOrders sind
                foreach (var order in unassignedOrdersFromApi)
                {
                    if (!UnassignedOrders.Any(o => o.OrderId == order.OrderId))
                    {
                        if (!string.IsNullOrEmpty(order.CustomerPhoneNumber))
                        {
                            Customer customer = await GetCustomerByPhoneNumberAsync(order.CustomerPhoneNumber);
                            if (customer != null)
                            {
                                order.Customer = customer;
                            }
                        }
                        UnassignedOrders.Add(order);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden unzugewiesener Bestellungen: " + ex.Message);
            }
        }


        private async Task<Customer> GetCustomerByPhoneNumberAsync(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                // Optional: Zeigen Sie eine Meldung an oder führen Sie eine andere Aktion aus
                MessageBox.Show("Keine Telefonnummer angegeben.");
                return null;
            }

            try
            {
                var customer = await _apiService.GetCustomerByPhoneNumberAsync(phoneNumber);
                return customer;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden des Kunden: " + ex.Message);
                return null;
            }
        }




        private async void DriversComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Driver> driversFromApi = await _apiService.GetAllDriversAsync();
                DriversComboBox.ItemsSource = driversFromApi;
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung hier
                MessageBox.Show("Fehler beim Laden der Fahrer: " + ex.Message);
            }
        }


        private async void OnAssignButtonClicked(object sender, RoutedEventArgs e)
        {
            if (OrdersList.SelectedItem is Order selectedOrder && DriversComboBox.SelectedItem is Driver selectedDriver)
            {
                if (!selectedOrder.IsDelivery)
                {
                    MessageBox.Show("Fahrer können keine Abholungen übernehmen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                double orderPrice = selectedOrder.OrderItems.Sum(item => item.Gesamt);


                try
                {
                    await _apiService.SaveOrderAssignmentAsync(selectedOrder.OrderId.ToString(), selectedDriver.Id, orderPrice);
                    UnassignedOrders.Remove(selectedOrder); // Entferne die Bestellung aus der ObservableCollection
                    DriversComboBox.SelectedItem = null;
                    HighlightSelectedItem(); // Aktualisiere die Hervorhebung
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Zuweisen der Bestellung: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie eine Bestellung und einen Fahrer aus.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private async void CheckServerConnection()
        {
            try
            {
                // Versuche, eine Testanfrage an den Server zu senden
                var response = await _apiService.CheckConnectionAsync();
                if (response.IsSuccessStatusCode)
                {
                    ConnectionStatusLabel.Content = "Verbunden";
                    ConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    ConnectionStatusLabel.Content = "Fehler bei der Verbindung";
                    ConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            catch
            {
                ConnectionStatusLabel.Content = "Server nicht erreichbar";
                ConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void OrdersList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // Aktualisiere das ausgewählte Item
            _selectedOrder = OrdersList.SelectedItem as Order;
            HighlightSelectedItem();
        }


        private void HighlightSelectedItem()
        {
            foreach (var item in OrdersList.Items)
            {
                var listViewItem = (ListViewItem)OrdersList.ItemContainerGenerator.ContainerFromItem(item);
                if (listViewItem != null)
                {
                    listViewItem.Background = item == _selectedOrder ? new SolidColorBrush(Colors.LightBlue) : new SolidColorBrush(Colors.White);
                }
            }
        }
        private void ShowError(string message)
        {
            if (!isErrorMessageDisplayed)
            {
                MessageBox.Show(message);
                isErrorMessageDisplayed = true; // Setze die Flagge, um anzuzeigen, dass die Fehlermeldung bereits angezeigt wurde
            }
        }
    }
}
