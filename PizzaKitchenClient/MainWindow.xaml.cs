using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using SharedLibrary;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using System.Windows.Media;

namespace PizzaKitchenClient
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Order> UnassignedOrders = new ObservableCollection<Order>();

       // private readonly SignalRService signalRService = new SignalRService();
        private ApiService _apiService = new ApiService();
        private DispatcherTimer refreshTimer = new DispatcherTimer();


        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            OrdersList.ItemsSource = UnassignedOrders; 
            LoadUnassignedOrdersAsync().ConfigureAwait(false);

            refreshTimer.Interval = TimeSpan.FromSeconds(2); // Aktualisiere alle 30 Sekunden
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
                List<Order> unassignedOrders = await _apiService.GetUnassignedOrdersAsync();
                UnassignedOrders.Clear(); // Bereinige die ObservableCollection

                foreach (Order order in unassignedOrders)
                {
                    UnassignedOrders.Add(order); // Füge jede Bestellung zur ObservableCollection hinzu
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden unzugewiesener Bestellungen: " + ex.Message);
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

    }
}
