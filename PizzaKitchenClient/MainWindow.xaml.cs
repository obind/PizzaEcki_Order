using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using SharedLibrary;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Drawing.Printing;
using System.Drawing;

namespace PizzaKitchenClient
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Order> UnassignedOrders = new ObservableCollection<Order>();

       // private readonly SignalRService signalRService = new SignalRService();
        private ApiService _apiService = new ApiService();
        private DispatcherTimer refreshTimer = new DispatcherTimer();
        private Order _selectedOrder;
        private Order order;
        private bool isErrorMessageDisplayed = false;
        private bool isDelivery = false;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            OrdersList.ItemsSource = UnassignedOrders;
            OrdersList.SelectionChanged += OrdersList_SelectionChanged_1;

            refreshTimer.Interval = TimeSpan.FromSeconds(1); // Aktualisiere alle 30 Sekunden
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
            CheckServerConnection();
            
        }
        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            await LoadUnassignedOrdersAsync();
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
           // await signalRService.HubConnection.StartAsync();
        }

        private async Task LoadUnassignedOrdersAsync()
        {
            try
            {
                List<Order> unassignedOrdersFromApi = await _apiService.GetUnassignedOrdersAsync();

                // Entferne alle Bestellungen aus UnassignedOrders, die nicht mehr in unassignedOrdersFromApi sind
                foreach (var order in UnassignedOrders.ToList())
                {
                    if (!unassignedOrdersFromApi.Any(o => o.OrderId == order.OrderId))
                    {
                        UnassignedOrders.Remove(order);
                    }
                }

                // Füge neue Bestellungen aus unassignedOrdersFromApi hinzu, die nicht in UnassignedOrders sind
                foreach (var order in unassignedOrdersFromApi)
                {
                    if (!UnassignedOrders.Any(o => o.OrderId == order.OrderId))
                    {
                        if (!string.IsNullOrEmpty(order.CustomerPhoneNumber))
                        {
                            Customer customer = await GetCustomerByPhoneNumberAsync(order.CustomerPhoneNumber);
                            if (customer != null)
                            {
                                order.Customer = customer;
                            }
                        }
                        UnassignedOrders.Add(order);
                    }
                }
                isErrorMessageDisplayed = false;
            }
            catch (Exception ex)
            {
                if (!isErrorMessageDisplayed)
                {
                
                    isErrorMessageDisplayed = true; 
                }
            }
        }


        private async Task<Customer> GetCustomerByPhoneNumberAsync(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                // Optional: Zeigen Sie eine Meldung an oder führen Sie eine andere Aktion aus
                MessageBox.Show("Keine Telefonnummer angegeben.");
                return null;
            }

            try
            {
                var customer = await _apiService.GetCustomerByPhoneNumberAsync(phoneNumber);
                return customer;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden des Kunden: " + ex.Message);
                return null;
            }
        }




        private async void DriversComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Driver> driversFromApi = await _apiService.GetAllDriversAsync();
                DriversComboBox.ItemsSource = driversFromApi;
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung hier
                MessageBox.Show("Fehler beim Laden der Fahrer: " + ex.Message);
            }
        }


        private async void OnAssignButtonClicked(object sender, RoutedEventArgs e)
        {
            if (OrdersList.SelectedItem is Order selectedOrder && DriversComboBox.SelectedItem is Driver selectedDriver)
            {
                if (!selectedOrder.IsDelivery)
                {
                    MessageBox.Show("Fahrer können keine Abholungen übernehmen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                double orderPrice = selectedOrder.OrderItems.Sum(item => item.Gesamt);


                try
                {
                    await _apiService.SaveOrderAssignmentAsync(selectedOrder.OrderId.ToString(), selectedDriver.Id, orderPrice);
                    UnassignedOrders.Remove(selectedOrder); // Entferne die Bestellung aus der ObservableCollection
                    DriversComboBox.SelectedItem = null;
                    HighlightSelectedItem(); // Aktualisiere die Hervorhebung
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Zuweisen der Bestellung: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie eine Bestellung und einen Fahrer aus.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private async void CheckServerConnection()
        {
            try
            {
                // Versuche, eine Testanfrage an den Server zu senden
                var response = await _apiService.CheckConnectionAsync();
                if (response.IsSuccessStatusCode)
                {
                    ConnectionStatusLabel.Content = "Verbunden";
                    ConnectionStatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                }
                else
                {
                    ConnectionStatusLabel.Content = "Fehler bei der Verbindung";
                    ConnectionStatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                }
            }
            catch
            {
                ConnectionStatusLabel.Content = "Server nicht erreichbar";
                ConnectionStatusLabel.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            }
        }

        private void OrdersList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // Aktualisiere das ausgewählte Item
            _selectedOrder = OrdersList.SelectedItem as Order;
            HighlightSelectedItem();
        }


        private void HighlightSelectedItem()
        {
            foreach (var item in OrdersList.Items)
            {
                var listViewItem = (ListViewItem)OrdersList.ItemContainerGenerator.ContainerFromItem(item);
              if (listViewItem != null)
                {
                    listViewItem.Background = item == _selectedOrder
                        ? new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#5f6269"))
                        : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);
                }
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder != null)
            {
                Customer customer = null;
                // Hier könntest du die Kundeninformationen abrufen, falls notwendig
                PrintReceipt(_selectedOrder, customer);
            }
            else
            {
                MessageBox.Show("Bitte wähle eine Bestellung zum Drucken aus.");
            }
        }

        private void PrintReceipt(Order order, Customer customer)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 300, 10000);
            printDoc.PrintPage += (sender, e) =>
            {
                Graphics graphics = e.Graphics;

                // Fonts
                Font regularFont = new Font("Segoe UI", 12);
                Font boldFont = new Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
                Font titleFont = new Font("Segoe UI", 24, System.Drawing.FontStyle.Bold);

                float yOffset = 10;  // Initial offset

                string sloagan = "Internationale Spezialitäten";
                float sloaganWidth = graphics.MeasureString(sloagan, regularFont).Width;
                graphics.DrawString(sloagan, regularFont, Brushes.Black, (e.PageBounds.Width - sloaganWidth) / 2, yOffset);
                yOffset += regularFont.GetHeight();

                // Title
                string title = "PIZZA ECKI";
                float titleWidth = graphics.MeasureString(title, titleFont).Width;
                graphics.DrawString(title, titleFont, Brushes.Black, (e.PageBounds.Width - titleWidth) / 2, yOffset);
                yOffset += titleFont.GetHeight();

                // Adresse
                SizeF addressSize = graphics.MeasureString("Woerdener Str. 4 · 33803 Steinhagen", regularFont);
                float addressX = (e.PageBounds.Width - addressSize.Width) / 2;
                graphics.DrawString("Woerdener Str. 4 · 33803 Steinhagen", regularFont, Brushes.Black, addressX, yOffset);
                yOffset += addressSize.Height;  // 10 pixels Abstand

                // Datum und Uhrzeit
                string dateStr = DateTime.Now.ToString("dd.MM.yyyy");
                string timeStr = DateTime.Now.ToString("HH:mm");
                graphics.DrawString(dateStr, regularFont, Brushes.Black, 0, yOffset);
                SizeF timeSize = graphics.MeasureString(timeStr, regularFont);
                graphics.DrawString(timeStr, regularFont, Brushes.Black, e.PageBounds.Width - timeSize.Width - 15, yOffset);
                // 10 pixels Abstand

                Pen blackPen = new Pen(Color.Black, 1);

                // Zeichne eine Trennlinie nach der Überschrift
                graphics.DrawLine(blackPen, 0, yOffset, e.PageBounds.Width, yOffset);
                yOffset += regularFont.GetHeight() + 10;

                if (_selectedOrder.IsDelivery && customer != null)  // Überprüfen, ob es sich um eine Lieferung handelt
                {
                    string addressStr = customer.Name + "\r\n" + customer.Street + "\r\n" + customer.City + "\r\n" + customer.AdditionalInfo;
                    graphics.DrawString(addressStr, boldFont, Brushes.Black, 0, yOffset);

                    // Hier ändern wir den yOffset, um zwei Zeilenhöhen hinzuzufügen, eine für jede Zeile der Adresse.
                    yOffset += boldFont.GetHeight() * 5;  // Anpassen für den Zeilenumbruch in der Adresse
                }

                string bonNumberStr = $"Bon Nummer: {order.BonNumber}";
                SizeF bonNumberSize = graphics.MeasureString(bonNumberStr, boldFont);
                float bonNumberX = (e.PageBounds.Width - bonNumberSize.Width) / 2;
                graphics.DrawString(bonNumberStr, boldFont, Brushes.Black, bonNumberX, yOffset);
                yOffset += bonNumberSize.Height + 10;


                if (_selectedOrder.IsDelivery)
                {
                    string deliveryTimeStr = "Lieferung bis: " + order.DeliveryUntil;
                    graphics.DrawString(deliveryTimeStr, boldFont, Brushes.Black, 0, yOffset);
                    yOffset += regularFont.GetHeight() + 5;
                }

                graphics.DrawLine(blackPen, 0, yOffset, e.PageBounds.Width, yOffset);
                yOffset += 5;

                // Verschoben innerhalb des Ereignishandlers


                // Bestellte Gerichte

                // Definiere einen Stift zum Zeichnen der Linien


                // Tabellenüberschrift
                string headerAnz = "Anz";
                string headerNr = "Nr";
                string headerGericht = "Gericht";
                string headerGr = "Gr.";
                string headerPreis = "Preis";

                // Definiere die Positionen der Spaltenköpfe
                float headerAnzPosition = 0;
                float headerNrPosition = 30;  // 
                float headerGerichtPosition = 50;  // Angenommene Position, anpassen nach Bedarf
                float headerGrPosition = 200;  // Angenommene Position, anpassen nach Bedarf
                                               // Preis rechtsbündig, Abstand vom rechten Rand
                float headerPreisPosition = e.PageBounds.Width - graphics.MeasureString(headerPreis, regularFont).Width - 15;

                // Zeichne die Tabellenüberschrift#
                graphics.DrawString(headerAnz, regularFont, Brushes.Black, headerAnzPosition, yOffset);
                graphics.DrawString(headerNr, regularFont, Brushes.Black, headerNrPosition, yOffset);
                graphics.DrawString(headerGericht, regularFont, Brushes.Black, headerGerichtPosition, yOffset);
                graphics.DrawString(headerGr, regularFont, Brushes.Black, headerGrPosition, yOffset);
                graphics.DrawString(headerPreis, regularFont, Brushes.Black, headerPreisPosition, yOffset);

                // Zeichne eine Trennlinie nach der Überschrift
                yOffset += regularFont.GetHeight();
                graphics.DrawLine(blackPen, 0, yOffset, e.PageBounds.Width, yOffset);
                yOffset += 3; // Füge einen kleinen Abstand nach der Linie hinzu

                // Vorhandener Code für Bestellzeilen ...
                Font extraFont = new Font("Segoe UI", 10);
                foreach (var item in order.OrderItems)
                {
                    string itemNameStr = $"{item.Menge}  x {item.OrderItemId} {item.Gericht}";
                    string itemSizeStr = $"{item.Größe}";
                    string itemPriceStr = $"{item.Gesamt:C}";

                    // Zeichne die Bestellzeile
                    graphics.DrawString(itemNameStr, regularFont, Brushes.Black, headerAnzPosition, yOffset);
                    graphics.DrawString(itemSizeStr, regularFont, Brushes.Black, headerGrPosition, yOffset);
                    SizeF itemPriceSize = graphics.MeasureString(itemPriceStr, regularFont);
                    graphics.DrawString(itemPriceStr, regularFont, Brushes.Black, e.PageBounds.Width - itemPriceSize.Width - 15, yOffset);

                    // Aktualisiere yOffset für die nächste Zeile
                    yOffset += regularFont.GetHeight();

                    // Überprüfe, ob es Extras gibt und zeige sie an
                    if (!string.IsNullOrWhiteSpace(item.Extras))
                    {
                        // Bereite den String mit Extras vor
                        string extrasStr = item.Extras.Trim();
                        // Stelle sicher, dass der String nicht mit einem Komma endet
                        if (extrasStr.EndsWith(","))
                        {
                            extrasStr = extrasStr.Substring(0, extrasStr.Length - 1);
                        }

                        // Zeichne die Extras
                        graphics.DrawString(extrasStr, extraFont, Brushes.Black, headerGerichtPosition + 10, yOffset);

                        // Aktualisiere yOffset für die nächste Zeile
                        yOffset += extraFont.GetHeight();
                    }


                    // Zeichne eine Trennlinie nach den Extras (oder nach dem Gericht, falls keine Extras vorhanden sind)
                    graphics.DrawLine(blackPen, 0, yOffset, e.PageBounds.Width, yOffset);
                    yOffset += 3; // Füge einen kleinen Abstand nach der Linie hinzu
                }

                // Zeichne eine abschließende Trennlinie am Ende der


                // Zeichne eine abschließende Trennlinie am Ende der Bestellliste
                graphics.DrawLine(blackPen, 0, yOffset, e.PageBounds.Width, yOffset);
                yOffset += 10;  // Füge einen größeren Abstand nach der Linie hinzu, um Platz für den Gesamtpreis zu schaffen

                // Berechne den Gesamtpreis
                // Berechne den Gesamtpreis
                decimal gesamtpreis = order.OrderItems.Sum(item => (decimal)item.Gesamt);


                // Zeichne den Gesamtpreis
                string gesamtpreisStr = $"Gesamtpreis: {gesamtpreis:C}";
                SizeF gesamtpreisSize = graphics.MeasureString(gesamtpreisStr, boldFont);
                graphics.DrawString(gesamtpreisStr, boldFont, Brushes.Black, e.PageBounds.Width - gesamtpreisSize.Width - 15, yOffset);

                // Aktualisiere yOffset für möglichen weiteren Inhalt
                yOffset += boldFont.GetHeight() + 20;




                // Bezahlmethode
                string paymentMethodStr = "Bezahlmethode: " + order.PaymentMethod;
                graphics.DrawString(paymentMethodStr, boldFont, Brushes.Black, 0, yOffset);
                yOffset += boldFont.GetHeight();  // Weiterer Abstand für die nächste Zeile

            };


            printDoc.Print();

        }
    }
}
