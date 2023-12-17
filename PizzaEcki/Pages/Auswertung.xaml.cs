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
            DisplayEarnings("Yearly");
        }

        private void DisplayEarnings(string period)
        {
            switch (period)
            {
                case "Jährlich":
                    YearlyDataGrid.ItemsSource = _orderAssignments
                        .GroupBy(o => o.Timestamp.Year)
                        .Select(g => new
                        {
                            Year = g.Key,
                            TotalPrice = g.Sum(o => o.Price).ToString("0.00 €").Replace('.', ',') // Formatierung des Gesamtpreises
                        })
                        .ToList();
                    break;
                case "Monatlich":
                    MonthlyDataGrid.ItemsSource = _orderAssignments
                        .GroupBy(o => new { o.Timestamp.Year, o.Timestamp.Month })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            TotalPrice = g.Sum(o => o.Price).ToString("0.00 €").Replace('.', ','),
                            FormattedDate = $"{g.Key.Month:D2}.{g.Key.Year}" // Monat.Jahr
                        })
                        .ToList();
                    break;
                case "Heute":
                    DailyDataGrid.ItemsSource = _orderAssignments
                        .Where(o => o.Timestamp.Date == DateTime.Today)
                        .Select(o => new
                        {
                            Date = o.Timestamp.ToString("dd.MM.yyyy"), // Tag.Monat.Jahr
                            Time = o.Timestamp.ToString("HH:mm"), // Stunden:Minuten
                            FormattedPrice = o.Price.ToString("0.00 €").Replace('.', ',') // Formatierung des Preises
                        })
                        .ToList();
                    break;

            }
        }


        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            // Prüfe, welcher Tab ausgewählt ist
            TabItem selectedTab = TabControl.SelectedItem as TabItem;

            // Bestimme den Zeitraum basierend auf dem Header des ausgewählten Tabs
            string period = selectedTab.Header.ToString();

            // Rufe die entsprechende Druckmethode auf
            switch (period)
            {
                case "Jährlich":
                    PrintYearlyReport(period);
                    break;
                case "Monatlich":
                    PrintMonthlyReport(period);
                    break;
                case "Heute":
                    PrintDailyReport(period);
                    break;
                default:
                    MessageBox.Show("Bitte wählen Sie einen gültigen Zeitraum aus.");
                    break;
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.Source is TabControl tabControl)) return;

            var tabItem = tabControl.SelectedItem as TabItem;
            DisplayEarnings(tabItem?.Header.ToString());
        }

        private void PrintDailyReport(string period)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 300, 10000);
            printDoc.PrintPage += (sender, e) =>
            {
                Graphics graphics = e.Graphics;

                // Schriftarten
                System.Drawing.Font regularFont = new System.Drawing.Font("Segoe UI", 10);
                System.Drawing.Font boldFont = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                System.Drawing.Font titleFont = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);

                // Startposition für das Zeichnen
                float yPos = 10;

                // Überschrift
                string header = $"Auswertung: {period}";
                SizeF headerSize = graphics.MeasureString(header, titleFont);
                graphics.DrawString(header, titleFont, Brushes.Black, (printDoc.DefaultPageSettings.PaperSize.Width - headerSize.Width) / 2, yPos);
                yPos += headerSize.Height + 5;

                // Spaltennamen
                string[] columnNames = { "Datum", "Uhrzeit", "Gesamtpreis" };
                float[] columnWidths = { 100f, 60f, 100f };
                yPos += 2;

                // Zeichne Spaltenüberschriften
                float xPos = 0;
                foreach (var columnName in columnNames)
                {
                    graphics.DrawString(columnName, boldFont, Brushes.Black, xPos, yPos);
                    xPos += columnWidths[Array.IndexOf(columnNames, columnName)];
                }
                yPos += boldFont.GetHeight() + 5;

                // Zeichne die Datenzeilen
                foreach (var assignment in _orderAssignments)
                {
                    xPos = 0;
                    string date = assignment.Timestamp.ToString("dd.MM.yyyy");
                    string time = assignment.Timestamp.ToString("HH:mm");
                    string totalPrice = assignment.Price.ToString("0.00 €").Replace('.', ',');

                    // Zeichne die einzelnen Zellen
                    graphics.DrawString(date, regularFont, Brushes.Black, xPos, yPos);
                    xPos += columnWidths[0];
                    graphics.DrawString(time, regularFont, Brushes.Black, xPos, yPos);
                    xPos += columnWidths[1];
                    graphics.DrawString(totalPrice, regularFont, Brushes.Black, xPos, yPos);

                    yPos += regularFont.GetHeight() + 5;
                }

                // Abschlusszeile
                string footer = "Ende der Auswertung";
                SizeF footerSize = graphics.MeasureString(footer, regularFont);
                graphics.DrawString(footer, regularFont, Brushes.Black, (printDoc.DefaultPageSettings.PaperSize.Width - footerSize.Width) / 2, yPos);
            };

            printDoc.Print();
        }

        private void PrintYearlyReport(string period)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 300, 10000);
            printDoc.PrintPage += (sender, e) =>
            {
                Graphics graphics = e.Graphics;

                // Schriftarten
                System.Drawing.Font regularFont = new System.Drawing.Font("Segoe UI", 10);
                System.Drawing.Font boldFont = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                System.Drawing.Font titleFont = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);


                // Startposition für das Zeichnen
                float yPos = 10;

                // Überschrift
                string header = "Jährliche Auswertung";
                SizeF headerSize = graphics.MeasureString(header, titleFont);
                graphics.DrawString(header, titleFont, Brushes.Black, (printDoc.DefaultPageSettings.PaperSize.Width - headerSize.Width) / 2, yPos);
                yPos += headerSize.Height + 10;

                // Spaltennamen
                string[] columnNames = { "Jahr", "Gesamtpreis" };
                float[] columnWidths = { 150f, 150f };

                // Zeichne Spaltenüberschriften
                float xPos = 10;
                foreach (var columnName in columnNames)
                {
                    graphics.DrawString(columnName, boldFont, Brushes.Black, xPos, yPos);
                    xPos += columnWidths[Array.IndexOf(columnNames, columnName)];
                }
                yPos += boldFont.GetHeight() + 10;

                // Zeichne die Datenzeilen
                foreach (var group in _orderAssignments.GroupBy(o => o.Timestamp.Year)
                            .Select(g => new
                            {
                                Year = g.Key,
                                TotalPrice = g.Sum(o => o.Price).ToString("0.00 €").Replace('.', ',') // Formatierung des Gesamtpreises
                            }))
                {
                    xPos = 10; // Zurücksetzen des X für jede neue Zeile
                    graphics.DrawString(group.Year.ToString(), regularFont, Brushes.Black, xPos, yPos);
                    xPos += columnWidths[0]; // Versetzen des X für die nächste Spalte
                    graphics.DrawString(group.TotalPrice, regularFont, Brushes.Black, xPos, yPos);
                    yPos += regularFont.GetHeight() + 5;
                }

                // Abschlusszeile
                yPos += 10; // Etwas Platz lassen
                string footer = "Ende der Auswertung";
                SizeF footerSize = graphics.MeasureString(footer, regularFont);
                graphics.DrawString(footer, regularFont, Brushes.Black, (printDoc.DefaultPageSettings.PaperSize.Width - footerSize.Width) / 2, yPos);
            };

            printDoc.Print();
        }

        private void PrintMonthlyReport(string period)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 300, 10000);
            printDoc.PrintPage += (sender, e) =>
            {
                Graphics graphics = e.Graphics;

                // Define fonts
                // Schriftarten
                System.Drawing.Font regularFont = new System.Drawing.Font("Segoe UI", 10);
                System.Drawing.Font boldFont = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                System.Drawing.Font titleFont = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);


                // Initial vertical position
                float yPos = 10;

                // Print header
                string header = "Monatliche Auswertung";
                SizeF headerSize = graphics.MeasureString(header, titleFont);
                graphics.DrawString(header, titleFont, Brushes.Black, (printDoc.DefaultPageSettings.PaperSize.Width - headerSize.Width) / 2, yPos);
                yPos += headerSize.Height + 5;

                // Column names
                string[] columnNames = { "Jahr", "Monat", "Gesamtpreis" };
                float[] columnWidths = { 100f, 100f, 100f };

                // Print column headers
                float xPos = 10;
                foreach (var columnName in columnNames)
                {
                    graphics.DrawString(columnName, boldFont, Brushes.Black, xPos, yPos);
                    xPos += columnWidths[Array.IndexOf(columnNames, columnName)];
                }
                yPos += boldFont.GetHeight() + 5;

                // Print data rows
                var monthlyGroups = _orderAssignments
                    .GroupBy(o => new { o.Timestamp.Year, o.Timestamp.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalPrice = g.Sum(o => o.Price).ToString("0.00 €").Replace('.', ',')
                    });

                foreach (var group in monthlyGroups)
                {
                    xPos = 10; // Reset horizontal position
                    graphics.DrawString(group.Year.ToString(), regularFont, Brushes.Black, xPos, yPos);
                    xPos += columnWidths[0];
                    graphics.DrawString(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(group.Month), regularFont, Brushes.Black, xPos, yPos);
                    xPos += columnWidths[1];
                    graphics.DrawString(group.TotalPrice, regularFont, Brushes.Black, xPos, yPos);

                    yPos += regularFont.GetHeight() + 5;
                }

                // Print footer
                yPos += 10; // Add some space before the footer
                string footer = "Ende der Auswertung";
                SizeF footerSize = graphics.MeasureString(footer, regularFont);
                graphics.DrawString(footer, regularFont, Brushes.Black, (printDoc.DefaultPageSettings.PaperSize.Width - footerSize.Width) / 2, yPos);
            };

            printDoc.Print();
        }


    }
}
