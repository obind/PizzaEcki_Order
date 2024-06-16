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
            // Fahrer-bezogene Tagesumsätze (altes DataGrid)
            var driverSalesInfoList = _dbManager.GetDailySalesByDriver(date); // Neue Methode für Fahrerumsätze

            DailySalesInfoList = new ObservableCollection<DailySalesInfo>(driverSalesInfoList);
            DailySalesDataGrid.ItemsSource = DailySalesInfoList
                .GroupBy(info => info.Name)
                .SelectMany(g => g.OrderBy(info => info.PaymentMethod));

            TotalSalesTextBlock.Text = $"{CalculateTotalSales(driverSalesInfoList):C}";

            // Zahlungsmethoden-Zusammenfassung (neues DataGrid)
            var paymentMethodSummaries = _dbManager.GetDailySales(date);
            PaymentMethodSummaryDataGrid.ItemsSource = paymentMethodSummaries;
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


        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 300, 10000); // A4-Größe in 1/100 Zoll
            printDoc.PrintPage += (sender, e) =>
            {
                Graphics graphics = e.Graphics;

                // Schriftarten
                Font titleFont = new Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);
                Font headerFont = new Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
                Font contentFont = new Font("Segoe UI", 10);

                float yOffset = 20; // Startposition

                // Titel
                graphics.DrawString("Tagesumsatz Auswertung", titleFont, Brushes.Black, new PointF(10, yOffset));
                yOffset += titleFont.Height + 10;

                // Datum
                graphics.DrawString(DateTime.Now.ToString("dd.MM.yyyy"), headerFont, Brushes.Black, new PointF(10, yOffset));
                yOffset += headerFont.Height + 5;

                // Tabelle Header
                graphics.DrawString("Name", headerFont, Brushes.Black, new PointF(10, yOffset));
                graphics.DrawString("Tagesumsatz", headerFont, Brushes.Black, new PointF(150, yOffset));
                yOffset += headerFont.Height + 5;

                // Tagesumsatz Daten
                foreach (var dailySalesInfo in DailySalesInfoList)
                {
                    graphics.DrawString(dailySalesInfo.Name, contentFont, Brushes.Black, new PointF(10, yOffset));
                    graphics.DrawString($"{dailySalesInfo.DailySales:F2} €", contentFont, Brushes.Black, new PointF(150, yOffset));
                    yOffset += contentFont.Height + 5;
                }

                // Gesamtumsatz
                graphics.DrawString("Gesamtumsatz: " + TotalSalesTextBlock.Text, headerFont, Brushes.Black, new PointF(10, yOffset));
            };

            printDoc.Print(); // Startet den Druckvorgang
        }

    }
}
