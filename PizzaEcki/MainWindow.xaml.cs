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

namespace PizzaEcki
{

    public partial class MainWindow : Window
    {
        private DatabaseManager _databaseManager;
        private List<Dish> dishesList;
        private List<Extra> extrasList;
        private SignalRService signalRService;
        private List<SharedLibrary.OrderItem> orderItems = new List<SharedLibrary.OrderItem>();
        private OrderItem tempOrderItem = new OrderItem();
   

        private int currentBonNumber = 0;
        private int currentReceiptNumber = 0; // das kann auch aus einer Datenbank oder einer Datei gelesen werden
        private int Lieferung = 0;
        private int Selbstabholer = 0;
        private int Mitnehmer = 0;
      


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
        }

        private void PhoneNumberTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Überprüfung, ob die Enter-Taste gedrückt wurde
            if (e.Key == Key.Return)
            {
                string _customerNr = PhoneNumberTextBox.Text;

                // Überprüfung, ob die Telefonnummer eingegeben wurde
                if (_customerNr != null && _customerNr != "")
                {
                    // Behandlung von speziellen Telefonnummern "1" und "2"
                    if (_customerNr == "1")
                    {
                        Selbstabholer++;
                        DishComboBox.Focus();
                    }
                    else if (_customerNr == "2")
                    {
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
                            Lieferung++;
                        }
                        else // Kunde nicht gefunden
                        {
                            // Aufforderung zur Erstellung eines neuen Kunden
                            MessageBoxResult result = MessageBox.Show("Es ist noch kein Kunde mit der Nummer bekann. \n Wollen sie einen Anelgen?", "Frage", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                SaveButton.Visibility = Visibility.Visible;
                                NameTextBox.Focus();
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
        private void Extras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Wenn kein Extra ausgewählt ist, tue nichts
            if (ExtrasComboBox.SelectedItem == null)
            {
                return;
            }

            // Hole das ausgewählte Extra
            Extra selectedExtra = (Extra)ExtrasComboBox.SelectedItem;

            // Aktualisiere das tempOrderItem mit dem ausgewählten Extra und füge den Preis hinzu
            tempOrderItem.Extras = selectedExtra.Name.ToString();
            tempOrderItem.Epreis += selectedExtra.Price;
        }

        //Hier gilt das gleiche wie oben ;D
        private void ExtraCombobox_AutocompleteKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Prüfe, ob das Dropdown-Menü der ComboBox geöffnet ist
                if (ExtrasComboBox.IsDropDownOpen)
                {
                    // Ändere den Text der ComboBox auf das aktuell hervorgehobene Element
                    ExtrasComboBox.Text = (ExtrasComboBox.SelectedItem as Extra)?.Name ?? ExtrasComboBox.Text;
                    ExtrasComboBox.IsDropDownOpen = false; // Schließe das Dropdown-Menü
                }

                // Verschiebe den Fokus auf das nächste Steuerelement in der Tab-Reihenfolge
                amountComboBox.Focus();

                e.Handled = true; // Markiere das Ereignis als behandelt
            }
        }

        //Anzahl in das tempOrderItem Schreiben 
        private void amountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)amountComboBox.SelectedItem;
            if (selectedItem != null)
            {
                tempOrderItem.Menge = int.Parse(selectedItem.Content.ToString());
            }
        }

        private void amountComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessOrder();
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
                Extra selectedExtra = extrasList.FirstOrDefault(x => x.Name == tempOrderItem.Extras);
                if (selectedExtra != null)
                {
                    tempOrderItem.Epreis += selectedExtra.Price;
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
            TotalPriceButton.Content = $"{gesamtPreis:F2} €";
        }

        private void FinishOrderBtn(object sender, RoutedEventArgs e)
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
                OrderItems = orderItems
            };
            SendOrderItems(order);

            // Leeren Sie die Bestellliste
            orderItems.Clear();

            // Aktualisieren Sie die DataGrid-Ansicht, wenn Sie die Liste direkt an die ItemsSource gebunden haben
            myDataGrid.Items.Refresh();

            //Aktualisiere Lieferungsart
            AuslieferungLabel.Content = Lieferung;
            MitnehmerLabel.Content = Mitnehmer;
            SelbstabholerLabel.Content = Selbstabholer;
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
        }

        private void SendOrderItems(Order order)
        {
            signalRService.SendOrderItemsAsync(order);
        }

        private void EinstellungenBtn_Click(object sender, RoutedEventArgs e)
        {
           // SendOrderItems();
        }
    }
}
