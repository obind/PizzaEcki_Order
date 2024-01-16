using PizzaEcki.Database;
using PizzaEcki.Models;
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
    /// Interaktionslogik für DailyEarnings.xaml
    /// </summary>
    public partial class DailyEarnings : Window
    {
        private readonly DatabaseManager _dbManager;

        public ObservableCollection<OrderSummary> DailySalesInfoList { get; set; }

        public DailyEarnings()
        {
            InitializeComponent();
            _dbManager = new DatabaseManager();
            LoadDailySales(DateTime.Now); // Loading the daily sales for the current date
        }

        private void LoadDailySales(DateTime date)
        {
            var orders = _dbManager.GetAllOrders();
            orders = orders.Where(order => DateTime.Parse(order.Timestamp).Date == date.Date).ToList();


            var orderSummaries = new List<OrderSummary>
            {
                new OrderSummary { OrderType = "Auslieferungen", Count = 0, Total = 0.0 },
                new OrderSummary { OrderType = "Selbstabholer", Count = 0, Total = 0.0 },
                new OrderSummary { OrderType = "Mitnehmer", Count = 0, Total = 0.0 }
            };

            foreach (var order in orders)
            {
                int index = -1;
                if (order.CustomerPhoneNumber == "1")
                {
                    index = 1; // Selbstabholer
                }
                else if (order.CustomerPhoneNumber == "2")
                {
                    index = 2; // Mitnehmer
                }
                else if (!string.IsNullOrWhiteSpace(order.CustomerPhoneNumber))
                {
                    index = 0; // Auslieferungen
                }

                if (index != -1)
                {
                    orderSummaries[index].Count += 1;
                    orderSummaries[index].Total += order.OrderItems.Sum(item => item.Gesamt);
                }
            }

            // Add the sum row
            var sumRow = new OrderSummary
            {
                OrderType = "Summe",
                Count = orderSummaries.Sum(s => s.Count),
                Total = orderSummaries.Sum(s => s.Total)
            };

            orderSummaries.Add(sumRow);

            DailySalesInfoList = new ObservableCollection<OrderSummary>(orderSummaries);
            DailySalesDataGrid.ItemsSource = DailySalesInfoList;
        }
    }
}
