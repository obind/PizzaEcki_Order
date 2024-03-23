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
    /// Interaktionslogik für Bestellungen.xaml
    /// </summary>
    public partial class Bestellungen : Window
    {
        private DatabaseManager _apiSevice;
        private ObservableCollection<SharedLibrary.Order> _orders;
        private BestellungBearbeiten _bearbeitenFenster;
        public bool IsEditEnabled { get; set; }
        public Bestellungen(List<Order> orders, bool isEditEnabled)
        {
            InitializeComponent();
            _apiSevice = new DatabaseManager();
            _orders = new ObservableCollection<Order>(orders);
            BestellungenListView.ItemsSource = _orders; // Hier wird die ItemsSource gesetzt
            Dispatcher.BeginInvoke(new Action(async () => await LoadCustomerDataAsync(_orders)));
            BestellungenListView.MouseDoubleClick += BestellungenListView_MouseDoubleClick;
            this.IsEditEnabled = isEditEnabled; // Verwende 'this', um die Eigenschaft zu setzen
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
                    order.Customer =  _apiSevice.GetCustomerByPhoneNumber(order.CustomerPhoneNumber);
                }
            }

            // Aktualisiere die UI, um die geladenen Kundendaten anzuzeigen
            BestellungenListView.Items.Refresh();
        }

        private async Task UpdateOrderListAsync()
        {
            try
            {
                var ordersWithAssignedDrivers = await _apiSevice.GetOrdersWithAssignedDrivers();
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

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.IsReadOnly = false;
                textBox.Background = Brushes.White; // Optional: Hintergrundfarbe ändern, um den Bearbeitungsmodus zu signalisieren
            }
        }

        private void BestellungenListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (!IsEditEnabled)
            {
                return;
            }   
            // Prüfe, ob das Bearbeitungsfenster bereits existiert und geöffnet ist
            if (_bearbeitenFenster != null && _bearbeitenFenster.IsVisible)
            {
                // Bring das bereits geöffnete Fenster in den Vordergrund
                _bearbeitenFenster.Activate();
                e.Handled = true; // Verhindere weiteres Bubbling des Events
            }
            else
            {
                var item = ((FrameworkElement)e.OriginalSource).DataContext as Order;
                if (item != null)
                {
                    _bearbeitenFenster = new BestellungBearbeiten
                    {
                        DataContext = item
                    };
                    _bearbeitenFenster.Closed += (s, args) => _bearbeitenFenster = null; // Setze es auf null, sobald das Fenster geschlossen wird
                    _bearbeitenFenster.ShowDialog();
                    e.Handled = true; // Verhindere weiteres Bubbling des Events
                }
            }
        }




    }
}
