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
    /// <summary>
    /// Interaktionslogik für BestellungenFenster.xaml
    /// </summary>
    public partial class BestellungenFenster : Window
    {
        private DatabaseManager _databaseManager;
        private ObservableCollection<SharedLibrary.Order> _orders;

        public BestellungenFenster(List<SharedLibrary.Order> orders)
        {
            InitializeComponent();
            _databaseManager = new DatabaseManager();
            _orders = new ObservableCollection<SharedLibrary.Order>(orders);
            BestellungenListView.ItemsSource = _orders;

            foreach (var order in _orders)
            {
                if (order.CustomerPhoneNumber == "1" || order.CustomerPhoneNumber == "2")
                {
                    order.Customer = new SharedLibrary.Customer();
                }
                else
                {                 
                    order.Customer = _databaseManager.GetCustomerByPhoneNumber(order.CustomerPhoneNumber);
                }
            }

            BestellungenListView.ItemsSource = _orders;
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1 && Keyboard.Modifiers == ModifierKeys.None)
            {
                ShowHelpDialog();
                e.Handled = true; // Ereignis als behandelt markieren
                return;
            }

            if (e.Key == Key.F8)
            {
                var selectedOrder = BestellungenListView.SelectedItem as SharedLibrary.Order;
                if (selectedOrder != null)
                {
                    bool success = await _databaseManager.DeleteOrderAsync(selectedOrder.OrderId);
                    if (success)
                    {
                        _orders.Remove(selectedOrder);
                        MessageBox.Show("Bestellung wurde gelöscht.");
                    }
                    else
                    {
                        MessageBox.Show("Fehler beim Löschen der Bestellung.");
                    }
                }
                else
                {
                    MessageBox.Show("Keine Bestellung ausgewählt.");
                }
            }
        }
        private void ShowHelpDialog()
        {
            string helpText = "F8: Ausgewähltes Gericht Löschen.\n" ;

            MessageBox.Show(helpText, "Hilfe zu Tastenkürzeln");
        }

    }
}
