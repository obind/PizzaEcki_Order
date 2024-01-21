using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PizzaEcki.Database;
using PizzaEcki.Models;
using System.Globalization;
using System.Drawing; // Für das Grafik-Objekt und Schriftarten
using System.Drawing.Printing; // Für den Druck
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PizzaEcki.Pages
{
    /// <summary>
    /// Interaktionslogik für Auswertung.xaml
    /// </summary>
    public partial class Auswertung : Window
    {
        private readonly DatabaseManager _dbManager;
        public ObservableCollection<DailySalesInfo> DailySalesInfoList { get; set; }

        public Auswertung()
        {
            InitializeComponent();
            _dbManager = new DatabaseManager();
            LoadDailySales(DateTime.Now); 

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
