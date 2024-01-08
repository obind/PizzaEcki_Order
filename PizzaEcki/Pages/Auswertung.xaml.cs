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


namespace PizzaEcki.Pages
{
    /// <summary>
    /// Interaktionslogik für Auswertung.xaml
    /// </summary>
    public partial class Auswertung : Window
    {
        private DatabaseManager _databaseManager;
        private List<OrderAssignment> _orderAssignments;

        public Auswertung()
        {
            InitializeComponent();
            _databaseManager = new DatabaseManager();
            _orderAssignments = _databaseManager.GetOrderAssignments();
         
        }


        private void DisplayDailyEarnings()
        {
            var todayAssignments = _orderAssignments
                .Where(o => o.Timestamp.Date == DateTime.Today);

            var groupedByCategory = todayAssignments
                .GroupBy(o => o.DriverId) // Angenommen, DriverId repräsentiert die Kategorie
                .Select(g => new
                {
                    Category = g.Key.ToString(), // Verwende hier eine Methode zur Bestimmung der Kategorie
                    Count = g.Count(),
                    Netto = g.Sum(o => Convert.ToDecimal(o.Price)), // Konvertiere double in decimal
                    Steuer = g.Sum(o => Convert.ToDecimal(o.Price) * 0.19m), // Angenommen, Steuersatz ist 19%
                    Gesamt = g.Sum(o => Convert.ToDecimal(o.Price) * 1.19m) // Gesamtpreis inklusive Steuer
                }).ToList();

            // Binden der gruppierten Liste an das DataGrid
            DailyDataGrid.ItemsSource = groupedByCategory;
        }



        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
           // PrintDailyReport();
        }

        //private void PrintDailyReport()
        //{
        //    PrintDocument printDoc = new PrintDocument();
        //    printDoc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 300, 10000);
        //    printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
        //    printDoc.Print();
        //}

        //private void PrintPage(object sender, PrintPageEventArgs e)
        //{
        //    Graphics graphics = e.Graphics;

        //    // Definiere Schriftarten
        //    Font regularFont = new Font("Segoe UI", 10);
        //    Font boldFont = new Font("Segoe UI", 10, FontStyle.Bold);
        //    Font titleFont = new Font("Segoe UI", 14, FontStyle.Bold);

        //    // Startposition für das Zeichnen
        //    float yPos = 10;

        //    // Überschrift
        //    string header = "Tagesstatistik";
        //    SizeF headerSize = graphics.MeasureString(header, titleFont);
        //    graphics.DrawString(header, titleFont, Brushes.Black,
        //        (printDoc.DefaultPageSettings.PaperSize.Width - headerSize.Width) / 2, yPos);
        //    yPos += headerSize.Height + 5;

        //    // Zeichne die Spaltenüberschriften
        //    string[] columnHeaders = { "Bestellungen", "Anzahl", "Netto", "Steuer", "Gesamt" };
        //    float xPos = 10;
        //    foreach (var columnHeader in columnHeaders)
        //    {
        //        graphics.DrawString(columnHeader, boldFont, Brushes.Black, xPos, yPos);
        //        xPos += 100; // oder eine andere geeignete Breite für die Spalten
        //    }
        //    yPos += boldFont.GetHeight() + 5;

        //    // Zeichne die Datenzeilen
        //    foreach (var item in DailyDataGrid.ItemsSource)
        //    {
        //        xPos = 10;
        //        // Hier musst du die Eigenschaften deines Datenobjekts extrahieren und zeichnen.
        //        // Zum Beispiel:
        //        graphics.DrawString(item.Category, regularFont, Brushes.Black, xPos, yPos);
        //        xPos += 100;
        //        graphics.DrawString(item.Count.ToString(), regularFont, Brushes.Black, xPos, yPos);
        //        // ... zeichne weitere Daten ...

        //        yPos += regularFont.GetHeight() + 5;
        //    }

        //    // Abschlusszeile
        //    string footer = "Ende der Auswertung";
        //    SizeF footerSize = graphics.MeasureString(footer, regularFont);
        //    graphics.DrawString(footer, regularFont, Brushes.Black,
        //        (printDoc.DefaultPageSettings.PaperSize.Width - footerSize.Width) / 2, yPos);
        //}



    }
}
