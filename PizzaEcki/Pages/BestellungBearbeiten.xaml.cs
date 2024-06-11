using SharedLibrary;
using System.Windows;
using PizzaEcki.Database;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using PizzaEcki.Services;
using System.Globalization;

namespace PizzaEcki.Pages
{
    /// <summary>
    /// Interaktionslogik für BestellungBearbeiten.xaml
    /// </summary>
    public partial class BestellungBearbeiten : Window
    {
        public int? SelectedDriverId { get; set; }
        public ObservableCollection<Driver> Drivers { get; set; } = new ObservableCollection<Driver>();
        public Func<Task> OnSaveCompleted { get; set; }
        public event Action OrderUpdated;
        
        private DatabaseManager databaseManager = new DatabaseManager();
        private Order _currentOrder;
        // Definiere _localOrderItems als ObservableCollection von OrderItem
        private ObservableCollection<OrderItem> _localOrderItems;
        public BestellungBearbeiten(Order order)
        {
            InitializeComponent();
            this.DataContext = order;
           OrderUpdated += ReloadOrderItems;
           _localOrderItems = new ObservableCollection<OrderItem>(order.OrderItems);
            _currentOrder = order;
            BestellungenListView.ItemsSource = _localOrderItems;
            LoadDriversAsync();
            this.Loaded += BestellungBearbeiten_Loaded;
        }


        public async void ReloadOrderItems()
        {

            try
            {
                var updatedOrderItems = await databaseManager.GetOrderItemsByOrderIdAsync(_currentOrder.OrderId.ToString());
                _localOrderItems.Clear();

                foreach (var item in updatedOrderItems)
                {
                    _localOrderItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Bestellungsartikel: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void LoadDriversAsync()
        {
            var driversList = await databaseManager.GetAllDriversAsync();
            Drivers = new ObservableCollection<Driver>(driversList);
            DriverComboBox.ItemsSource = Drivers;
            DriverComboBox.DisplayMemberPath = "Name";  // Zeigt den Namen in der ComboBox an

            // Wähle den aktuellen Fahrer basierend auf der DriverId in _currentOrder
            var selectedDriver = Drivers.FirstOrDefault(d => d.Id == _currentOrder.DriverId);
            if (selectedDriver != null)
            {
                DriverComboBox.SelectedItem = selectedDriver;
            }
        }
        private void BestellungBearbeiten_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        private async void Speichern_Click(object sender, RoutedEventArgs e)
        {
            var orderToUpdate = DataContext as Order;
            if (orderToUpdate != null)
            {
                try
                {
                    // Aktualisiere zuerst die Bestellungsdaten
                    await databaseManager.UpdateOrderAsync(orderToUpdate);

                    // Extrahiere den ausgewählten Fahrer aus der ComboBox
                    var selectedDriver = DriverComboBox.SelectedItem as Driver;

                    if (selectedDriver != null)
                    {
                        // Berechne die Gesamtsumme der OrderItems
                        double totalPrice = orderToUpdate.OrderItems.Sum(item => item.Gesamt);

                        // Aktualisiere die Zuordnung der Bestellung mit Fahrer und Preis
                        await databaseManager.SaveOrderAssignmentAsync(orderToUpdate.OrderId.ToString(), selectedDriver.Id, totalPrice);
                    }

                    OrderUpdated?.Invoke();
                    MessageBox.Show("Die Bestellung wurde erfolgreich aktualisiert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (OnSaveCompleted != null)
                    {
                        await OnSaveCompleted.Invoke();
                    }

                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ein Fehler ist aufgetreten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Keine Bestellung zum Speichern gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private async void AddNewOrderItem_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new OrderItem
            {
                OrderId = _currentOrder.OrderId,
                Nr = -1,
                Gericht = "",
                Extras = "",
                Größe = "Standardgröße",
                Menge = 1,
                Epreis = 0.0,
                Gesamt = 0.0,
                LieferungsArt = 0,
                Uhrzeit = DateTime.Now.ToString("HH:mm:ss") 
            };
       
           

            
            _localOrderItems.Add(newItem);
        }


        private async void DeleteOrderItem_Click(object sender, RoutedEventArgs e)
        {
            if (BestellungenListView.SelectedItem is OrderItem selectedOrderItem)
            {
                var result = MessageBox.Show("Möchten Sie dieses Gericht wirklich löschen?", "Löschen bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Überprüfe, ob eine gültige OrderItemId vorhanden ist
                    int idToDelete = selectedOrderItem.OrderItemId > 0 ? selectedOrderItem.OrderItemId : selectedOrderItem.Nr;
                    await databaseManager.DeleteOrderItemAsync(idToDelete);
                    _localOrderItems.Remove(selectedOrderItem);

                    OrderUpdated?.Invoke(); // Ereignis auslösen, um die Hauptansicht zu aktualisieren
                }
            }
        }

        private async void Loeschen_Click(object sender, RoutedEventArgs e)
        {
            if (_currentOrder == null)
            {
                MessageBox.Show("Es wurde keine Bestellung ausgewählt.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Möchten Sie diese Bestellung wirklich löschen?", "Bestellung löschen", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                bool deleteSuccess = await databaseManager.DeleteOrderAsync(_currentOrder.OrderId);
                if (deleteSuccess)
                {
                    MessageBox.Show("Die Bestellung wurde erfolgreich gelöscht.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Aktualisiere die UI oder schließe das Fenster, falls nötig
                    OrderUpdated?.Invoke(); // Benachrichtige andere Teile der Anwendung, dass eine Aktualisierung nötig ist
                    this.Close(); // Optional: Schließe das Fenster nach dem Löschen
                }
                else
                {
                    MessageBox.Show("Fehler beim Löschen der Bestellung.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
