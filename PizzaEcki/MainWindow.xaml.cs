using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PizzaEcki.Database;
using System.Linq;
using PizzaEcki.Models;
using PizzaEcki.Extensions;
using Microsoft.AspNetCore.SignalR;


namespace PizzaEcki
{

    public partial class MainWindow : Window
    {
        private DatabaseManager _databaseManager;
        private List<Dish> dishesList;
        private List<Extra> extrasList;
        private List<OrderItem> orderItems = new List<OrderItem>();
        private OrderItem tempOrderItem = new OrderItem();     
        private int currentReceiptNumber = 0; // das kann auch aus einer Datenbank oder einer Datei gelesen werden
        private int Lieferung = 0;
        private int Selbstabholer = 0;
        private int Mitnehmer = 0;
        private List<OrderItem> currentOrderItems; // das sollte mit den aktuellen Bestellartikeln gefüllt werden


        public MainWindow()
        {
            InitializeComponent();
         

            _databaseManager = new DatabaseManager();
            TimePicker.Value = DateTime.Now;
            TimePicker.Value = DateTime.Now.AddMinutes(15);

            TimePicker.TimeInterval = new TimeSpan(0, 30, 0);

            // Füllen die ComboBox für Gerichte aus der Datenbank
            dishesList = _databaseManager.GetDishes();
            DishComboBox.ItemsSource = dishesList;

            // Füllen die ComboBox für Extras aus der Datenbank
            extrasList = _databaseManager.GetExtras();
           extras.ItemsSource = extrasList;
                  
            currentOrderItems = new List<OrderItem>();

        }


        //Auto Complete Client Data
        private void OnEnterKeyPressed(object sender, KeyEventArgs e)
        {

        }

        private void PhoneNumberTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.Key == Key.Return)
            {
                string _customerNr = PhoneNumberTextBox.Text;

                if (_customerNr != null && _customerNr != "" )
                {
                    if(_customerNr == "1")
                    {
                        Selbstabholer++;
                        DishComboBox.Focus();
                    }
                    else if(_customerNr == "2") {

                        Mitnehmer++;
                        DishComboBox.Focus();
                    }
                    else
                    {
                        Customer customer = _databaseManager.GetCustomerByPhoneNumber(_customerNr);
                        if(customer != null)
                        {
                            SetCustomerDataToFields(customer);
                            Lieferung++;
                        }
                        else
                        {
                            MessageBoxResult result = MessageBox.Show("Es ist noch kein Kunde mit der Nummer bekann.", " Wollen sie einen Anelgen?",
                             MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                            // Wenn der Benutzer Nein wählt, verlasse die Methode
                            if (result == MessageBoxResult.No)
                            {
                                return;

                            }
                            else if (result == MessageBoxResult.Yes) // Wenn der Benutzer Ja wählt, zeige den Button "SaveButton" an
                            {
                                SaveButton.Visibility = Visibility.Visible;
                                NameTextBox.Focus();
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
                // Hier setzen wir die Werte der anderen Felder zurück
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
            // Wenn kein Gericht ausgewählt ist, tue nichts
            if (DishComboBox.SelectedItem == null)
            {
                SizeComboBox.ItemsSource = null; // Leere die SizeComboBox
                return;
            }

            // Ansonsten aktualisiere das tempOrderItem mit dem ausgewählten Gericht
            Dish selectedDish = (Dish)DishComboBox.SelectedItem;
            tempOrderItem.Gericht = selectedDish.Name.ToString();
            tempOrderItem.Nr = selectedDish.Id;
            tempOrderItem.Epreis = selectedDish.Price;

            // Holen Sie sich die Größen für die Kategorie des ausgewählten Gerichts
            var sizes = DishSizeManager.CategorySizes[selectedDish.Category];

            // Füllen Sie die SizeComboBox mit den verfügbaren Größen
            SizeComboBox.ItemsSource = sizes;
            SizeComboBox.SelectedItem = "L";

            // Optional: Wenn es nur eine Größe gibt, wählen Sie sie automatisch aus
            if (sizes.Count == 1)
            {
                SizeComboBox.SelectedIndex = 0;
            }

            // Leere die ausgewählten Extras, da sich das Gericht geändert hat
            tempOrderItem.Extras = "";
        }


        private void DishComboBox_AutocompleteKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Check if the dropdown of the combobox is opened
                if (DishComboBox.IsDropDownOpen)
                {
                    // Change the text of the combobox to the currently highlighted item
                    DishComboBox.Text = (DishComboBox.SelectedItem as Dish)?.Name ?? DishComboBox.Text;
                    DishComboBox.IsDropDownOpen = false;
                }

                // Move the focus to the next control in the tab order
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }

                e.Handled = true;
            }
        }

        private void DishComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Überprüfen, ob ein Gericht ausgewählt wurde
            if (DishComboBox.SelectedItem is Dish selectedDish)
            {
                // Holen Sie sich die Größen für die Kategorie des ausgewählten Gerichts
                var sizes = DishSizeManager.CategorySizes[selectedDish.Category];

                // Füllen Sie die SizeComboBox mit den verfügbaren Größen
                SizeComboBox.ItemsSource = sizes;

                // Optional: Wenn es nur eine Größe gibt, wählen Sie sie automatisch aus
                if (sizes.Count == 1)
                {
                    SizeComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                // Kein Gericht ausgewählt, leeren Sie die SizeComboBox
                SizeComboBox.ItemsSource = null;
            }
        }

        //Extras
        private void Extras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (extras.SelectedItem == null)
            {
                return;
            }

            Extra selectedExtra = (Extra)extras.SelectedItem;

            // Leere die Extras und den zusätzlichen Preis
            tempOrderItem.Extras = "";
            tempOrderItem.Extras = selectedExtra.Name.ToString();
            tempOrderItem.Epreis += selectedExtra.Price;
        }

        private void ExtraCombobox_AutocompleteKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Check if the dropdown of the combobox is opened
                if (extras.IsDropDownOpen)
                {
                    // Change the text of the combobox to the currently highlighted item
                    extras.Text = (extras.SelectedItem as Extra)?.Name ?? extras.Text;
                    extras.IsDropDownOpen = false;
                }

                // Move the focus to the next control in the tab order
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }

