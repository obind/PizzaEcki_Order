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
                    orderToUpdate.OrderItems = _localOrderItems.ToList();

                    await databaseManager.UpdateOrderAsync(orderToUpdate);
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
    }
}
