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

        // Definiere _localOrderItems als ObservableCollection von OrderItem
        private ObservableCollection<OrderItem> _localOrderItems;

        public BestellungBearbeiten(Order order)
        {
            InitializeComponent();
            this.DataContext = order;

            // Initialisiere _localOrderItems mit den OrderItems der übergebenen Bestellung
            _localOrderItems = new ObservableCollection<OrderItem>(order.OrderItems);

            this.Loaded += BestellungBearbeiten_Loaded;
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
                    var databaseManager = new DatabaseManager();

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

        private void AddNewOrderItem_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new OrderItem
            {
                Gericht = "Neues Gericht",
                Extras = "",
                Größe = "",
                Menge = 1,
                Epreis = 0,
                Gesamt = 0,
            };

            _localOrderItems.Add(newItem);
        }
    }
}
