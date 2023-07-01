using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PizzaEcki.Database;
using System.Linq;
using PizzaEcki.Models;
using PizzaEcki.Extensions;

namespace PizzaEcki
{ 

    public partial class MainWindow : Window
    {
        private DatabaseManager _databaseManager;
        private List<Dish> dishesList;
        private List<Extra> extrasList;
        private List<OrderItem> orderItems = new List<OrderItem>();
        private OrderItem tempOrderItem = new OrderItem();
        private ListBox extrasListBox;

        public MainWindow()
        {
            InitializeComponent();

            _databaseManager = new DatabaseManager();

            // Füllen Sie die ComboBox für Gerichte aus der Datenbank
            dishesList = _databaseManager.GetDishes();
            dishComboBox.ItemsSource = dishesList;
            
            extrasList = _databaseManager.GetExtras();
            extras.ItemsSource = extrasList;
            // Initialisiere die Liste von Extras
            extrasList = new List<Extra>
            {
                new Extra { Id = 1, Name = "Extra Käse", Price = 2 },
                new Extra { Id = 2, Name = "Pepperoni", Price =1.5 },
                new Extra { Id = 3, Name = "Oliven", Price = 1 },
                // Füge weitere Extras hinzu
            };

            extras.ItemsSource = extrasList;


        }

        private void OnEnterKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string phoneNumber = PhoneNumberTextBox.Text;

                Customer customer = _databaseManager.GetCustomerByPhoneNumber(phoneNumber);

                if (customer != null)
                {
                    // Setze die Werte der Kundendaten in die entsprechenden Felder
                    SetCustomerDataToFields(customer);
                }
            }
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                // Speichere den Kunden, wenn die Eingabetaste gedrückt wurde
                Customer customer = new Customer
                {
                    PhoneNumber = PhoneNumberTextBox.Text,
                    Name = NameTextBox.Text,
                    Street = StreetTextBox.Text,
                    City = CityTextBox.Text,
                    AdditionalInfo = AdditionalInfoTextBox.Text
                };

                _databaseManager.AddOrUpdateCustomer(customer);
            }
        }

        private void OnSaveButtonClicked(object sender, RoutedEventArgs e)
        {
            // Speichere den Kunden, wenn der Button geklickt wurde
            Customer customer = new Customer
            {
                PhoneNumber = PhoneNumberTextBox.Text,
                Name = NameTextBox.Text,
                Street = StreetTextBox.Text,
                City = CityTextBox.Text,
                AdditionalInfo = AdditionalInfoTextBox.Text
            };

            // Überprüfe, ob der Kunde bereits existiert
            Customer existingCustomer = _databaseManager.GetCustomerByPhoneNumber(customer.PhoneNumber);

            // Wenn der Kunde bereits existiert, zeige einen Bestätigungsdialog an
            if (existingCustomer != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Der Kunde mit dieser Telefonnummer existiert bereits. Sind Sie sicher, dass Sie die Daten überschreiben möchten?",
                    "Bestätigung",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                // Wenn der Benutzer Nein wählt, verlasse die Methode
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            // Füge den neuen Kunden hinzu oder aktualisiere den bestehenden Kunden
            _databaseManager.AddOrUpdateCustomer(customer);
        }


        private void SetCustomerDataToFields(Customer customer)
        {
            NameTextBox.Text = customer.Name;
            StreetTextBox.Text = customer.Street;
            CityTextBox.Text = customer.City;
            AdditionalInfoTextBox.Text = customer.AdditionalInfo;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string phoneNumber = PhoneNumberTextBox.Text;

            Customer customer = _databaseManager.GetCustomerByPhoneNumber(phoneNumber);

            if (customer != null)
            {
                NameTextBox.Text = customer.Name;
                StreetTextBox.Text = customer.Street;
                CityTextBox.Text = customer.City;
                AdditionalInfoTextBox.Text = customer.AdditionalInfo;
            }
        }

        private void dishComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Wenn kein Gericht ausgewählt ist, tue nichts
            if (dishComboBox.SelectedItem == null)
            {
                return;
            }

            // Ansonsten aktualisiere das tempOrderItem mit dem ausgewählten Gericht
            Dish selectedDish = (Dish)dishComboBox.SelectedItem;
            tempOrderItem.Gericht = selectedDish.Name;
            tempOrderItem.Epreis = selectedDish.Price;

            // Leere die ausgewählten Extras, da sich das Gericht geändert hat
            tempOrderItem.Extras = "";
        }


        private void extras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Leere die Extras und den zusätzlichen Preis
            tempOrderItem.Extras = "";
            tempOrderItem.Epreis = 0;

            // Füge alle ausgewählten Extras hinzu
            foreach (Extra extra in extras.Items)
            {
                tempOrderItem.Extras += extra.Name + "; ";
                tempOrderItem.Epreis += extra.Price;
            }
        }

        private void AddOrderItemButton_Click(object sender, RoutedEventArgs e)
        {
            // Add the tempOrderItem to the list
            tempOrderItem.Nr = orderItems.Count + 1; // Or any other way you generate the Nr
            orderItems.Add(tempOrderItem);
            myDataGrid.ItemsSource = null;
            myDataGrid.ItemsSource = orderItems;

            // Create a new tempOrderItem
            tempOrderItem = new OrderItem();
        }

        private void amountComboBox_KeyUp(object sender, KeyEventArgs e)
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

                // Setzen Sie tempOrderItem zurück, um bereit für die nächste Eingabe zu sein
                tempOrderItem = new OrderItem();

                // Löschen Sie die Auswahlen und den Text in den ComboBoxen und dem TextBox
                dishComboBox.SelectedItem = null;
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

        private void dishComboBox_TextChanged(object sender, EventArgs e)
        {
            // Get the text the user has typed
            var searchText = dishComboBox.Text;

            // Search the dishes in the database
            var matchingDishes = _databaseManager.SearchDishes(searchText);
            if(matchingDishes != null)
            {
                // Update the ItemsSource of the ComboBox
                dishComboBox.ItemsSource = null;
                dishComboBox.ItemsSource = matchingDishes;
                // Set the text back to what the user has typed
                dishComboBox.Text = searchText;
                dishComboBox.ItemsSource = dishesList;
            }
            else
            {
                dishComboBox.ItemsSource = dishesList;
            }
       
        }

        private void dishComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Check if the dropdown of the combobox is opened
                if (dishComboBox.IsDropDownOpen)
                {
                    // Change the text of the combobox to the currently highlighted item
                    dishComboBox.Text = (dishComboBox.SelectedItem as Dish)?.Name ?? dishComboBox.Text;
                    dishComboBox.IsDropDownOpen = false;
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

    }
}
