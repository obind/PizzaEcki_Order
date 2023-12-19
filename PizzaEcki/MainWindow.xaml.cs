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
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Threading;

namespace PizzaEcki
{

    public partial class MainWindow : Window
    {
        private StringBuilder extrasStringBuilder = new StringBuilder();
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
        private bool isProgrammaticChange = false;

        private BestellungenFenster _bestellungenFenster;
        private string _currentBestellungsTyp;

        private DispatcherTimer _reloadTimer;

        public List<Order> orders;
        public MainWindow()
        {
            InitializeComponent();

            //signalRService = new SignalRService();
            //signalRService.StartConnectionAsync();

            // Erstellt eine neue Instanz von DatabaseManager, um die Verbindung zur Datenbank zu verwalten
            // und alle erforderlichen Tabellen und Initialdaten zu initialisieren.
            _databaseManager = new DatabaseManager();

            // Füllen die ComboBox für Gerichte aus der Datenbank
            dishesList = _databaseManager.GetAllDishes();
            DishComboBox.ItemsSource = dishesList;

            // Füllen die ComboBox für Extras aus der Datenbank
            extrasList = _databaseManager.GetExtras();
            ExtrasComboBox.ItemsSource = extrasList;

            //Den Time Picker Vorbereiten zum Programm start 
            TimePickermein.Value = null;      
            TimePickermein.TimeInterval = new TimeSpan(0, 30, 0);

            LoadDrivers();
            DataContext = this;
            currentBonNumber = _databaseManager.CheckAndResetBonNumberIfNecessary();


            _reloadTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _reloadTimer.Tick += ReloadTimer_Tick;     
            _reloadTimer.Start();
        }
        

     
        private void ReloadTimer_Tick(object sender, EventArgs e)
        {
            // Rufe deine Methode hier auf
            ReloadeUnassignedOrders();
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
                            MessageBoxResult result = MessageBox.Show("Es ist noch kein Kunde mit der Nummer bekannt. \nWollen Sie einen anlegen?", "Frage", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                SaveButton.Visibility = Visibility.Visible;
                                SaveButtonBorder.Visibility = Visibility.Visible;
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
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isProgrammaticChange) // prüft, ob die Änderung durch den Benutzer und nicht durch den Code gemacht wurde
            {
                SaveButton.Visibility = Visibility.Visible; // Zeigt den "Speichern"-Button an
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
            isProgrammaticChange = true;
            NameTextBox.Text = customer.Name;
            StreetTextBox.Text = customer.Street;
            CityTextBox.Text = customer.City;
            AdditionalInfoTextBox.Text = customer.AdditionalInfo;
            isProgrammaticChange = false;
        }
        private void OnCustomerDataChanged(object sender, TextChangedEventArgs e)
        {
            if (!isProgrammaticChange)
            {
                SaveButton.Visibility = Visibility.Visible;
            }
        }
       //Gerichte
        private void DishComboBox_TextChanged(object sender, SelectionChangedEventArgs e)
        {
            // Prüfe, ob ein Gericht ausgewählt ist
            if (DishComboBox.SelectedItem == null)
            {
                SizeComboBox.ItemsSource = null; // Leere die SizeComboBox, wenn kein Gericht ausgewählt ist
                
                tempOrderItem.Gericht = "";
                tempOrderItem.Nr = 0;
                return;
            }

            SizeComboBox.SelectedItem = "L";

            // Umwandeln des ausgewählten Items in ein Dish-Objekt, um auf dessen Eigenschaften zugreifen zu können
            Dish selectedDish = (Dish)DishComboBox.SelectedItem;

            // Aktualisiere das temporäre OrderItem mit den Details des ausgewählten Gerichts
            tempOrderItem.Gericht = selectedDish.Name.ToString();
            tempOrderItem.OrderItemId = selectedDish.Id;

            //tempOrderItem.Epreis = selectedDish.Preis;

            // Ermittle die verfügbaren Größen für die Kategorie des ausgewählten Gerichts

            var sizes = DishSizeManager.CategorySizes[selectedDish.Kategorie];

            // Fülle die SizeComboBox mit den verfügbaren Größen für das ausgewählte Gericht
            SizeComboBox.ItemsSource = sizes;


            // Wenn nur eine Größe verfügbar ist, wähle sie automatisch aus
            if (sizes.Count == 1)
            {
                SizeComboBox.SelectedIndex = 0;
            }


            //tempOrderItem.Epreis = GetPriceForSelectedSize(selectedDish, selectedSize);
            // Leere die ausgewählten Extras, da sich das ausgewählte Gericht geändert hat
            tempOrderItem.Extras = "";
        }

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
                return;  // Wenn der Benutzer löscht, tun wir nichts
            }

