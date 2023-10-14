using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PizzaEcki.Database;
using System.Linq;
using PizzaEcki.Models;
using Microsoft.AspNetCore.SignalR;
using PizzaEcki.Services;
using SharedLibrary;
using PizzaEcki.Pages;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;

namespace PizzaEcki
{

    public partial class MainWindow : Window
    {
        public ObservableCollection<Driver> Drivers { get; set; }

        private DatabaseManager _databaseManager;
        private List<Dish> dishesList;
        private List<Extra> extrasList;
        private SignalRService signalRService;
        private List<SharedLibrary.OrderItem> orderItems = new List<SharedLibrary.OrderItem>();
        private OrderItem tempOrderItem = new OrderItem();

        public string SelectedPaymentMethod { get; private set; }

        public string _customerNr;

        private int currentReceiptNumber = 0; // das kann auch aus einer Datenbank oder einer Datei gelesen werden
        private int Lieferung = 0;
        private int Selbstabholer = 0;
        private int Mitnehmer = 0;
        private int currentBonNumber;

        private bool isDelivery = false;


        public MainWindow()
        {
            InitializeComponent();

            signalRService = new SignalRService();
            signalRService.StartConnectionAsync();

            // Erstellt eine neue Instanz von DatabaseManager, um die Verbindung zur Datenbank zu verwalten
            // und alle erforderlichen Tabellen und Initialdaten zu initialisieren.
            _databaseManager = new DatabaseManager();

            // Füllen die ComboBox für Gerichte aus der Datenbank
            dishesList = _databaseManager.GetDishes();
            DishComboBox.ItemsSource = dishesList;

            // Füllen die ComboBox für Extras aus der Datenbank
            extrasList = _databaseManager.GetExtras();
            ExtrasComboBox.ItemsSource = extrasList;
                           
            //Den Time Picker Vorbereiten zum Programm start 
            TimePicker.Value = DateTime.Now;
            TimePicker.Value = DateTime.Now.AddMinutes(15);
            TimePicker.TimeInterval = new TimeSpan(0, 30, 0);

            //Fahrer laden 
            LoadDrivers();
            DataContext = this; // Setzen Sie den DataContext auf diese Instanz, damit die Bindung funktioniert

            //bon Nummer Lden
            currentBonNumber = _databaseManager.GetCurrentBonNumber();
        }

        private void PhoneNumberTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Überprüfung, ob die Enter-Taste gedrückt wurde
            if (e.Key == Key.Return)
            {
                _customerNr = PhoneNumberTextBox.Text;

                // Überprüfung, ob die Telefonnummer eingegeben wurde
                if (_customerNr != null && _customerNr != "")
                {
                    // Behandlung von speziellen Telefonnummern "1" und "2"
                    if (_customerNr == "1")
                    {
                        isDelivery = false;
                        Selbstabholer++;
                        DishComboBox.Focus();
                    }
                    else if (_customerNr == "2")
                    {
                        isDelivery = false;
                        Mitnehmer++;
                        DishComboBox.Focus();
                    }
                    else // Suche nach einem Kunden mit der eingegebenen Telefonnummer
                    {
                        Customer customer = _databaseManager.GetCustomerByPhoneNumber(_customerNr);
                        if (customer != null) // Kunde gefunden
                        {
                            //Rufe die MEthode auf um die Textfelder Automatisch zu füllen
                            SetCustomerDataToFields(customer);
                            DishComboBox.Focus();
                            Lieferung++;
                            isDelivery = true;
                        }
                        else // Kunde nicht gefunden
                        {
                            // Aufforderung zur Erstellung eines neuen Kunden
                            MessageBoxResult result = MessageBox.Show("Es ist noch kein Kunde mit der Nummer bekann. \n Wollen sie einen Anelgen?", "Frage", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                SaveButton.Visibility = Visibility.Visible;
                                NameTextBox.Focus();
                                Lieferung++;
                                isDelivery = true;
                            }
                        }
                    }
                }
                else // Fehlermeldung, wenn keine Telefonnummer eingegeben wurde
                {
                    MessageBox.Show("Bitte eine KundenNr eingeben");
                }
            }

