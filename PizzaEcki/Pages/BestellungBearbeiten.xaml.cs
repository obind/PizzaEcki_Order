using SharedLibrary;
using System.Windows;
using PizzaEcki.Database;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;

namespace PizzaEcki.Pages
{
    /// <summary>
    /// Interaktionslogik für BestellungBearbeiten.xaml
    /// </summary>
    public partial class BestellungBearbeiten : Window
    {
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
            // Initialisiere _localOrderItems mit den OrderItems der übergebenen Bestellung
            _localOrderItems = new ObservableCollection<OrderItem>(order.OrderItems);
            _currentOrder = order;
            BestellungenListView.ItemsSource = _localOrderItems;

            this.Loaded += BestellungBearbeiten_Loaded;
        }


        public async void ReloadOrderItems()
        {

            try
            {
                // Hole die aktuellen OrderItems für die gegebene OrderId
                var updatedOrderItems = await databaseManager.GetOrderItemsByOrderIdAsync(_currentOrder.OrderId.ToString());
                _localOrderItems.Clear(); // Bestehende Einträge löschen

                foreach (var item in updatedOrderItems)
                {
                    _localOrderItems.Add(item); // Neue, aktuelle Einträge hinzufügen
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Bestellungsartikel: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BestellungBearbeiten_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialisiere die Ansicht
            // Zum Beispiel könntest du hier _localOrderItems an ein UI-Element binden
        }

        private async void Speichern_Click(object sender, RoutedEventArgs e)
        {
            var orderToUpdate = DataContext as Order;
            if (orderToUpdate != null)
            {
                try
                {
                  

                    // Stelle sicher, dass die OrderItems der Bestellung aktualisiert werden
                    orderToUpdate.OrderItems = _localOrderItems.ToList();

                    await databaseManager.UpdateOrderAsync(orderToUpdate);

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
            // Erstelle ein neues OrderItem mit Standardwerten oder UI-Eingaben
            var newItem = new OrderItem
            {
                OrderId = _currentOrder.OrderId,
                Gericht = "Neues Gericht",
                Extras = "",
                Größe = "Standardgröße",
                Menge = 1,
                Epreis = 0.0,
                Gesamt = 0.0,
                LieferungsArt = 0, // Standardlieferart, könnte dynamisch gesetzt werden
                Uhrzeit = DateTime.Now.ToString("HH:mm:ss") // Beispiel für aktuelle Uhrzeit
            };

            // Füge das neue Item zur ObservableCollection hinzu
            _localOrderItems.Add(newItem);

            // Optional: Speichere das neue OrderItem sofort in der Datenbank
            await databaseManager.AddOrderItemAsync(newItem);
            OrderUpdated?.Invoke();
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
    }
}