            string text = textBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;  // Wenn der Text leer ist, tun wir nichts
            }

            var segments = text.Split(',');
            var lastSegment = segments.Last().Trim();

            bool isRemoving = lastSegment.StartsWith("-");
            string lookupText = isRemoving ? lastSegment.Substring(1) : lastSegment;

            // Stellen Sie sicher, dass lookupText mindestens einen Buchstaben hat,
            // bevor die Autovervollständigung ausgeführt wird
            if (lookupText.Length < 1)
            {
                return;
            }

            var matchingExtra = extrasList
                .FirstOrDefault(extra => extra.Name.StartsWith(lookupText, StringComparison.OrdinalIgnoreCase));

            if (matchingExtra != null)
            {
                // Ermittle den Index des letzten Segments im Text
                int lastSegmentIndex = text.LastIndexOf(lastSegment);
                // Ersetze den letzten Segmenttext durch den Autovervollständigungstext
                string newText = text.Substring(0, lastSegmentIndex) + (isRemoving ? "-" : "") + matchingExtra.Name;
                int textStart = textBox.SelectionStart;
                textBox.Text = newText;
                textBox.SelectionStart = textStart;
                textBox.SelectionLength = textBox.Text.Length - textStart;
            }
        }
        private void ExtrasComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)ExtrasComboBox.Template.FindName("PART_EditableTextBox", ExtrasComboBox);
            if (textBox != null)
            {
                textBox.TextChanged += ExtrasTextBox_TextChanged;
                textBox.PreviewKeyDown += ExtrasComboBox_PreviewKeyDown;
            }
        }
        private async void ExtrasComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }

            if (e.Key == Key.Space)
            {
                string text = textBox.Text;
                if (!text.EndsWith(", "))
                {
                    textBox.Text += ", ";  // Füge ein Komma und ein Leerzeichen hinzu, wenn die Leertaste gedrückt wird
                    textBox.SelectionStart = textBox.Text.Length;  // Setze den Cursor ans Ende des Texts
                }
                e.Handled = true;  // Verhindere, dass die Leertaste ein Leerzeichen einfügt
            }
            else if (e.Key == Key.Enter)
            {

                await Task.Delay(50); 
                tempOrderItem.Extras = textBox.Text;
                amountComboBox.Focus();  // Verschiebe den Fokus zur amountComboBox
            }
        }
        private async void ExtrasComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }

            if (e.Key == Key.Space)
            {
                string text = textBox.Text;
                if (!text.EndsWith(", "))
                {
                    textBox.Text += ", ";  // Füge ein Komma und ein Leerzeichen hinzu, wenn die Leertaste gedrückt wird
                    textBox.SelectionStart = textBox.Text.Length;  // Setze den Cursor ans Ende des Texts
                }
                e.Handled = true;  // Verhindere, dass die Leertaste ein Leerzeichen einfügt
            }
             if (e.Key == Key.Enter)
            {
                tempOrderItem.Extras = textBox.Text;
                await Task.Delay(10);
                amountComboBox.Focus();
                e.Handled = true;
            }
        }
        private bool ShouldProcessOrder()
        {
            return !string.IsNullOrWhiteSpace(tempOrderItem.Extras) && tempOrderItem.Menge > 0;
        }
        //Anzahl in das tempOrderItem Schreiben 
        private void amountComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateTempOrderItemAmount();
                TimePickermein.Focus();
                e.Handled = true;
                
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

                //tempOrderItem.Uhrzeit = TimePicker.Value.Value.ToString("HH:mm");
                ProcessOrder();
               
            }
        }
        private double GetPriceForSelectedSize(Dish selectedDish, string selectedSize)
        {
            switch (selectedSize)
            {
                case "S":
                    return selectedDish.Preis_S;
                case "L":
                    return selectedDish.Preis_L;
                case "XL":
                    return selectedDish.Preis_XL;
                default:
                    return 0;
            }
        }
        private void ProcessOrder()
        {
            if (dishesList.FirstOrDefault(d => d.Name == tempOrderItem.Gericht) == null)
            {
                MessageBox.Show("Bitte gebe ein Gericht an.");
                return;
            }

            if (string.IsNullOrEmpty(tempOrderItem.Gericht) || tempOrderItem.Menge == 0)
            {
                MessageBox.Show("Bitte füllen Sie alle erforderlichen Felder aus.");
                return;
            }

            if (TimePickermein.Value != null)
            {
                // Überprüfe, ob die ausgewählte Uhrzeit in der Zukunft liegt
                if (TimePickermein.Value.Value <= DateTime.Now)
                {
                    MessageBox.Show("Die eingegebene Uhrzeit muss in der Zukunft liegen.");
                    return;
                }
            }

            // Setze den Epreis zurück
            tempOrderItem.Epreis = 0;

            Dish selectedDish = dishesList.FirstOrDefault(d => d.Name == tempOrderItem.Gericht);
            string selectedSize = SizeComboBox.SelectedItem.ToString();
            tempOrderItem.Größe = selectedSize;
            tempOrderItem.Epreis += GetPriceForSelectedSize(selectedDish, selectedSize);

            //Dish selectedDish = dishesList.FirstOrDefault(d => d.Name == tempOrderItem.Gericht);
            //if (selectedDish != null)
            //{
            //    string selectedSize = SizeComboBox.SelectedItem.ToString();
            //    
            //}

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
            ExtrasComboBox.Text = "";
            amountComboBox.Text = "1";

            DishComboBox.Focus();
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
        private void myDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                // Ausgewählte Zeile holen
                var selectedRow = myDataGrid.SelectedItem as OrderItem;
                if (selectedRow != null)
                {
                    // Zeile aus dem DataGrid und der Datenquelle entfernen
                    orderItems.Remove(selectedRow);
                    myDataGrid.ItemsSource = null;
                    myDataGrid.ItemsSource = orderItems;

                    // Gesamtpreis neu berechnen
                    CalculateTotal(orderItems);
                }
            }
        }
        private void CompleteOrder(string paymentMethod)
        {
            if (!orderItems.Any())
            {
                MessageBox.Show("Es wurden keine Order-Items hinzugefügt. Bitte füge mindestens ein Order-Item hinzu, bevor du die Bestellung abschließt.");
                return; // Verlasse die Methode frühzeitig
            }
            if (!isDelivery)
            { 
            
            }

            if (PhoneNumberTextBox.Text != "")
            {
                // Erstelle ein neues Receipt Objekt und fülle es mit den OrderItems und der ReceiptNumber
                Receipt receipt = new Receipt
                {
                    ReceiptNumber = GetNextReceiptNumber(),
                    //OrderItems = GetCurrentOrderItems(),
                };

                var deliveryUntilStr = TimePickermein.Value.HasValue
                    ? TimePickermein.Value.Value.ToString("HH:mm")
                    : "00:00";

                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    BonNumber = ++currentBonNumber, // Erhöhen und zuweisen
                    IsDelivery = isDelivery,
                    OrderItems = orderItems,
                    PaymentMethod = paymentMethod, // Zuweisen der Zahlungsmethode
                    CustomerPhoneNumber = _customerNr,
                    Timestamp = DateTime.Now.ToString("HH:mm"),
                    DeliveryUntil = deliveryUntilStr
                };
             

                _databaseManager.UpdateCurrentBonNumber(currentBonNumber);

                    _databaseManager.SaveOrder(order);
                    SendOrderItems(order);
                    ReloadeUnassignedOrders();

                    if (PhoneNumberTextBox.Text != "1" && PhoneNumberTextBox.Text != "2")

                    {
                        Customer customer = _databaseManager.GetCustomerByPhoneNumber(_customerNr);
                        PrintReceipt(order, customer);
                    }
                    else
                    {
                        PrintReceipt(order, null);
                    }

                    // Leeren Sie die Bestellliste
                    orderItems.Clear();

                    TotalPriceLabel.Content = $"0.00 €";

                    // Aktualisieren Sie die DataGrid-Ansicht, wenn Sie die Liste direkt an die ItemsSource gebunden haben
                    myDataGrid.Items.Refresh();

                    //Aktualisiere Lieferungsart
                    //AuslieferungLabel.Content = Lieferung;
                    //MitnehmerLabel.Content = Mitnehmer;
                    //SelbstabholerLabel.Content = Selbstabholer;
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

            if (e.Key == Key.F4)
            {
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
            //signalRService.SendOrderItemsAsync(order);
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

            // Filtere nur 'Theke' und 'Kasse1'
            var driversForCounter = driversFromDb.Where(d => d.Name == "Theke" || d.Name == "Kasse1").ToList();

            cb_cashRegister.Items.Clear();
            cb_cashRegister.ItemsSource = driversForCounter;
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
                yOffset += addressSize.Height ;  // 10 pixels Abstand

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

                if (isDelivery && customer != null)  // Überprüfen, ob es sich um eine Lieferung handelt
                {
                    string addressStr ="Tel: " + customer.PhoneNumber + "\r\n" + customer.Name+ "\r\n" + customer.Street + "\r\n" + customer.City + "\r\n" + customer.AdditionalInfo;
                    graphics.DrawString(addressStr, boldFont, Brushes.Black, 0, yOffset);

                    // Hier ändern wir den yOffset, um zwei Zeilenhöhen hinzuzufügen, eine für jede Zeile der Adresse.
                    yOffset += boldFont.GetHeight() * 6;  // Anpassen für den Zeilenumbruch in der Adresse
                }

                string bonNumberStr = $"Bon Nummer: {order.BonNumber}";
                SizeF bonNumberSize = graphics.MeasureString(bonNumberStr, boldFont);
                float bonNumberX = (e.PageBounds.Width - bonNumberSize.Width) / 2;
                graphics.DrawString(bonNumberStr, boldFont, Brushes.Black, bonNumberX, yOffset);
                yOffset += bonNumberSize.Height + 10;


                if (isDelivery)
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

        private void btn_tables_Click(object sender, RoutedEventArgs e)
        {
            TableView tableView = new TableView();
            tableView.ShowDialog();
        }

        private void Btn_zuordnen_Click(object sender, RoutedEventArgs e)
        {
            if (cb_bonNummer.SelectedItem != null && cb_cashRegister.SelectedItem != null)
            {
                // Hier nehmen wir an, dass die 'BonNumber' in 'cb_bonNummer' ausgewählt wird
                int selectedBonNumber = (int)cb_bonNummer.SelectedItem;

                // Finde die ausgewählte Order basierend auf der Bonnummer
                Order selectedOrder = orders.FirstOrDefault(o => o.BonNumber == selectedBonNumber);

                // Wir gehen davon aus, dass 'Name' in 'cb_cashRegister' ausgewählt wird
                Driver selectedDriver = (Driver)cb_cashRegister.SelectedItem;

                if (selectedOrder != null && selectedDriver != null)
                {
                    // Berechne den Gesamtpreis der Bestellung
                    double orderPrice = selectedOrder.OrderItems.Sum(item => item.Gesamt); // Hier 'Price' statt 'Gesamt', falls das das korrekte Property ist.

                    // Speichere die Zuordnung
                    _databaseManager.SaveOrderAssignment(selectedOrder.OrderId.ToString(), selectedDriver.Id, orderPrice);

                    // Setze die ausgewählte Items zurück
                    cb_bonNummer.SelectedItem = null;
                    cb_cashRegister.SelectedItem = null;
                    cb_bonNummer.Items.Clear();

                    ReloadeUnassignedOrders();
                }
                else
                {
                    // Fehlerbehandlung, falls keine Order oder kein Driver gefunden wurde
                }
            }
            else
            {
                // Fehlerbehandlung, falls nichts ausgewählt wurde
            }
        }

        private async Task ReloadeUnassignedOrders()
        {
            orders = _databaseManager.GetUnassignedOrders();
            cb_bonNummer.Items.Clear();
            foreach (Order order in orders)
            {
                if (order.IsDelivery == false)
                {
                    cb_bonNummer.Items.Add(order.BonNumber);
                }
            }
            UpdateOrderCounts();
        }

        private void UpdateOrderCounts()
        {
            var orders = _databaseManager.GetUnassignedOrders();
            int auslieferungCount = 0;
            int mitnehmerCount = 0;
            int selbstabholerCount = 0;

            foreach (var order in orders)
            {
                if (order.CustomerPhoneNumber.Length > 2) // Prüft, ob es eine Telefonnummer ist
                {
                    auslieferungCount++;
                }
                else if (order.CustomerPhoneNumber == "1")
                {
                    selbstabholerCount++;
                }
                else if (order.CustomerPhoneNumber == "2")
                {
                    mitnehmerCount++;
                }
            }

            // Aktualisiere die Labels
            AuslieferungLabel.Content = auslieferungCount.ToString();
            MitnehmerLabel.Content = mitnehmerCount.ToString();
            SelbstabholerLabel.Content = selbstabholerCount.ToString();
        }

        private void AuslieferungLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
            BestellungenAnzeigen("");

        }

        private void SelbstabholerLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BestellungenAnzeigen("1");
        }

        private void MitnehmerLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BestellungenAnzeigen("2");
        }
        private void BestellungenAnzeigen(string bestellungsTyp)
        {
            var unassignedOrders = _databaseManager.GetUnassignedOrders();
            List<Order> filteredOrders;

            if (bestellungsTyp == "1" || bestellungsTyp == "2")
            {
                // Filtere Bestellungen für Selbstabholer und Mitnehmer
                filteredOrders = unassignedOrders
                    .Where(order => order.CustomerPhoneNumber == bestellungsTyp)
                    .ToList();
            }
            else
            {
                // Filtere Bestellungen für Auslieferung
                filteredOrders = unassignedOrders
                    .Where(order => !string.IsNullOrEmpty(order.CustomerPhoneNumber) &&
                                    order.CustomerPhoneNumber != "1" &&
                                    order.CustomerPhoneNumber != "2")
                    .ToList();
            }

            // Überprüfe, ob das Fenster für einen anderen Bestellungstyp geöffnet werden soll
            if (_bestellungenFenster != null && _currentBestellungsTyp != bestellungsTyp)
            {
                // Schließe das aktuelle Fenster und setze die Referenz zurück
                _bestellungenFenster.Close();
                _bestellungenFenster = null;
            }

            // Öffne das Fenster nur, wenn es noch nicht existiert
            if (_bestellungenFenster == null)
            {
                _bestellungenFenster = new BestellungenFenster(filteredOrders);
                // Setze den Titel des Fensters basierend auf dem Bestellungstyp
                switch (bestellungsTyp)
                {
                    case "1":
                        _bestellungenFenster.Title = "Bestellungen - Selbstabholer";
                        break;
                    case "2":
                        _bestellungenFenster.Title = "Bestellungen - Mitnehmer";
                        break;
                    default:
                        _bestellungenFenster.Title = "Bestellungen - Auslieferung";
                        break;
                }
                _currentBestellungsTyp = bestellungsTyp; // Speichere den aktuellen Bestellungstyp
                _bestellungenFenster.Closed += (s, e) =>
                {
                    _bestellungenFenster = null;
                    _currentBestellungsTyp = null; // Setze den Bestellungstyp zurück, wenn das Fenster geschlossen wird
                };
                _bestellungenFenster.Show();
            }
            else
            {
                // Wenn das Fenster bereits geöffnet ist, bringe es in den Vordergrund
                _bestellungenFenster.Focus();
            }
        }

        private void Auswertung_Btn_Click(object sender, RoutedEventArgs e)
        {
            var auswertungWindow = new Auswertung();
            auswertungWindow.ShowDialog();
        }
    }
}
