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
        // Verwende ObservableCollection, um die UI automatisch zu aktualisieren
        private ObservableCollection<SharedLibrary.Order> _orders;

        public BestellungenFenster(List<SharedLibrary.Order> orders)
        {
            InitializeComponent();
            _databaseManager = new DatabaseManager(); // Stelle sicher, dass DatabaseManager initialisiert wird
            _orders = new ObservableCollection<SharedLibrary.Order>(orders);
            BestellungenListView.ItemsSource = _orders;
            foreach (var order in _orders)
            {
                // Prüfe den Wert der CustomerPhoneNumber
                if (order.CustomerPhoneNumber == "1" || order.CustomerPhoneNumber == "2")
                {
                    // Für Selbstabholer und Mitnehmer - setze ein leeres Customer-Objekt oder handle es anders
                    order.Customer = new SharedLibrary.Customer();
                }
                else
                {
                    // Für normale Bestellungen mit einer gültigen Telefonnummer
                    order.Customer = _databaseManager.GetCustomerByPhoneNumber(order.CustomerPhoneNumber);
                }
            }


            BestellungenListView.ItemsSource = _orders;


        }

        private void LoadOrders(string bestellungsTyp)
        {
            // Hier würdest du deine Datenquelle abfragen, um die Bestellungen zu erhalten
            // Die folgende Zeile ist nur ein Platzhalter für die tatsächliche Datenabfrage
            //var bestellungen = BestellungenDatenService.GetBestellungenNachTyp(bestellungsTyp);

            // Setze die erhaltenen Bestellungen als ItemsSource für deine ListView
           // BestellungenListView.ItemsSource = bestellungen;
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
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
    }
}
