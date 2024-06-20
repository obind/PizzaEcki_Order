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
        public ObservableCollection<PaymentMethodSummary> PaymentMethodSummaryList { get; set; }

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
            var paymentMethodSummaries = _dbManager.GetDailySales(date);

            DailySalesInfoList = new ObservableCollection<DailySalesInfo>(driverSalesInfoList);
            DailySalesDataGrid.ItemsSource = DailySalesInfoList
                .GroupBy(info => info.Name)
                .SelectMany(g => g.OrderBy(info => info.PaymentMethod));

            TotalSalesTextBlock.Text = $"{CalculateTotalSales(driverSalesInfoList):C}";
            PaymentMethodSummaryList = new ObservableCollection<PaymentMethodSummary>(paymentMethodSummaries);

            // Zahlungsmethoden-Zusammenfassung (neues DataGrid)
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
            string defaultPrinter = new PrinterSettings().PrinterName;

            // Setze den Drucker auf den Standarddrucker
            printDoc.PrinterSettings.PrinterName = defaultPrinter;

            // Überprüfe, ob der Drucker gültig ist
            if (!printDoc.PrinterSettings.IsValid)
            {
                MessageBox.Show("Der Standarddrucker ist nicht gültig. Bitte überprüfen Sie Ihre Druckereinstellungen.", "Druckerfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            printDoc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 300, 10000); // A4-Größe in 1/100 Zoll
            printDoc.PrintPage += (s, ev) =>
            {
                Graphics graphics = ev.Graphics;

                // Schriftarten
                Font titleFont = new Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);
                Font headerFont = new Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
                Font middelFont = new Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
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
                graphics.DrawString("Anzahl", headerFont, Brushes.Black, new PointF(100, yOffset));
                graphics.DrawString("Tagesumsatz", headerFont, Brushes.Black, new PointF(170, yOffset));
                yOffset += headerFont.Height + 5;

                // Tagesumsatz Daten
                foreach (var dailySalesInfo in DailySalesInfoList)
                {
                    graphics.DrawString(dailySalesInfo.Name, contentFont, Brushes.Black, new PointF(10, yOffset));
                    graphics.DrawString(dailySalesInfo.Count.ToString(), contentFont, Brushes.Black, new PointF(110, yOffset)); // Anzahl der Bestellungen (Count
                    graphics.DrawString($"{dailySalesInfo.DailySales:F2} €", contentFont, Brushes.Black, new PointF(200, yOffset));
                    yOffset += contentFont.Height + 10;
                }
                // Spaltenüberschriften für Zahlungsmethoden-Zusammenfassung
                graphics.DrawString("Zahlungsart", middelFont, Brushes.Black, new PointF(10, yOffset));
        
                yOffset += headerFont.Height + 5;


                foreach (var summary in PaymentMethodSummaryList)
                {
                    graphics.DrawString(summary.PaymentMethod, contentFont, Brushes.Black, new PointF(10, yOffset));
                    graphics.DrawString($"{summary.OrderCount} ", contentFont, Brushes.Black, new PointF(110, yOffset));
                    graphics.DrawString($"{summary.TotalSales:F2} €", contentFont, Brushes.Black, new PointF(200, yOffset));
                    yOffset += contentFont.Height + 5;
                }
                // Gesamtumsatz
                graphics.DrawString("Gesamtumsatz: " + TotalSalesTextBlock.Text, headerFont, Brushes.Black, new PointF(10, yOffset));
            };

            try
            {
                printDoc.Print(); // Startet den Druckvorgang
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Drucken: " + ex.Message, "Druckfehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
