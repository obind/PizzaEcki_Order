using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using PizzaEcki.Database;
using SharedLibrary; // Importiere den Namespace, in dem sich SharedLibrary befindet

namespace PizzaEcki.Pages
{
    /// <summary>
    /// Interaktionslogik für GelöschteBestellungen.xaml
    /// </summary>
    public partial class GelöschteBestellungen : Window
    {
        private DatabaseManager _databaseManager;
        private ObservableCollection<Order> _deletedOrders;

        public GelöschteBestellungen(List<Order> deletedOrders)
        {
            InitializeComponent();
            _databaseManager = new DatabaseManager();
            _deletedOrders = new ObservableCollection<Order>(deletedOrders);
            DeletedOrdersListView.ItemsSource = _deletedOrders;

            foreach (var order in _deletedOrders)
            {
                if (order.CustomerPhoneNumber == "1" || order.CustomerPhoneNumber == "2")
                {
                    order.Customer = new Customer();
                }
                else
                {
                    order.Customer = _databaseManager.GetCustomerByPhoneNumber(order.CustomerPhoneNumber);
                }

                // Setze den Fahrernamen basierend auf der DriverId
                var drivers = _databaseManager.GetAllDrivers();
                var driver = drivers.Find(d => d.Id == order.DriverId);
                order.Name = driver != null ? driver.Name : "Unbekannt";
            }

            DeletedOrdersListView.ItemsSource = _deletedOrders;
        }
    }
}
