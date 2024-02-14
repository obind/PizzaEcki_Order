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
    /// Interaktionslogik für DailyEarnings.xaml
    /// </summary>
    public partial class DailyEarnings : Window
    {
        private readonly DatabaseManager _dbManager;
        public double TotalSum { get; set; }
        private DateTime date = DateTime.Now;   
        public ObservableCollection<OrderSummary> DailySalesInfoList { get; set; }

        public DailyEarnings()
        {
            InitializeComponent();
            _dbManager = new DatabaseManager();
            LoadDailySales(DateTime.Now); // Loading the daily sales for the current date
            this.DataContext = this; // Set the DataContext to this class
           
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
            TotalSum = orderSummaries.Sum(s => s.Total);


            DailySalesInfoList = new ObservableCollection<OrderSummary>(orderSummaries);
            DailySalesDataGrid.ItemsSource = DailySalesInfoList;
        }


        private void CloseDayButton_Click(object sender, RoutedEventArgs e)
        {

                CloseDayAndDeleteOrders();    
          
        }


        private void PrintDailyEarningsSummary()
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 300, 10000);
            printDoc.PrintPage += (sender, e) =>
            {
                Graphics graphics = e.Graphics;


                // Schriftarten
                System.Drawing.Font smallFont = new System.Drawing.Font("Segoe UI", 10,  System.Drawing.FontStyle.Bold);
                    System.Drawing.Font regularFont = new System.Drawing.Font("Segoe UI", 15,  System.Drawing.FontStyle.Regular);
                    System.Drawing.Font boldFont = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                    System.Drawing.Font titleFont = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);

                    float yOffset = 10;

                // Titel
                graphics.DrawString("Tagesstatistik", titleFont, Brushes.Black, 10, yOffset);
                yOffset += titleFont.GetHeight(graphics);

                // Datum
                graphics.DrawString(date.ToString("dd.MM.yyyy"), boldFont, Brushes.Black, 10, yOffset);
                yOffset += boldFont.GetHeight(graphics) + 5;

                // Header für Tabelle
                graphics.DrawString("Bestellung", boldFont, Brushes.Black, 10, yOffset);
                graphics.DrawString("Anzahl", boldFont, Brushes.Black, 110, yOffset);
                graphics.DrawString("Gesamt", boldFont, Brushes.Black, 210, yOffset);
                yOffset += boldFont.GetHeight(graphics) + 5;

                // Tagesverkaufszahlen
                foreach (var summary in DailySalesInfoList)
                {
                    graphics.DrawString(summary.OrderType, smallFont, Brushes.Black, 10, yOffset);
                    graphics.DrawString(summary.Count.ToString(), smallFont, Brushes.Black, 110, yOffset);
                    graphics.DrawString(summary.Total.ToString("C"), smallFont, Brushes.Black, 210, yOffset);
                    yOffset += smallFont.GetHeight(graphics) + 5;
                }

                // Gesamtsumme
                graphics.DrawString("Summe", boldFont, Brushes.Black, 10, yOffset);
                graphics.DrawString(TotalSum.ToString("C"), boldFont, Brushes.Black, 210, yOffset);
            };

            printDoc.Print();
        }

        private async void CloseDayAndDeleteOrders()
        {
            var result = MessageBox.Show("Möchten Sie den Tag wirklich abschließen, die Tagesstatistik drucken und alle Bestellungen löschen?", "Tag abschließen", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Drucken der Tagesstatistik
                PrintDailyEarningsSummary();

                // Löschen aller Bestellungen
                await _dbManager.DeleteDailyOrdersAsync();
       
            }
        }

        // Hilfsmethode, um zu überprüfen, ob der Drucker existiert
        private bool PrinterExists(string printerName)
        {
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                if (printerName == printer)
                    return true;
            }
            return false;
        }


    }
}