                e.Handled = true;
            }
        }


        private void amountComboBox_KeyDown(object sender, KeyEventArgs e)
        {
           
                if (e.Key == Key.Enter)
                {
                    // Überprüfen Sie, ob alle erforderlichen Felder ausgefüllt sind
                    if (string.IsNullOrEmpty(tempOrderItem.Gericht) ||
                        tempOrderItem.Menge == 0)
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

                    // Berechnen Sie den Epreis basierend auf der Menge und dem Preis des Gerichts und der Extras
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
                        Extra selectedExtra = extrasList.FirstOrDefault(x => x.Name == tempOrderItem.Extras);
                        if (selectedExtra != null)
                        {
                            tempOrderItem.Epreis += selectedExtra.Price;
                        }
                    }
                    tempOrderItem.Gesamt = tempOrderItem.Epreis * tempOrderItem.Menge;

                    // Fügen Sie das tempOrderItem zur Liste hinzu
                    tempOrderItem.Nr = orderItems.Count + 1; // Oder jede andere Art, wie Sie die Nr generieren
                    orderItems.Add(tempOrderItem);
                    myDataGrid.ItemsSource = null;
                    myDataGrid.ItemsSource = orderItems;

                   

                CalculateTotal(orderItems);

                // Setzen Sie tempOrderItem zurück, um bereit für die nächste Eingabe zu sein
                tempOrderItem = new OrderItem();

                    // Löschen Sie die Auswahlen und den Text in den ComboBoxen und dem TextBox
                    DishComboBox.SelectedItem = null;
                    extras.SelectedItem = null;
                    amountComboBox.SelectedItem = null;
                }
            
        }

        private void amountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)amountComboBox.SelectedItem;
            if (selectedItem != null)
            {
                tempOrderItem.Menge = int.Parse(selectedItem.Content.ToString());
            }
        }


        private void TimePicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Überprüfen Sie, ob alle erforderlichen Felder ausgefüllt sind
                if (string.IsNullOrEmpty(tempOrderItem.Gericht) ||
                    tempOrderItem.Menge == 0)
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

                // Berechnen Sie den Epreis basierend auf der Menge und dem Preis des Gerichts und der Extras
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
                    Extra selectedExtra = extrasList.FirstOrDefault(x => x.Name == tempOrderItem.Extras);
                    if (selectedExtra != null)
                    {
                        tempOrderItem.Epreis += selectedExtra.Price;
                    }
                }
                tempOrderItem.Gesamt = tempOrderItem.Epreis * tempOrderItem.Menge;

                // Fügen Sie das tempOrderItem zur Liste hinzu
                tempOrderItem.Nr = orderItems.Count + 1; // Oder jede andere Art, wie Sie die Nr generieren
                orderItems.Add(tempOrderItem);
                myDataGrid.ItemsSource = null;
                myDataGrid.ItemsSource = orderItems;

                CalculateTotal(orderItems);

                // Setzen Sie tempOrderItem zurück, um bereit für die nächste Eingabe zu sein
                tempOrderItem = new OrderItem();

                // Löschen Sie die Auswahlen und den Text in den ComboBoxen und dem TextBox
                DishComboBox.SelectedItem = null;
                extras.SelectedItem = null;
                amountComboBox.SelectedItem = null;
            }
        }

        public void CalculateTotal(List<OrderItem> orderItem)
        {
            double gesamtPreis = 0;
            foreach (var item in orderItems)
            {
                gesamtPreis += item.Gesamt;
            }
            TotalPriceButton.Content = $"{gesamtPreis:F2} €";
        }

        private void FinishOrderBtn(object sender, RoutedEventArgs e)
        {
            // Erstelle ein neues Receipt Objekt und fülle es mit den OrderItems und der ReceiptNumber
            Receipt receipt = new Receipt
            {
                ReceiptNumber = GetNextReceiptNumber(),
                OrderItems = GetCurrentOrderItems(),
            };


            
           // Erstelle ein neues ReceiptPrinter Objekt und drucke das Receipt
        


            //Aktualisiere Lieferungsart
            AuslieferungLabel.Content = Lieferung;
            MitnehmerLabel.Content = Mitnehmer;
            SelbstabholerLabel.Content = Selbstabholer;

            //Disabled till its ready
            // printer.Print(receipt);
        }

        private int GetNextReceiptNumber()
        {
            return ++currentReceiptNumber;
        }

        private List<OrderItem> GetCurrentOrderItems()
        {
            // Hier sollten Sie die aktuellen Bestellartikel zurückgeben.
            // Im Moment gebe ich nur die Liste zurück, die im Konstruktor erstellt wurde.
            return currentOrderItems;
        }

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
        }



    }
}
