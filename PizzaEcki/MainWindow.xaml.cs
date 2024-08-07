﻿
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
using System.Diagnostics;
using static PizzaEcki.Pages.SettingsWindow;
using System.IO;
using Microsoft.AspNetCore.Mvc.Formatters;


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
        private List<string> selectedExtras = new List<string>();

        public string SelectedPaymentMethod { get; private set; }
        public string _customerNr;
        private string _currentBestellungsTyp;
        private string _pickupType = "";

        private int currentReceiptNumber = 0;
        private int Auslieferung = 0;
        private int Selbstabholer = 0;
        private int Mitnehmer = 0;
        private int currentBonNumber;

        private bool isDelivery = false;
        private bool isProgrammaticChange = false;
        private bool secretPrintTriggered = false;

        private BestellungenFenster _bestellungenFenster;

        private DispatcherTimer _reloadTimer;
        private Dish _selectedDish;
        public List<Order> orders;
        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
            NameTextBox.TextChanged += OnCustomerDataChanged;
            CityTextBox.TextChanged += OnCustomerDataChanged;
            AdditionalInfoTextBox.TextChanged += OnCustomerDataChanged;
        }

        private void InitializeApplication()
        {
            _databaseManager = new DatabaseManager();
            StartServer();

            dishesList = _databaseManager.GetAllDishes();
            extrasList = _databaseManager.GetExtras();
        
            DishComboBox.ItemsSource = dishesList;   
            ExtrasComboBox.ItemsSource = extrasList;

            TimePickermein.Value = null;
            TimePickermein.TimeInterval = new TimeSpan(0, 30, 0);

            LoadDrivers();
            DataContext = this;
            currentBonNumber = _databaseManager.CheckAndResetBonNumberIfNecessary();
                
            if (string.IsNullOrEmpty(Properties.Settings.Default.SelectedPrinter))
            {
                string defaultPrinterName = new PrinterSettings().PrinterName;

                Properties.Settings.Default.SelectedPrinter = defaultPrinterName;
                Properties.Settings.Default.Save();
            }
            _reloadTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _reloadTimer.Tick += ReloadTimer_Tick;
            _reloadTimer.Start();
            SetDefaultpassword();
            PhoneNumberTextBox.Focus();
        }



        private void StartServer()
        {
            try
            {
                // Zuerst überprüfen, ob der Prozess bereits läuft und ihn beenden
                EndTaskIfRunning("PizzaServer");

                // Pfad zur Server-EXE relativ zum aktuellen Ausführungsverzeichnis
                string serverExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PizzaServer.exe");

                if (File.Exists(serverExePath))
                {
                    Process serverProcess = new Process();
                    serverProcess.StartInfo.FileName = serverExePath;
                    serverProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(serverExePath);
                    serverProcess.StartInfo.CreateNoWindow = true;
                    serverProcess.StartInfo.UseShellExecute = false;
                    serverProcess.Start();
                }
                else
                {
                    MessageBox.Show("Die Server-EXE wurde nicht gefunden: " + serverExePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Der Server konnte nicht gestartet werden: " + ex.Message);
            }
        }

        private void EndTaskIfRunning(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Any())
                {
                    foreach (var process in processes)
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                   
                }

            }
            catch (Exception ex)
            {
            }
        }

        public void RefreshDishesAndExtras()
        {
            dishesList = _databaseManager.GetAllDishes();
            DishComboBox.ItemsSource = dishesList;

            extrasList = _databaseManager.GetExtras();
            ExtrasComboBox.ItemsSource = extrasList;
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            RefreshDishesAndExtras();
        }

        private void ReloadTimer_Tick(object sender, EventArgs e)
        {     
            ReloadeUnassignedOrders();
        }
        private void PhoneNumberTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                _customerNr = PhoneNumberTextBox.Text;

                if (_customerNr != null && _customerNr != "")
                {
                    if (_customerNr == "1")
                    {
                        SaveButton.Visibility = Visibility.Collapsed;
                        isDelivery = false;
                        Selbstabholer++;
                        _pickupType = "Selbstabholer";
                        DishComboBox.Focus();
                    }
                    else if (_customerNr == "2")
                    {
                        SaveButton.Visibility = Visibility.Collapsed;
                        isDelivery = false;
                        Mitnehmer++;
                        _pickupType = "Mitnehmer";
                        DishComboBox.Focus();
                    }
                    else
                    {
                        SaveButton.Visibility = Visibility.Collapsed;
                        Customer customer = _databaseManager.GetCustomerByPhoneNumber(_customerNr);
                        if (customer != null)
                        {
                            SetCustomerDataToFields(customer);
                            DishComboBox.Focus();
                            Auslieferung++;
                            isDelivery = true;


                        }
                        else
                        {                   
                            MessageBoxResult result = MessageBox.Show("Es ist noch kein Kunde mit der Nummer bekannt. \nWollen Sie einen anlegen?", "Frage", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                SaveButton.Visibility = Visibility.Visible;
                                SaveButtonBorder.Visibility = Visibility.Visible;
                                NameTextBox.Focus();
                                Auslieferung++;
                                isDelivery = true;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Bitte eine KundenNr eingeben");
                }
            }

            if (e.Key == Key.Escape)
            {
                PhoneNumberTextBox.Text = string.Empty;
                NameTextBox.Text = string.Empty;
                CustomerStreetTextBox.Text = string.Empty;
                CityTextBox.Text = string.Empty;
                AdditionalInfoTextBox.Text = string.Empty;

                SaveButton.Visibility = Visibility.Collapsed;
            }
        }
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isProgrammaticChange)
            {
                SaveButton.Visibility = Visibility.Visible; 
            }

          

        }
        private void NameTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Lösche den Text der CustomerStreetComboBox
                CustomerStreetTextBox.Text = string.Empty;

                // Setze den Fokus auf die TextBox innerhalb der ComboBox
                CustomerStreetTextBox.Focus();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var textBox = (TextBox)CustomerStreetTextBox.Template.FindName("PART_EditableTextBox", CustomerStreetTextBox);
                    if (textBox != null)
                    {
                        textBox.Focus();
                        textBox.Text = string.Empty; // Stelle sicher, dass der Text leer ist
                        textBox.SelectionStart = textBox.Text.Length; // Setze den Cursor ans Ende
                    }
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        private bool _firstClick = true;
        private List<string> _allStreets;

      

        private string ExtractStreetName(string streetWithNumber)
        {
            // Angenommen, die Hausnummer ist durch ein Leerzeichen vom Straßennamen getrennt
            int index = streetWithNumber.LastIndexOf(' ');
            if (index > 0)
            {
                return streetWithNumber.Substring(0, index);
            }
            return streetWithNumber; // Wenn keine Hausnummer gefunden wurde, den gesamten String zurückgeben
        }

        private void CityTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Enter == e.Key)
            {
                AdditionalInfoTextBox.Focus();
            }
        }

        private void AdditionalInfoTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Enter == e.Key && SaveButton.Visibility == Visibility.Visible)
            {
                OnSaveButtonClicked(sender, e);
            }
            else
            {
                if (Key.Enter == e.Key)
                {
                    DishComboBox.Focus();
                
                }
            }

        }

        private void OnSaveButtonClicked(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(NameTextBox.Text))
            {
                MessageBox.Show("Bitte geben Sie einen Namen ein.");
                return;
            }
            Customer customer = new Customer
            {
                PhoneNumber = PhoneNumberTextBox.Text,
                Name = NameTextBox.Text,
                Street = CustomerStreetTextBox.Text,
                City = CityTextBox.Text,
                AdditionalInfo = AdditionalInfoTextBox.Text
            };

            _databaseManager.AddOrUpdateCustomer(customer);

            SaveButton.Visibility = Visibility.Collapsed;
        }
        //Methode um die Textfelder automatisch zu füllen
        private void SetCustomerDataToFields(Customer customer)
        {
            isProgrammaticChange = true;
            NameTextBox.Text = customer.Name;
            CustomerStreetTextBox.Text = customer.Street;
            CityTextBox.Text = customer.City;
            AdditionalInfoTextBox.Text = customer.AdditionalInfo;
            isProgrammaticChange = false;
        }
        private void OnCustomerDataChanged(object sender, TextChangedEventArgs e)
        {
            if (!isProgrammaticChange)
            {
                SaveButton.Visibility = Visibility.Visible;
                SaveButtonBorder.Visibility = Visibility.Visible;
            }
        }

       
        //Gerichte
        private async void DishComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DishComboBox.SelectedItem == null)
            {
                SizeComboBox.ItemsSource = null;
                PriceLabel.Content = "";
                tempOrderItem.Gericht = "";
                tempOrderItem.Nr = 0;
                return;
            }

            _selectedDish = (Dish)DishComboBox.SelectedItem;
            tempOrderItem.Gericht = _selectedDish.Name.ToString();
            tempOrderItem.OrderItemId = _selectedDish.Id;
            if(tempOrderItem.OrderItemId == 700)
    {
                ShowPartyPizzaPopup(); // Methode zum Anzeigen des Popups
            }

            else
            {
                var sizes = DishSizeManager.CategorySizes[_selectedDish.Kategorie];
                SizeComboBox.ItemsSource = sizes;

                if (sizes.Contains("L") && tempOrderItem.OrderItemId != 700)
                {
                    SizeComboBox.SelectedItem = "L";
                }
                
                else if (sizes.Count > 0)
                {
                    SizeComboBox.SelectedIndex = 0; // Automatisch den ersten Eintrag auswählen
                }

                tempOrderItem.Extras = "";
            }

        }

        private void SizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isHappyHour = IsHappyHour();
            Dish selectedDish = (Dish)DishComboBox.SelectedItem;

            if (selectedDish == null)
            {
                return;
            }

            if (selectedDish.Id == 700)
            {
                PriceLabel.Content = $"{tempOrderItem.Gesamt:F2} €";
                return;
            }

            // Überprüfe, ob Mittagsangebot anwendbar ist und die ausgewählte Größe "L" ist
            if (isHappyHour && SizeComboBox.SelectedItem != null && SizeComboBox.SelectedItem.ToString() == "L" && _orderHelper.IsEligibleForLunchOffer(selectedDish, "L"))
            {
                PriceLabel.Content = $"9.00 €"; // Preis für das Mittagsangebot setzen
            }
            else if (SizeComboBox.SelectedItem != null)
            {
                string selectedSize = SizeComboBox.SelectedItem.ToString();
                double price = GetPriceForSelectedSize(selectedDish, selectedSize);
                PriceLabel.Content = $"{price:F2} €";
                tempOrderItem.Epreis = price;
            }
        }
    


        private void DishComboBox_AutocompleteKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DishComboBox.IsDropDownOpen)
                {
                    // Stelle sicher, dass ein Element ausgewählt ist oder der Text nicht leer ist
                    var selectedItem = DishComboBox.SelectedItem as Dish;
                    if (selectedItem != null || !string.IsNullOrWhiteSpace(DishComboBox.Text))
                    {
                        DishComboBox.Text = selectedItem?.Name ?? DishComboBox.Text;
                        DishComboBox.IsDropDownOpen = false;
                        // Fokus nur dann verschieben, wenn ein Element ausgewählt ist oder der Text nicht leer ist
                        SizeComboBox.Focus();
                    }
                }
                else if (!string.IsNullOrWhiteSpace(DishComboBox.Text))
                {
                    // Fokus verschieben, wenn der Dropdown nicht offen ist, aber Text vorhanden ist
                    SizeComboBox.Focus();
                }

                // Markiere das Ereignis als behandelt, um zu verhindern, dass andere Handler darauf reagieren
                e.Handled = true;
            }
        }

        private void ShowPartyPizzaPopup()
        {
            var popup = new PartyPizza();
            bool? dialogResult = popup.ShowDialog();

            if (dialogResult == true) 
            {
                double averagePricePerPizza = popup.AveragePricePerPizza;
                double totalPrice = popup.SelectedPizzasPrices.Sum();
                int pizzaCount = popup.SelectedPizzaIds.Count;


                if (pizzaCount > 0)
                {
                    string formattedTotalPrice = totalPrice.ToString("F2");
                    string formattedPricePerPizza = averagePricePerPizza.ToString("F2");
                    var sizes = DishSizeManager.CategorySizes[_selectedDish.Kategorie];
                    SizeComboBox.ItemsSource = sizes;
                    tempOrderItem.Gericht = popup.DescriptionOfSelectedPizzas;
                    tempOrderItem.Epreis = averagePricePerPizza;
                    tempOrderItem.Gesamt = averagePricePerPizza;
                    SizeComboBox.SelectedItem = "XL";
                    tempOrderItem.Größe = "XL"; 
       
                }
               
            }
            else
            {
                SizeComboBox.SelectedItem = null;
                DishComboBox.Text = string.Empty;
                DishComboBox.SelectedItem = null;
                

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
                return; // Wenn der Benutzer löscht, tun wir nichts
            }

            string text = textBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                return; // Wenn der Text leer ist, tun wir nichts
            }

            var segments = text.Split(',');
            var lastSegment = segments.Last().Trim();

            // Überprüfe, ob das letzte Segment ein Minuszeichen und mindestens ein weiteres Zeichen enthält
            if (lastSegment.StartsWith("-") && lastSegment.Length <= 1)
            {
                return; // Warte auf weitere Eingaben nach dem Minuszeichen
            }

            string lookupText = lastSegment.StartsWith("-") ? lastSegment.Substring(1).Trim() : lastSegment;

            if (lookupText.Length < 1)
            {
                return; // Kein ausreichender Text für die Autovervollständigung
            }

            var matchingExtras = extrasList
                .Where(extra => extra.Name.StartsWith(lookupText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingExtras.Any())
            {
                var selectedExtra = matchingExtras.First(); // Wähle das erste passende Extra

                string newText = text.Substring(0, text.LastIndexOf(lastSegment));
                newText += lastSegment.StartsWith("-") ? "-" : ""; // Füge das Minuszeichen wieder hinzu, wenn vorhanden
                newText += selectedExtra.Name;

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
                textBox.Text = "OK";
                textBox.SelectAll();
                textBox.Focus();
                textBox.TextChanged += ExtrasTextBox_TextChanged;
                textBox.PreviewKeyDown += ExtrasComboBox_PreviewKeyDown;
            }
        }

        private void ExtrasComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                if (textBox.Text.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    // Der Benutzer hat die Eingabe der Extras abgeschlossen
                    tempOrderItem.Extras = string.Join(", ", selectedExtras);
                    extraShowLabel.Text = tempOrderItem.Extras; // Zeige alle ausgewählten Extras im Label an
                    selectedExtras.Clear(); // Liste leeren für die nächste Bestellung
                    textBox.Text = string.Empty; // Textbox leeren
                    ProcessOrder(); // Weiter mit der Bestellung
                }
                else if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    // Wenn das Textfeld leer ist und Enter gedrückt wird, "OK" anzeigen
                    textBox.Text = "OK";
                    textBox.SelectAll();
                }
                else
                {
                    string searchText = textBox.Text;
                    // Ignoriere das Minuszeichen bei der Suche
                    if (searchText.StartsWith("-"))
                    {
                        searchText = searchText.Substring(1);
                    }

                    var matchingExtra = extrasList.FirstOrDefault(extra => extra.Name.Equals(searchText, StringComparison.OrdinalIgnoreCase));
                    if (matchingExtra != null && !textBox.Text.StartsWith("-"))
                    {
                        selectedExtras.Add(matchingExtra.Name); // Füge das Extra zur Liste hinzu
                        extraShowLabel.Text += string.IsNullOrEmpty(extraShowLabel.Text) ? matchingExtra.Name : ", " + matchingExtra.Name; // Update das TextBlock
                    }
                    else if (textBox.Text.StartsWith("-"))
                    {
                        // Wenn kein passendes Extra gefunden wurde, aber das Minuszeichen vorhanden ist, füge den Text so hinzu, wie er ist
                        selectedExtras.Add(textBox.Text);
                        extraShowLabel.Text += string.IsNullOrEmpty(extraShowLabel.Text) ? textBox.Text : ", " + textBox.Text;
                    }


                    textBox.Text = string.Empty; // Textbox leeren für die nächste Eingabe
                }
                e.Handled = true;
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
                e.Handled = true;
            }
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
                DishComboBox.Focus();
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

        public double GetPriceForSelectedExtraSize(Extra selectedExtra, string selectedSize)
        {
            switch (selectedSize)
            {
                case "S":
                    return selectedExtra.ExtraPreis_S;
                case "L":
                    return selectedExtra.ExtraPreis_L;
                case "XL":
                    return selectedExtra.ExtraPreis_XL;
                default:
                    return 0; // Oder einen Standardpreis, wenn keine Größe übereinstimmt
            }
        }


        private OrderHelper _orderHelper = new OrderHelper();


        private bool IsHappyHour()
        {
            var currentDateTime = DateTime.Now;
            var happyHourStart = Properties.Settings.Default.HappyHourStart;
            var happyHourEnd = Properties.Settings.Default.HappyHourEnd;
            var happyHourStartDay = ConvertGermanDayOfWeek(Properties.Settings.Default.HappyHourStartDay);
            var happyHourEndDay = ConvertGermanDayOfWeek(Properties.Settings.Default.HappyHourEndDay);

            return _orderHelper.IsHappyHour(currentDateTime, happyHourStart, happyHourEnd, happyHourStartDay, happyHourEndDay);
        }


        private DayOfWeek ConvertGermanDayOfWeek(string germanDay)
        {
            switch (germanDay)
            {
                case "Montag":
                    return DayOfWeek.Monday;
                case "Dienstag":
                    return DayOfWeek.Tuesday;
                case "Mittwoch":
                    return DayOfWeek.Wednesday;
                case "Donnerstag":
                    return DayOfWeek.Thursday;
                case "Freitag":
                    return DayOfWeek.Friday;
                case "Samstag":
                    return DayOfWeek.Saturday;
                case "Sonntag":
                    return DayOfWeek.Sunday;
                default:
                    throw new ArgumentException("Ungültiger Tag der Woche");
            }
        }

        private void ProcessOrder()
        {
            UpdateTempOrderItemAmount();
            if (!tempOrderItem.Gericht.StartsWith("Party Pizza") && dishesList.FirstOrDefault(d => d.Name == tempOrderItem.Gericht) == null)
            {
                MessageBox.Show("Bitte gebe ein Gericht an.");
                DishComboBox.Focus();
                return;
            }

            bool isHappyHourNow = IsHappyHour();

            if (tempOrderItem.OrderItemId != 700)
            {
                tempOrderItem.Epreis = 0;
            }

            Dish selectedDish = dishesList.FirstOrDefault(d => d.Name == tempOrderItem.Gericht);
            string selectedSize = SizeComboBox.SelectedItem.ToString();
            tempOrderItem.Größe = selectedSize;

            bool isHappyHour = IsHappyHour();

            if (selectedDish != null)
            {
                // Grundpreis des Gerichts basierend auf der ausgewählten Größe berechnen
                double basePrice = GetPriceForSelectedSize(selectedDish, selectedSize);

                // Überprüfe, ob Mittagsangebot anwendbar ist
                if (isHappyHourNow && _orderHelper.IsEligibleForLunchOffer(selectedDish, selectedSize))
                {
                    basePrice = 9; // Preis für das Mittagsangebot setzen
                }

                if (tempOrderItem.OrderItemId != 700)
                {
                    tempOrderItem.Epreis = basePrice;
                }

                // Verarbeite alle ausgewählten Extras
                if (tempOrderItem.Extras != null)
                {
                    var extras = tempOrderItem.Extras.Split(',').Select(extra => extra.Trim());
                    foreach (var extraName in extras)
                    {
                        bool isRemovingExtra = extraName.StartsWith("-");
                        string cleanExtraName = isRemovingExtra ? extraName.Substring(1) : extraName;

                        var selectedExtra = extrasList.FirstOrDefault(extra => extra.Name.Equals(cleanExtraName, StringComparison.OrdinalIgnoreCase));
                        if (selectedExtra != null)
                        {
                            double extraPrice = GetPriceForSelectedExtraSize(selectedExtra, tempOrderItem.Größe);

                            if (isRemovingExtra)
                            {
                                // Ziehe den Preis für das entfernte Extra ab, aber nicht unter den Grundpreis
                                tempOrderItem.Epreis = Math.Max(tempOrderItem.Epreis - extraPrice, basePrice);
                            }
                            else
                            {
                                // Füge den Preis für das hinzugefügte Extra hinzu
                                tempOrderItem.Epreis += extraPrice;
                            }
                        }
                    }
                }

                // Berechne den Gesamtpreis für das Gericht
                tempOrderItem.Gesamt = tempOrderItem.Epreis * tempOrderItem.Menge;
            }

            // Berechnen Sie den Gesamtpreis eines Gerichtes mit Berücksichtigung der Anzahl
            tempOrderItem.Gesamt = tempOrderItem.Epreis * tempOrderItem.Menge;
            tempOrderItem.Uhrzeit = TimePickermein.Value?.ToString("HH:mm") ?? string.Empty;
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
            extraShowLabel.Text = "";
            DishComboBox.Focus();
        }

       


        private bool IsLunchOfferApplicable(DateTime time)
        {
            var startLunchTime = new TimeSpan(11, 0, 0); // 11:00 AM
            var endLunchTime = new TimeSpan(14, 0, 0);   // 2:00 PM
            bool isWithinTime = time.TimeOfDay >= startLunchTime && time.TimeOfDay <= endLunchTime;
            bool isWithinDay = time.DayOfWeek >= DayOfWeek.Tuesday && time.DayOfWeek <= DayOfWeek.Friday;


            return isWithinTime && isWithinDay;
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
            if (e.Key == Key.Delete)
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
                return;
            }

            string totalPriceString = TotalPriceLabel.Content.ToString();
            double gesamtPreis = 0;

            //Möglichkeit für einen Mindestbestellwert
            //if (double.TryParse(totalPriceString.Split(' ')[0], out gesamtPreis) && gesamtPreis < 10)
            //{
            //    MessageBox.Show("Der Mindestbestellwert wert muss über 10 € liegen.");
            //    return; // Verlasse die Methode frühzeitig
            //}

            if (PhoneNumberTextBox.Text != "")
            {
            
                Receipt receipt = new Receipt
                {
                    ReceiptNumber = GetNextReceiptNumber(),             
                };

                var deliveryUntilStr = TimePickermein.Value.HasValue
                     ? TimePickermein.Value.Value.ToString("HH:mm")
                     : string.Empty; 


                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    BonNumber = ++currentBonNumber, 
                    IsDelivery = isDelivery,
                    OrderItems = orderItems,
                    PaymentMethod = paymentMethod,
                    CustomerPhoneNumber = _customerNr,
                    Timestamp = DateTime.Now.ToString("HH:mm"),
                    DeliveryUntil = deliveryUntilStr
                };


                _databaseManager.UpdateCurrentBonNumber(currentBonNumber);

                _databaseManager.SaveOrder(order);
               
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

                orderItems.Clear();
                PhoneNumberTextBox.Text = string.Empty;
                NameTextBox.Text = string.Empty;
                CustomerStreetTextBox.Text = string.Empty;
                CityTextBox.Text = string.Empty;
                AdditionalInfoTextBox.Text = string.Empty;
                SaveButton.Visibility = Visibility.Collapsed;
                TimePickermein.Value = null;
                TotalPriceLabel.Content = $"0.00 €";

              
                myDataGrid.Items.Refresh();
                PhoneNumberTextBox.Focus();             
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

        private void ShowPasswordDialogAndCheckForF1Grid()
        {
            PasswordInputDialog dialog = new PasswordInputDialog();
            if (dialog.ShowDialog() == true)
            {
                if (IsPasswordCorrect(dialog.Password))
                {
                    // Passwort ist korrekt, F1Grid kann sichtbar gemacht werden
                    F1Grid.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("Das Passwort ist nicht korrekt.");
                    // Optional: F1Grid unsichtbar machen oder andere Aktionen ausführen
                }
            }
        }

        private string DecryptPassword(string encryptedPassword)
        {      
            byte[] data = Convert.FromBase64String(encryptedPassword);
            return Encoding.UTF8.GetString(data);
        }
        private bool IsPasswordCorrect(string inputPassword)
        {
            string encryptedStoredPassword = Properties.Settings.Default.EncryptedPassword;
            string decryptedStoredPassword = DecryptPassword(encryptedStoredPassword);

            return inputPassword == decryptedStoredPassword;
        }

        private void SaveEncryptedPassword(string password)
        {
            string encryptedPassword = EncryptPassword(password);
            Properties.Settings.Default.EncryptedPassword = encryptedPassword;
            Properties.Settings.Default.Save();    
        }

        private void SetDefaultpassword() {
            if (Properties.Settings.Default.EncryptedPassword == null || Properties.Settings.Default.EncryptedPassword == "")
            {
                string standardPassword = EncryptPassword("ecki");
                Properties.Settings.Default.EncryptedPassword = standardPassword;
                Properties.Settings.Default.Save();

            }
        }


        private string EncryptPassword(string password)
        {        
            byte[] data = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(data);
        }
        private bool ShowPasswordDialogAndCheck()
        {
            PasswordInputDialog dialog = new PasswordInputDialog();
            if (dialog.ShowDialog() == true)
            {
                if (IsPasswordCorrect(dialog.Password))
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("Das Passwort ist nicht korrekt.");
                }
            }
            return false;
        }
        private async void MainWindowEcki_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2 && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                if (F1Grid.Visibility == Visibility.Collapsed)
                {
                    ShowPasswordDialogAndCheckForF1Grid();
                }
                else
                {
                    F1Grid.Visibility = Visibility.Collapsed;
                }
                e.Handled = true;
                return;
            }

            if (e.Key == Key.F1 && Keyboard.Modifiers == ModifierKeys.None)
            {
                ShowHelpDialog();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.F2 && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (SaveButton.Visibility == Visibility.Visible)
                {
                    OnSaveButtonClicked(this, new RoutedEventArgs());
                    e.Handled = true;
                }
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F3)
            {
                var passwordCorrect = ShowPasswordDialogAndCheck();
                if (passwordCorrect)
                {
                    var deletedOrders = await _databaseManager.GetDeletedOrdersWithItems();
                    var deletedOrdersWindow = new GelöschteBestellungen(deletedOrders);
                    deletedOrdersWindow.ShowDialog();
                    e.Handled = true;
                    return;
                }
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F4)
            {
                var passwordCorrect = ShowPasswordDialogAndCheck();
                if (passwordCorrect)
                {
                    DailyEarnings dailyEarnings = new DailyEarnings();
                    dailyEarnings.ShowDialog();
                }
            }

            if (e.Key == Key.F5)
            {
                var selectedItem = myDataGrid.SelectedItem as OrderItem;
                if (selectedItem != null)
                {
                    selectedItem.Epreis = 0.00;
                    selectedItem.Gesamt = 0.00;
                    myDataGrid.Items.Refresh();
                    CalculateTotal(orderItems);
                }
            }

            if (e.Key == Key.F7)
            {
                if (orderItems.Any())
                {
                    orderItems.RemoveAt(orderItems.Count - 1);
                    myDataGrid.ItemsSource = null;
                    myDataGrid.ItemsSource = orderItems;
                    CalculateTotal(orderItems);
                }
            }

            if (e.Key == Key.F11)
            {
                BestellungenAnzeigen("alle");
            }
            if (e.Key == Key.F12)
            {
                BarzahlungBtn(null, null);
            }
            if (e.Key == Key.F8)
            {
                secretPrintTriggered = true;
                BarzahlungBtn(null, null);
            }
            if (e.Key == Key.Escape)
            {
                ClearOrder();
            }
        }

        private void OpenPaymentPopup()
        {
            BezahlPopup popup = new BezahlPopup();
            if (popup.ShowDialog() == true)
            {
                string paymentMethod = popup.SelectedPaymentMethod;
                CompleteOrder(paymentMethod);
            }
        }
        private void ShowHelpDialog()
        {
            string helpText = "F1: Zeige Hotkeys.\n" +
                              "F2: Kunde speichern.\n" +                 
                              "F5: Das markierte Gericht wird gratis.\n" +
                              "F7: Letztes Gericht löschen.\n" +
                              "F8: Küchen Druck.\n" +
                              "F11: Alle Bestellungen anzeigen.\n" +
                              "F12: Bestellung abschließen und drucken.";

            MessageBox.Show(helpText, "Hilfe zu Tastenkürzeln");
        }
        private void SendOrderItems(Order order)
        {
            //signalRService.SendOrderItemsAsync(order);
        }
        private void EinstellungenBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Closed += SettingsWindow_Closed;
            settingsWindow.ShowDialog();
        }
        private void LoadDrivers()
        {
            DatabaseManager dbManager = new DatabaseManager();
            List<Driver> driversFromDb = dbManager.GetAllDrivers();

            // Filtere nur 'Theke' und 'Kasse1'
            var driversForCounter = driversFromDb.Where(d => d.Name == "Theke" || d.Name == "Kasse").ToList();

            cb_cashRegister.Items.Clear();
            cb_cashRegister.ItemsSource = driversForCounter;
        }
        private void SizeComboBox_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                // Ermitteln der Ziffer als Zeichen
                char keyChar = (char)((int)'0' + (e.Key - Key.D0) % 10);

                // Setzen der Eingabe in die amountComboBox
                amountComboBox.Text = keyChar.ToString();

                // Optional: Fokus zur amountComboBox setzen, um weitere Eingaben dort zu ermöglichen
                amountComboBox.Focus();

                // Verhindern, dass die Eingabe weiterverarbeitet wird (z.B. in der SizeComboBox)
                e.Handled = true;
            }

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

            string defaultPrinter = new PrinterSettings().PrinterName;

            // Entscheiden, welcher Drucker basierend auf der Tastenkombination verwendet werden soll
            string selectedPrinter = secretPrintTriggered
                                     ? Properties.Settings.Default.NetworkPrinter
                                     : Properties.Settings.Default.SelectedPrinter;

            // Überprüfen, ob der Netzwerkdruckername leer ist
            if (secretPrintTriggered && string.IsNullOrEmpty(selectedPrinter))
            {
                MessageBox.Show("Netzwerkdrucker nicht konfiguriert. Verwende Standarddrucker.");
                selectedPrinter = defaultPrinter;
            }
            else
            {
                printDoc.PrinterSettings.PrinterName = selectedPrinter;
            }
            secretPrintTriggered = false;

            // Überprüfe, ob der angegebene Drucker vorhanden und verfügbar ist
            if (!printDoc.PrinterSettings.IsValid)
            {
                printDoc.PrinterSettings.PrinterName = selectedPrinter;
            }
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 300, 10000);
            printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            printDoc.PrintPage += (sender, e) =>
            {
                Graphics graphics = e.Graphics;
                float yOffset = 10; 
                // Fonts
                Font smallFont = new Font("Segoe UI", 10, System.Drawing.FontStyle.Regular);
                Font regularFont = new Font("Segoe UI Semibold", 13, System.Drawing.FontStyle.Regular);
                Font boldFont = new Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
                Font titleFont = new Font("Segoe UI", 24, System.Drawing.FontStyle.Bold);

              

                // Title
                string title = "PIZZA ECKI";
                float titleWidth = graphics.MeasureString(title, titleFont).Width;
                graphics.DrawString(title, titleFont, Brushes.Black, (e.PageBounds.Width - titleWidth) / 2, yOffset);
                yOffset += titleFont.GetHeight();

                // Adresse
                SizeF addressSize = graphics.MeasureString("Woerdener Str. 4 · 33803 Steinhagen", smallFont);
                float addressX = (e.PageBounds.Width - addressSize.Width) / 2;
                graphics.DrawString("Woerdener Str. 4 · 33803 Steinhagen", smallFont, Brushes.Black, addressX, yOffset);
                yOffset += addressSize.Height;  // 10 pixels Abstand

                // Datum und Uhrzeit
                string dateStr = DateTime.Now.ToString("dd.MM.yyyy");
                string timeStr = DateTime.Now.ToString("HH:mm");
                graphics.DrawString(dateStr, regularFont, Brushes.Black, 0, yOffset);
                SizeF timeSize = graphics.MeasureString(timeStr, regularFont);
                graphics.DrawString(timeStr, regularFont, Brushes.Black, e.PageBounds.Width - timeSize.Width - 20, yOffset);
                // 10 pixels Abstand

                Pen blackPen = new Pen(Color.Black, 1);

                // Zeichne eine Trennlinie nach der Überschrift
                graphics.DrawLine(blackPen, 0, yOffset, e.PageBounds.Width, yOffset);
                yOffset += regularFont.GetHeight() + 10;


                if (isDelivery && customer != null)  // Überprüfen, ob es sich um eine Lieferung handelt
                {
                    string addressStr = "Tel: " + customer.PhoneNumber + "\r\n" + customer.Name + "\r\n" + customer.Street + "\r\n" + customer.City + "\r\n" + customer.AdditionalInfo;
                    graphics.DrawString(addressStr, boldFont, Brushes.Black, 0, yOffset);

                    // Hier ändern wir den yOffset, um zwei Zeilenhöhen hinzuzufügen, eine für jede Zeile der Adresse.
                    yOffset += boldFont.GetHeight() * 6;  // Anpassen für den Zeilenumbruch in der Adresse
                }

                string bonNumberStr = $"Bon Nummer: {order.BonNumber}";
                SizeF bonNumberSize = graphics.MeasureString(bonNumberStr, boldFont);
                float bonNumberX = (e.PageBounds.Width - bonNumberSize.Width) / 2;
                graphics.DrawString(bonNumberStr, boldFont, Brushes.Black, bonNumberX, yOffset);
                yOffset += bonNumberSize.Height + 5;

                if (!string.IsNullOrEmpty(_pickupType))
                {
                    string pickupTypeStr = _pickupType; // "Selbstabholer" oder "Mitnehmer"
                    graphics.DrawString(pickupTypeStr, boldFont, Brushes.Black, 0, yOffset);
                    yOffset += boldFont.GetHeight() + 10;
                }

                var deliveryUntilStr = TimePickermein.Value.HasValue
                    ? TimePickermein.Value.Value.ToString("HH:mm")
                    : string.Empty; // Leer, wenn kein Wert vorhanden

                if (isDelivery && !string.IsNullOrEmpty(deliveryUntilStr))
                {
                    string deliveryTimeStr = "Lieferung bis: " + deliveryUntilStr;
                    graphics.DrawString(deliveryTimeStr, boldFont, Brushes.Black, 0, yOffset);
                    yOffset += regularFont.GetHeight() + 5;
                }
                else if (!string.IsNullOrEmpty(deliveryUntilStr))
                {
                    string deliveryTimeStr = "Abholung bis: " + deliveryUntilStr;
                    graphics.DrawString(deliveryTimeStr, boldFont, Brushes.Black, 0, yOffset);
                    yOffset += regularFont.GetHeight() + 5;
                }


                graphics.DrawLine(blackPen, 0, yOffset, e.PageBounds.Width, yOffset);
                yOffset += 5;


                // Tabellenüberschrift
                string headerAnz = "Anz  ";
                string headerNr = "Nr  ";
                string headerGericht = "Gericht  ";
                string headerGr = " Gr.  ";
                string headerPreis = "Preis";

                // Definiere die Positionen der Spaltenköpfe
                float headerAnzPosition = 0;
                float headerNrPosition = 37;  // 
                float headerGerichtPosition = 65;  // Angenommene Position, anpassen nach Bedarf
                float headerGrPosition = 200;  // Angenommene Position, anpassen nach Bedarf
                                               // Preis rechtsbündig, Abstand vom rechten Rand
                float headerPreisPosition = e.MarginBounds.Right - graphics.MeasureString(headerPreis, regularFont).Width - 15;

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
                Font extraFont = new Font("Segoe UI Semibold", 10);
                foreach (var item in order.OrderItems)
                {
                    string itemNameStr = $" {item.Menge} x  {item.OrderItemId}  {item.Gericht}";
                    string itemSizeStr = $" {item.Größe}";
                    string itemPriceStr = $"{item.Gesamt:C}";

                    // Zeichne die Bestellzeile
                    graphics.DrawString(itemNameStr, regularFont, Brushes.Black, headerAnzPosition, yOffset);
                    graphics.DrawString(itemSizeStr, regularFont, Brushes.Black, headerGrPosition, yOffset);
                    SizeF itemPriceSize = graphics.MeasureString(itemPriceStr, regularFont);
                    graphics.DrawString(itemPriceStr, regularFont, Brushes.Black, e.PageBounds.Width - itemPriceSize.Width, yOffset);

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
                        graphics.DrawString(extrasStr, extraFont, Brushes.Black, headerGerichtPosition - 8, yOffset);

                        // Aktualisiere yOffset für die nächste Zeile
                        yOffset += extraFont.GetHeight();
                    }

                    yOffset += 10; // Füge einen kleinen Abstand nach der Linie hinzu
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


        private void PrintToNetworkPrinter(Order order, Customer customer)
        {
            // Erstellen des PrintDocument-Objekts
            PrintDocument printDoc = new PrintDocument();
            // Setzen des Druckers auf den Netzwerkdrucker
            string networkPrinter = PizzaEcki.Properties.Settings.Default.NetworkPrinter;
            if (!string.IsNullOrEmpty(networkPrinter))
            {
                printDoc.PrinterSettings.PrinterName = networkPrinter;
            }
            // Überprüfen, ob der Drucker verfügbar ist
            if (!printDoc.PrinterSettings.IsValid)
            {
                MessageBox.Show("Der Netzwerkdrucker ist nicht verfügbar.");
                return;
            }
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 300, 10000);
            printDoc.PrintPage += (sender, e) =>
            {
                Graphics graphics = e.Graphics;
                // Fonts
                Font regularFont = new Font("Segoe UI", 16, System.Drawing.FontStyle.Bold);
                Font boldFont = new Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);
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
                if (isDelivery && customer != null)  // Überprüfen, ob es sich um eine Lieferung handelt
                {
                    string addressStr = "Tel: " + customer.PhoneNumber + "\r\n" + customer.Name + "\r\n" + customer.Street + "\r\n" + customer.City + "\r\n" + customer.AdditionalInfo;
                    graphics.DrawString(addressStr, boldFont, Brushes.Black, 0, yOffset);
                    // Hier ändern wir den yOffset, um zwei Zeilenhöhen hinzuzufügen, eine für jede Zeile der Adresse.
                    yOffset += boldFont.GetHeight() * 6;  // Anpassen für den Zeilenumbruch in der Adresse
                }
                string bonNumberStr = $"Bon Nummer: {order.BonNumber}";
                SizeF bonNumberSize = graphics.MeasureString(bonNumberStr, boldFont);
                float bonNumberX = (e.PageBounds.Width - bonNumberSize.Width) / 2;
                graphics.DrawString(bonNumberStr, boldFont, Brushes.Black, bonNumberX, yOffset);
                yOffset += bonNumberSize.Height + 5;
                if (!string.IsNullOrEmpty(_pickupType))
                {
                    string pickupTypeStr = _pickupType; // "Selbstabholer" oder "Mitnehmer"
                    graphics.DrawString(pickupTypeStr, boldFont, Brushes.Black, 0, yOffset);
                    yOffset += boldFont.GetHeight() + 10;
                }
                var deliveryUntilStr = TimePickermein.Value.HasValue
                    ? TimePickermein.Value.Value.ToString("HH:mm")
                    : string.Empty; // Leer, wenn kein Wert vorhanden
                if (isDelivery && !string.IsNullOrEmpty(deliveryUntilStr))
                {
                    string deliveryTimeStr = "Lieferung bis: " + deliveryUntilStr;
                    graphics.DrawString(deliveryTimeStr, boldFont, Brushes.Black, 0, yOffset);
                    yOffset += regularFont.GetHeight() + 5;
                }
                else if (!string.IsNullOrEmpty(deliveryUntilStr))
                {
                    string deliveryTimeStr = "Abholung bis: " + deliveryUntilStr;
                    graphics.DrawString(deliveryTimeStr, boldFont, Brushes.Black, 0, yOffset);
                    yOffset += regularFont.GetHeight() + 5;
                }
                graphics.DrawLine(blackPen, 0, yOffset, e.PageBounds.Width, yOffset);
                yOffset += 5;
                // Tabellenüberschrift
                string headerAnz = "Anz  ";
                string headerNr = "Nr  ";
                string headerGericht = "Gericht  ";
                string headerGr = "Gr.  ";
                string headerPreis = "Preis";
                // Definiere die Positionen der Spaltenköpfe
                float headerAnzPosition = 0;
                float headerNrPosition = 45;  // 
                float headerGerichtPosition = 80;  // Angenommene Position, anpassen nach Bedarf
                float headerGrPosition = 15;  // Angenommene Position, anpassen nach Bedarf
                                              // Preis rechtsbündig, Abstand vom rechten Rand
                float headerPreisPosition = e.PageBounds.Width - graphics.MeasureString(headerPreis, regularFont).Width;
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
                    string itemSizeStr = $"{item.Größe} h";
                    string itemPriceStr = $"{item.Gesamt:C}";
                    // Zeichne die Bestellzeile
                    graphics.DrawString(itemNameStr, regularFont, Brushes.Black, headerAnzPosition, yOffset);
                    graphics.DrawString(itemSizeStr, regularFont, Brushes.Black, headerGrPosition, yOffset);
                    SizeF itemPriceSize = graphics.MeasureString(itemPriceStr, regularFont);
                    graphics.DrawString(itemPriceStr, regularFont, Brushes.Black, e.PageBounds.Width - itemPriceSize.Width, yOffset);
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
                    yOffset += 10; // Füge einen kleinen Abstand nach der Linie hinzu
                }
                // Zeichne eine abschließende Trennlinie am Ende der
                // Zeichne eine abschließende Trennlinie am Ende der Bestellliste
                graphics.DrawLine(blackPen, 0, yOffset, e.PageBounds.Width, yOffset);
                yOffset += 10;  // Füge einen größeren Abstand nach der Linie hinzu, um Platz für den Gesamtpreis zu schaffen
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
        private async void Btn_zuordnen_Click(object sender, RoutedEventArgs e)
        {
            if (cb_bonNummer.SelectedItem != null && cb_cashRegister.SelectedItem != null)
            {
                int selectedBonNumber = (int)cb_bonNummer.SelectedItem;
                Order selectedOrder = orders.FirstOrDefault(o => o.BonNumber == selectedBonNumber);
                Driver selectedDriver = (Driver)cb_cashRegister.SelectedItem;

                if (selectedOrder != null && selectedDriver != null)
                {
                    double orderPrice = selectedOrder.OrderItems.Sum(item => item.Gesamt);
                    _databaseManager.SaveOrderAssignmentAsync(selectedOrder.OrderId.ToString(), selectedDriver.Id, orderPrice);

                    cb_bonNummer.SelectedItem = null;
                    cb_cashRegister.SelectedItem = null;

                    await ReloadeUnassignedOrders();
                }
                else
                {
                    MessageBox.Show("Es wurde keine Bestellung oder kein Fahrer gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool HaveBonNumbersChanged(List<Order> oldOrders, List<Order> newOrders)
        {
            if (oldOrders == null || newOrders == null)
            {
                return true; // Gibt true zurück, wenn eine der Listen null ist
            }

            var oldBonNumbers = new HashSet<int>(oldOrders.Select(o => o.BonNumber));
            var newBonNumbers = new HashSet<int>(newOrders.Select(o => o.BonNumber));

            return !oldBonNumbers.SetEquals(newBonNumbers);
        }
        private async Task ReloadeUnassignedOrders()
        {
            var newOrders = _databaseManager.GetUnassignedOrders();

            if (HaveBonNumbersChanged(orders, newOrders))
            {
                var selectedValue = cb_bonNummer.SelectedItem;

                orders = newOrders;
                cb_bonNummer.Items.Clear();
                foreach (Order order in orders)
                {
                    if (order.IsDelivery == false)
                    {
                        cb_bonNummer.Items.Add(order.BonNumber);
                    }
                }

                cb_bonNummer.SelectedItem = selectedValue;
            }

            UpdateOrderCounts(); // Wird unabhängig von der Bedingung aufgerufen
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
        private async void BestellungenAnzeigen(string bestellungsTyp)
        {
            var unassignedOrders = _databaseManager.GetUnassignedOrders();
            List<Order> filteredOrders;

            if (bestellungsTyp == "1")
            {
                // Filtere Bestellungen für Selbstabholer
                filteredOrders = unassignedOrders
                    .Where(order => order.CustomerPhoneNumber == bestellungsTyp)
                    .ToList();
            }
            else if (bestellungsTyp == "2")
            {
                // Filtere Bestellungen für Mitnehmer
                filteredOrders = unassignedOrders
                    .Where(order => order.CustomerPhoneNumber == bestellungsTyp)
                    .ToList();
            }
            else if (bestellungsTyp == "alle")
            {
                var ordersWithAssignedDrivers = await _databaseManager.GetOrdersWithAssignedDrivers();
                // Zeige alle Bestellungen ohne Filterung
                filteredOrders = ordersWithAssignedDrivers;
            }
            else
            {
                // Standard: Filtere Bestellungen für Auslieferung
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
                    case "alle":
                        _bestellungenFenster.Title = "Alle Bestellungen";
                        break;
                }
                _currentBestellungsTyp = bestellungsTyp; // Speichere den aktuellen Bestellungstyp
                _bestellungenFenster.Closed += (s, e) =>
                {
                    _bestellungenFenster = null;
                    _currentBestellungsTyp = null; // Setze den Bestellungstyp zurück, wenn das Fenster geschlossen wird
                };
                _bestellungenFenster.ShowDialog();
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

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                textBlock.TextDecorations = TextDecorations.Underline;
            }
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                textBlock.TextDecorations = null;
            }
        }

        private async void Bestellungen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ordersWithAssignedDrivers = await  _databaseManager.GetOrdersWithAssignedDrivers();
                var allDrivers = await _databaseManager.GetAllDriversAsync();
                var updatedOrders = new List<Order>();

                foreach (var order in ordersWithAssignedDrivers)
                {
                    if (order.DriverId.HasValue && order.DriverId.Value != -1)
                    {
                        var driver = allDrivers.FirstOrDefault(d => d.Id == order.DriverId.Value);
                        order.Name = driver?.Name ?? "Nicht zugewiesen";  // Wenn kein passender Fahrer gefunden wird, "Nicht zugewiesen" verwenden
                    }
                    else
                    {
                        order.Name = "Nicht zugewiesen";
                    }

                    updatedOrders.Add(order);
                }

                Bestellungen bestellungenFenster = new Bestellungen(updatedOrders, true);
                bestellungenFenster.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Bestellungen und Fahrer: {ex.Message}");
            }
        }

        private async void Uebersicht_Click(object sender, RoutedEventArgs e)
        {

            BestellungenAnzeigen("alle");
        }

        private void MainWindowEcki_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window != this)
                {
                    window.Close();
                }
            }

        }

        private void SizeComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null)
            {
                return;
            }

            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                int numberPressed = e.Key - Key.NumPad0;
                amountComboBox.Text = numberPressed.ToString();
                e.Handled = true;
            }
            else if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                int numberPressed = e.Key - Key.D0;
                amountComboBox.Text = numberPressed.ToString();
                e.Handled = true;
            }
        }

        private void ClearOrder()
        {
            orderItems.Clear();
            PhoneNumberTextBox.Text = string.Empty;
            NameTextBox.Text = string.Empty;
            CustomerStreetTextBox.Text = string.Empty;
            CityTextBox.Text = string.Empty;
            AdditionalInfoTextBox.Text = string.Empty;
            SaveButton.Visibility = Visibility.Collapsed;
            TimePickermein.Value = null;
            TotalPriceLabel.Content = $"0.00 €";
            DishComboBox.Text = string.Empty;
            ExtrasComboBox.Text = string.Empty;
            selectedExtras.Clear();
            extraShowLabel.Text = string.Empty;

            myDataGrid.Items.Refresh();
            PhoneNumberTextBox.Focus();
        }
        private void Zuordnen_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    var ordersWithAssignedDrivers = await _databaseManager.GetOrdersWithAssignedDrivers();
            //    var allDrivers = await _databaseManager.GetAllDriversAsync();
            //    var updatedOrders = new List<Order>();

            //    foreach (var order in ordersWithAssignedDrivers)
            //    {
            //        if (order.DriverId.HasValue && order.DriverId.Value != -1)
            //        {
            //            var driver = allDrivers.FirstOrDefault(d => d.Id == order.DriverId.Value);
            //            order.Name = driver?.Name ?? "Nicht zugewiesen";  // Wenn kein passender Fahrer gefunden wird, "Nicht zugewiesen" verwenden
            //        }
            //        else
            //        {
            //            order.Name = "Nicht zugewiesen";
            //        }

            //        updatedOrders.Add(order);
            //    }

            //    Bestellungen bestellungenFenster = new Bestellungen(updatedOrders, true);
            //    bestellungenFenster.ShowDialog();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Fehler beim Laden der Bestellungen und Fahrer: {ex.Message}");
            //}
        }

        private void ZuordnenBtn_Click(object sender, RoutedEventArgs e)
        {

        }


        private void cb_cashRegister_Loaded(object sender, RoutedEventArgs e)
        {            ComboBox comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }

        private void CustomerStreetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string suchText = CustomerStreetTextBox.Text;

            if (string.IsNullOrEmpty(suchText))
            {
                SuggestionsListBox.Visibility = Visibility.Collapsed;
                return;
            }

            var vorschläge = _databaseManager.GetStreetSuggestions(suchText);

            if (vorschläge.Any())
            {
                SuggestionsListBox.ItemsSource = vorschläge;
                SuggestionsListBox.Visibility = Visibility.Visible;

                // Positioniere die ListBox unter der TextBox
                System.Windows.Point relativePoint = CustomerStreetTextBox.TransformToAncestor(this)
                                          .Transform(new System.Windows.Point(0, 0));
                Canvas.SetLeft(SuggestionsListBox, relativePoint.X);
                Canvas.SetTop(SuggestionsListBox, relativePoint.Y + CustomerStreetTextBox.ActualHeight - 20);
            }
            else
            {
                SuggestionsListBox.Visibility = Visibility.Collapsed;
            }
        }



        private void SuggestionsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SuggestionsListBox.SelectedItem != null)
                {
                    var selectedSuggestion = (StreetSuggestion)SuggestionsListBox.SelectedItem;
                    CustomerStreetTextBox.Text = selectedSuggestion.Street;
                    CityTextBox.Text = selectedSuggestion.City; // Setze die Stadt in die TextBox für den Ort
                    SuggestionsListBox.Visibility = Visibility.Collapsed;
                    CustomerStreetTextBox.CaretIndex = CustomerStreetTextBox.Text.Length; // Setze den Cursor ans Ende der TextBox
                    CustomerStreetTextBox.Focus();
                }
            }
            else if (e.Key == Key.Escape)
            {
                SuggestionsListBox.Visibility = Visibility.Collapsed;
                CustomerStreetTextBox.Focus();
            }
            else if (e.Key == Key.Up && SuggestionsListBox.SelectedIndex == 0)
            {
                e.Handled = true; // Verhindert das Standardverhalten der ListBox
                CustomerStreetTextBox.Focus();
            }
        }

        private void CustomerStreetTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Down && SuggestionsListBox.Visibility == Visibility.Visible)
                {
                    e.Handled = true; // Verhindert das Standardverhalten der TextBox
                    SuggestionsListBox.Focus();
                    SuggestionsListBox.SelectedIndex = 0;
                }
            }
        
    }
}
