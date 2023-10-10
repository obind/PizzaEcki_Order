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
        public ObservableCollection<DailySalesInfo> DailySalesInfoList { get; set; }

        public DailyEarnings()
        {
            InitializeComponent();
            _dbManager = new DatabaseManager();
            LoadDailySales(DateTime.Now);  // Laden der täglichen Umsätze für das heutige Datum
        }

        private void LoadDailySales(DateTime date)
        {
            var dailySalesInfoList = _dbManager.GetDailySales(date);
            DailySalesInfoList = new ObservableCollection<DailySalesInfo>(dailySalesInfoList);
            DailySalesDataGrid.ItemsSource = DailySalesInfoList;
            TotalSalesTextBlock.Text = $"{CalculateTotalSales(dailySalesInfoList):C}";  // Formatierung als Währung
        }

        private double CalculateTotalSales(IEnumerable<DailySalesInfo> dailySalesInfoList)
        {
            double totalSales = 0;
            foreach (var info in dailySalesInfoList)
            {
                totalSales += info.DailySales;
            }
            return totalSales;
        }
    }
}