            // Zurücksetzen der Felder, wenn die Escape-Taste gedrückt wird
            if (e.Key == Key.Escape)
            {
                PhoneNumberTextBox.Text = string.Empty;
                NameTextBox.Text = string.Empty;
                StreetTextBox.Text = string.Empty;
                CityTextBox.Text = string.Empty;
                AdditionalInfoTextBox.Text = string.Empty;
                SaveButton.Visibility = Visibility.Collapsed;
            }
        }


        private void OnSaveButtonClicked(object sender, RoutedEventArgs e)
        {
            // Erstelle ein neues Customer-Objekt mit den Werten aus den Eingabefeldern
            Customer customer = new Customer
            {
                PhoneNumber = PhoneNumberTextBox.Text,
                Name = NameTextBox.Text,
                Street = StreetTextBox.Text,
                City = CityTextBox.Text,
                AdditionalInfo = AdditionalInfoTextBox.Text
            };

            // Füge den neuen Kunden hinzu oder aktualisiere den bestehenden Kunden
            _databaseManager.AddOrUpdateCustomer(customer);

            // Nach dem Speichern den "SaveButton" wieder ausblenden
            SaveButton.Visibility = Visibility.Collapsed;
        }

        //Methode um die Textfelder automatisch zu füllen
        private void SetCustomerDataToFields(Customer customer)
        {
            NameTextBox.Text = customer.Name;
            StreetTextBox.Text = customer.Street;
            CityTextBox.Text = customer.City;
            AdditionalInfoTextBox.Text = customer.AdditionalInfo;
        }

        //Gerichte
        private void DishComboBox_TextChanged(object sender, SelectionChangedEventArgs e)
        {
            // Prüfe, ob ein Gericht ausgewählt ist
            if (DishComboBox.SelectedItem == null)
            {
                SizeComboBox.ItemsSource = null; // Leere die SizeComboBox, wenn kein Gericht ausgewählt ist
                return;               
            }

            // Umwandeln des ausgewählten Items in ein Dish-Objekt, um auf dessen Eigenschaften zugreifen zu können
            Dish selectedDish = (Dish)DishComboBox.SelectedItem;

            // Aktualisiere das temporäre OrderItem mit den Details des ausgewählten Gerichts
            tempOrderItem.Gericht = selectedDish.Name.ToString();
            tempOrderItem.Nr = selectedDish.Id;
            tempOrderItem.Epreis = selectedDish.Price;

            // Ermittle die verfügbaren Größen für die Kategorie des ausgewählten Gerichts
            var sizes = DishSizeManager.CategorySizes[selectedDish.Category];

            // Fülle die SizeComboBox mit den verfügbaren Größen für das ausgewählte Gericht
            SizeComboBox.ItemsSource = sizes;
            SizeComboBox.SelectedItem = "L"; // Setze "L" als Standardgröße

            // Wenn nur eine Größe verfügbar ist, wähle sie automatisch aus
            if (sizes.Count == 1)
            {
                SizeComboBox.SelectedIndex = 0;
            }

            // Leere die ausgewählten Extras, da sich das ausgewählte Gericht geändert hat
            tempOrderItem.Extras = "";
        }

        //Das ist gnaz nice aber ich weiß nicht genau ob das so notwendig ist oder ob es eine bessere Lösung gibt
        //Es ist dafür da das wenn die autovervollständigung etwas vorschlägt man es mit Enter bestätigen kann und zur nächten Eingabe springt
        private void DishComboBox_AutocompleteKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DishComboBox.IsDropDownOpen)
                {
                    // Ändere den Text der Combobox auf das aktuell hervorgehobene Element (ausgewählte Dish)
                    // Wenn kein Gericht ausgewählt ist, bleibt der aktuelle Text erhalten
                    DishComboBox.Text = (DishComboBox.SelectedItem as Dish)?.Name ?? DishComboBox.Text;

                    // Schließe das Dropdown-Menü der Combobox
                    DishComboBox.IsDropDownOpen = false;
                }

                // Erstelle eine Anforderung, um den Fokus auf das nächste Steuerelement in der Tab-Reihenfolge zu setzen
                SizeComboBox.Focus();

                // Markiere das Ereignis als behandelt, um zu verhindern, dass andere Handler darauf reagieren
                e.Handled = true;
            }
        }

        //Extras
        private void ExtrasComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)ExtrasComboBox.Template.FindName("PART_EditableTextBox", ExtrasComboBox);
            if (textBox != null)
            {
                textBox.TextChanged += ExtrasTextBox_TextChanged;
            }
        }

        private void ExtrasTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }

            bool isDeleting = e.Changes.Any(change => change.RemovedLength > 0);
            if (isDeleting)
            {
                return;
            }

            string text = textBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            // Split the text by commas and get the last segment for auto-completion
            var segments = text.Split(',');
            var lastSegment = segments.Last().Trim();

            bool isRemoving = lastSegment.StartsWith("-");
            string lookupText = isRemoving ? lastSegment.Substring(1) : lastSegment;

            var matchingExtra = extrasList
                .FirstOrDefault(extra => extra.Name.StartsWith(lookupText, StringComparison.OrdinalIgnoreCase));

            if (matchingExtra != null)
            {
                int textStart = textBox.SelectionStart;
                // Replace the last segment with the auto-completed text
                segments[segments.Length - 1] = (isRemoving ? "-" : "") + matchingExtra.Name;
                textBox.Text = string.Join(", ", segments);
                textBox.SelectionStart = textStart;
                textBox.SelectionLength = textBox.Text.Length - textStart;
            }

            tempOrderItem.Extras = textBox.Text;
        }

        private void ExtrasComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                amountComboBox.Focus();
            }
        }

        //Anzahl in das tempOrderItem Schreiben 
        private void amountComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateTempOrderItemAmount();
                ProcessOrder();
            }
        }

        private void UpdateTempOrderItemAmount()
        {
            int amount;
            if (int.TryParse(amountComboBox.Text, out amount) && amount > 0)
            {
                tempOrderItem.Menge = amount;
            }
            else
            {
                MessageBox.Show("Bitte geben Sie eine gültige Nummer ein.");
                amountComboBox.Focus();
            }
        }

        private void TimePicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessOrder();
            }
        }

        private void ProcessOrder()
        {
            // Überprüfen Sie, ob alle erforderlichen Felder ausgefüllt sind
            if (string.IsNullOrEmpty(tempOrderItem.Gericht) || tempOrderItem.Menge == 0)
            {
                MessageBox.Show("Bitte füllen Sie alle erforderlichen Felder aus.");
                return;
            }

            // Überprüfen Sie, ob die eingegebene Uhrzeit in der Zukunft liegt
            if (TimePicker.Value == null || TimePicker.Value.Value <= DateTime.Now)
            {
                MessageBox.Show("Die eingegebene Uhrzeit muss in der Zukunft liegen.");
                return;
            }

            // Setzen Sie den Epreis zurück
            tempOrderItem.Epreis = 0;

            // Berechnen Sie den Epreis basierend auf dem Preis des Gerichts und der Extras
            if (tempOrderItem.Gericht != null)
            {
                Dish selectedDish = dishesList.FirstOrDefault(d => d.Name == tempOrderItem.Gericht);
                if (selectedDish != null)
                {
                    tempOrderItem.Epreis += selectedDish.Price;
                }
            }
            if (tempOrderItem.Extras != null)
            {
                var extras = tempOrderItem.Extras.Split(',').Select(extra => extra.Trim());  // Konvertieren Sie den Extras-String in ein Array
                foreach (var extra in extras)
                {
                    Extra selectedExtra = extrasList.FirstOrDefault(x => x.Name == extra);
                    if (selectedExtra != null)
                    {
                        tempOrderItem.Epreis += selectedExtra.Price;
                    }
                }
            }


            // Berechnen Sie den Gesamtpreis eines Gerichtes mit Berücksichtigung der Anzahl
            tempOrderItem.Gesamt = tempOrderItem.Epreis * tempOrderItem.Menge;

            // Fügen Sie das tempOrderItem zur Liste hinzu
            tempOrderItem.Nr = orderItems.Count + 1;
            orderItems.Add(tempOrderItem);
            myDataGrid.ItemsSource = null;
            myDataGrid.ItemsSource = orderItems;

            CalculateTotal(orderItems);

            // Setzen Sie tempOrderItem zurück, um bereit für die nächste Eingabe zu sein
            tempOrderItem = new OrderItem();

            // Löschen Sie die Auswahlen und den Text in den ComboBoxen und dem TextBox
            DishComboBox.SelectedItem = null;
            ExtrasComboBox.SelectedItem = null;
            amountComboBox.SelectedItem = null;
        }

        public void CalculateTotal(List<OrderItem> orderItem)
        {
            double gesamtPreis = 0;
            foreach (var item in orderItems)
            {
                gesamtPreis += item.Gesamt;
            }
            TotalPriceLabel.Content = $"{gesamtPreis:F2} €";
        }


        private void BarzahlungBtn(object sender, RoutedEventArgs e)
        {
            CompleteOrder("Barzahlung");
        }

        private void KartenzahlungBtn(object sender, RoutedEventArgs e)
        {
            CompleteOrder("Kartenzahlung");
        }

        private void PaypalBtn(object sender, RoutedEventArgs e)
        {
            CompleteOrder("PayPal");
        }


        private void CompleteOrder(string paymentMethod)
        {
            if (!orderItems.Any())
            {
                MessageBox.Show("Es wurden keine Order-Items hinzugefügt. Bitte füge mindestens ein Order-Item hinzu, bevor du die Bestellung abschließt.");
                return; // Verlasse die Methode frühzeitig
            }

            if (PhoneNumberTextBox.Text != "")
            {
                // Erstelle ein neues Receipt Objekt und fülle es mit den OrderItems und der ReceiptNumber
                Receipt receipt = new Receipt
                {
                    ReceiptNumber = GetNextReceiptNumber(),
                    //OrderItems = GetCurrentOrderItems(),
                };

                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    BonNumber = ++currentBonNumber, // Erhöhen und zuweisen
                    OrderItems = orderItems,
                    PaymentMethod = paymentMethod // Zuweisen der Zahlungsmethode
                };
                _databaseManager.UpdateCurrentBonNumber(currentBonNumber);

                _databaseManager.SaveOrder(order);
                SendOrderItems(order);

                Customer customer = _databaseManager.GetCustomerByPhoneNumber(_customerNr);
                PrintReceipt(order, customer);

        

                // Leeren Sie die Bestellliste
                orderItems.Clear();

                TotalPriceLabel.Content = $"0.00 €";

                // Aktualisieren Sie die DataGrid-Ansicht, wenn Sie die Liste direkt an die ItemsSource gebunden haben
                myDataGrid.Items.Refresh();

                //Aktualisiere Lieferungsart
                AuslieferungLabel.Content = Lieferung;
                MitnehmerLabel.Content = Mitnehmer;
                SelbstabholerLabel.Content = Selbstabholer;
            }
            else
            {
                MessageBox.Show("Bitte eine Kundenummer Eingeben");
            }
        }

        private int GetNextReceiptNumber()
        {
            return ++currentReceiptNumber;
        }

        //Zeigt das Vertecke Menü für Admins
        private void MainWindowEcki_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                if (F1Grid.Visibility == Visibility.Visible)
                {
                    F1Grid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    F1Grid.Visibility = Visibility.Visible;
                }
            }

            //Zeige die Tagesübersicht

            if(e.Key == Key.F4) {
                DailyEarnings dailyEarnings = new DailyEarnings();
                dailyEarnings.ShowDialog();
                
            }

            if (e.Key == Key.F12)
            {
                BarzahlungBtn(null, null); // Du kannst null für sender und RoutedEventArgs übergeben, da sie in deiner Methode nicht verwendet werden
            }
        }
        private void SendOrderItems(Order order)
        {
            signalRService.SendOrderItemsAsync(order);
        }
        private void EinstellungenBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }
        private void LoadDrivers()
        {
            DatabaseManager dbManager = new DatabaseManager();
            List<Driver> driversFromDb = dbManager.GetAllDrivers();

            Drivers = new ObservableCollection<Driver>(driversFromDb);

            DriversComboBox.ItemsSource = driversFromDb;
        }
        private void SizeComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExtrasComboBox.Focus();
            }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            _databaseManager.UpdateCurrentBonNumber(currentBonNumber);
        }
        private void PrintReceipt(Order order, Customer customer)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 315, 10000);
            printDoc.PrintPage += (sender, e) =>
            {
                Graphics graphics = e.Graphics;

                // Fonts
                Font regularFont = new Font("Segoe UI", 12);
                Font boldFont = new Font("Segoe UI", 16, System.Drawing.FontStyle.Bold);
                Font titleFont = new Font("Segoe UI", 24, System.Drawing.FontStyle.Bold);

                float yOffset = 10;  // Initial offset

                // Title
                string title = "PIZZA ECKI";
                float titleWidth = graphics.MeasureString(title, titleFont).Width;
                graphics.DrawString(title, titleFont, Brushes.Black, (e.PageBounds.Width - titleWidth) / 2, yOffset);
                yOffset += titleFont.GetHeight() + 20;

                // Adresse
                SizeF addressSize = graphics.MeasureString("Woerdener Str. 4 · 33803 Steinhagen", regularFont);
                float addressX = (e.PageBounds.Width - addressSize.Width) / 2;
                graphics.DrawString("Woerdener Str. 4 · 33803 Steinhagen", regularFont, Brushes.Black, addressX, yOffset);
                yOffset += addressSize.Height + 10;  // 10 pixels Abstand

                // Datum und Uhrzeit
                string dateStr = DateTime.Now.ToString("dd.MM.yyyy");
                string timeStr = DateTime.Now.ToString("HH:mm");
                graphics.DrawString(dateStr, regularFont, Brushes.Black, 0, yOffset);
                SizeF timeSize = graphics.MeasureString(timeStr, regularFont);
                graphics.DrawString(timeStr, regularFont, Brushes.Black, e.PageBounds.Width - timeSize.Width, yOffset);
                yOffset += regularFont.GetHeight() + 20;  // 10 pixels Abstand

                // Verschoben innerhalb des Ereignishandlers
                string bonNumberStr = $"Bon Nummer: {order.BonNumber}";
                SizeF bonNumberSize = graphics.MeasureString(bonNumberStr, boldFont);
                float bonNumberX = (e.PageBounds.Width - bonNumberSize.Width) / 2;
                graphics.DrawString(bonNumberStr, boldFont, Brushes.Black, bonNumberX, yOffset);
                yOffset += bonNumberSize.Height + 10;

                // Bestellte Gerichte
                foreach (var item in order.OrderItems)
                {
                    string itemNameStr = $"{item.Menge} x {item.Gericht}";
                    string itemPriceStr = $"{item.Gesamt:C}";
                    graphics.DrawString(itemNameStr, regularFont, Brushes.Black, 0, yOffset);
                    SizeF itemPriceSize = graphics.MeasureString(itemPriceStr, regularFont);
                    graphics.DrawString(itemPriceStr, regularFont, Brushes.Black, e.PageBounds.Width - itemPriceSize.Width, yOffset);
                    yOffset += regularFont.GetHeight();
                }
                yOffset += 20;  // 10 pixels Abstand


                if (isDelivery && customer != null)  // Überprüfen, ob es sich um eine Lieferung handelt
                {
                    string addressStr = "Adresse: " + customer.Street + ", " + customer.City;
                    graphics.DrawString(addressStr, boldFont, Brushes.Black, 0, yOffset);
                    yOffset += (int)boldFont.GetHeight();  // Aktualisieren der yOffset für den nächsten zu druckenden Text
                }


                // Bezahlmethode
                string paymentMethodStr = "Bezahlmethode: " + order.PaymentMethod;
                graphics.DrawString(paymentMethodStr, boldFont, Brushes.Black, 0, yOffset);
                yOffset += (int)boldFont.GetHeight();
            };

       
                printDoc.Print();
            
        }

        private void btn_tables_Click(object sender, RoutedEventArgs e)
        {
            TableView tableView = new TableView();
            tableView.ShowDialog();
        }
    }
}
