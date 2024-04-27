using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Input;
using PizzaEcki.Database;
using PizzaEcki.Models;
using SharedLibrary;
using System.Printing;
using System.Text;

namespace PizzaEcki.Pages
{
    public partial class SettingsWindow : Window
    {
        private DatabaseManager _dbManager;
        public ObservableCollection<Dish> Dishes { get; set; }
        public TimeSpan HappyHourStartTime { get; private set; }
        public TimeSpan HappyHourEndTime { get; private set; }


        public SettingsWindow()
        {
            InitializeComponent();
            _dbManager = new DatabaseManager();
            LoadDrivers();
            LoadDishes();
          
            PopulateDayComboBoxes();
            LoadHappyHourDaySettings();

            HappyHourStartTimePicker.Value = DateTime.Today.Add(Properties.Settings.Default.HappyHourStart);
            HappyHourEndTimePicker.Value = DateTime.Today.Add(Properties.Settings.Default.HappyHourEnd);

            if (LocalPrinterComboBox.Items.Contains(PizzaEcki.Properties.Settings.Default.SelectedPrinter))
            {
                LocalPrinterComboBox.SelectedItem = PizzaEcki.Properties.Settings.Default.SelectedPrinter;
            }

        }

        private void LoadDrivers()
        {
            DriversList.ItemsSource = _dbManager.GetDrivers();
        }

        private void LoadDishes()
        {
            Dishes = new ObservableCollection<Dish>(_dbManager.GetAllDishes());
            DishListView.ItemsSource = Dishes;
        }



        private void AddDriverButton_Click(object sender, RoutedEventArgs e)
        {
            DriverDialog dialog = new DriverDialog();
            if (dialog.ShowDialog() == true)
            {
                Driver newDriver = new Driver
                {
                    Name = dialog.DriverName,
                    PhoneNumber = dialog.PhoneNumber // Hinzufügen der Telefonnummer
                };
                _dbManager.AddDriver(newDriver);
                LoadDrivers();
            }
        }

        private void DriversList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditDriverButton_Click();
        }

        private void EditDriverButton_Click()
        {
            Driver selectedDriver = DriversList.SelectedItem as Driver;
            if (selectedDriver != null)
            {
                DriverDialog dialog = new DriverDialog(selectedDriver);
                if (dialog.ShowDialog() == true)
                {
                    selectedDriver.Name = dialog.DriverName;
                    selectedDriver.PhoneNumber = dialog.PhoneNumber; // Aktualisieren der Telefonnummer
                                                                     // Weitere Aktualisierungen nach Bedarf
                    _dbManager.UpdateDriver(selectedDriver);
                    LoadDrivers();
                }
            }
        }

        private void AddDishButton_Click(object sender, RoutedEventArgs e)
        {
            DishDialog dialog = new DishDialog();
            dialog.Closed += Dialog_Closed; // Abonnieren des Closed-Events
            dialog.ShowDialog();

        }
        private void Dialog_Closed(object sender, EventArgs e)
        {
            LoadDishes(); // Aufrufen von LoadDishes, wenn das Dialogfenster geschlossen wird
        }


        private void DishListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DishListView.SelectedItem is Dish selectedDish)
            {
                DishDialog dialog = new DishDialog(selectedDish);
                if (dialog.ShowDialog() == true)
                {
                    _dbManager.AddOrUpdateDish(dialog.Dish);
                    LoadDishes();
                }
            }
        }



        private void DeleteDishButton_Click(object sender, RoutedEventArgs e)
        {
            if (DishListView.SelectedItem is Dish selectedDish)
            {
                // Bestätigungsfenster, um sicherzustellen, dass der Benutzer das Gericht wirklich löschen möchte
                MessageBoxResult result = MessageBox.Show("Sind Sie sicher, dass Sie dieses Gericht löschen möchten?", "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Lösche das Gericht aus der Datenbank
                    _dbManager.DeleteDish(selectedDish.Id);

                    // Entferne das Gericht aus der ObservableCollection
                    Dishes.Remove(selectedDish);
                }
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie ein Gericht aus, das Sie löschen möchten.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void PopulateDayComboBoxes()
        {
            string[] days = { "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag", "Sonntag" };
            foreach (var day in days)
            {
                HappyHourStartDayComboBox.Items.Add(day);
                HappyHourEndDayComboBox.Items.Add(day);
            }
        }

        private void LoadHappyHourDaySettings()
        {
            // Laden der gespeicherten Wochentage
            HappyHourStartDayComboBox.SelectedItem = Properties.Settings.Default.HappyHourStartDay;
            HappyHourEndDayComboBox.SelectedItem = Properties.Settings.Default.HappyHourEndDay;
        }

        private void SaveHappyHourTimesButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.HappyHourStart = HappyHourStartTimePicker.Value?.TimeOfDay ?? TimeSpan.Zero;
            Properties.Settings.Default.HappyHourEnd = HappyHourEndTimePicker.Value?.TimeOfDay ?? TimeSpan.Zero;
            Properties.Settings.Default.HappyHourStartDay = HappyHourStartDayComboBox.SelectedItem.ToString();
            Properties.Settings.Default.HappyHourEndDay = HappyHourEndDayComboBox.SelectedItem.ToString();
            Properties.Settings.Default.Save();

            MessageBox.Show("Happy Hour Zeiten und Tage wurden gespeichert.");
        }


        public static class ApplicationSettings
        {
            public static TimeSpan HappyHourStartTime { get; set; }
            public static TimeSpan HappyHourEndTime { get; set; }
        }

        private void DeleteDriverButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDriver = DriversList.SelectedItem as Driver;
            if (selectedDriver != null)
            {
                MessageBoxResult result = MessageBox.Show("Sind Sie sicher, dass Sie diesen Fahrer löschen möchten?", "Fahrer löschen", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool deleteSuccessful = _dbManager.DeleteDriver(selectedDriver.Id);

                    if (deleteSuccessful)
                    {
                        // Aktualisiere die Liste der Fahrer
                        LoadDrivers();
                    }
                    else
                    {
                        MessageBox.Show("Der Fahrer konnte nicht gelöscht werden, da er zugewiesene Bestellungen hat.", "Löschen fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie einen Fahrer zum Löschen aus.");
            }
        }





        private void PopulatePrinterComboBoxes()
        {
            // Lokale Drucker
            LocalPrinterComboBox.Items.Clear();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                LocalPrinterComboBox.Items.Add(printer);
            }

         
            // Setzen der aktuell gespeicherten Drucker, falls vorhanden
            var savedLocalPrinter = PizzaEcki.Properties.Settings.Default.SelectedPrinter;
            if (!string.IsNullOrEmpty(savedLocalPrinter) && LocalPrinterComboBox.Items.Contains(savedLocalPrinter))
            {
                LocalPrinterComboBox.SelectedItem = savedLocalPrinter;
            }
        }

        private void SavePrinterSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            bool saveSuccessful = true;

            // Speichern des lokalen Druckers
            if (LocalPrinterComboBox.SelectedItem != null)
            {
                string selectedLocalPrinter = LocalPrinterComboBox.SelectedItem.ToString();
                PizzaEcki.Properties.Settings.Default.SelectedPrinter = selectedLocalPrinter;
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie einen lokalen Drucker aus der Liste.");
                saveSuccessful = false;
            }

            

            if (saveSuccessful)
            {
                PizzaEcki.Properties.Settings.Default.Save();
                MessageBox.Show("Drucker wurden gespeichert.");
            }
        }

        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            PopulatePrinterComboBoxes();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Überprüfe, ob die Passwörter übereinstimmen
            if (NewPasswordInput.Password == ConfirmPasswordInput.Password)
            {
                SaveEncryptedPassword(NewPasswordInput.Password);
                MessageBox.Show("Passwort wurde gesetzt.");
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Die Passwörter stimmen nicht überein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveEncryptedPassword(string password)
        {
            string encryptedPassword = EncryptPassword(password);
            Properties.Settings.Default.EncryptedPassword = encryptedPassword;
            Properties.Settings.Default.Save(); // Speichern der Änderungen
        }
        private string EncryptPassword(string password)
        {
            // Eine einfache Verschlüsselungsmethode (nur als Beispiel, für echte Anwendungen stärkere Methoden verwenden)
            // Ersetzen Sie dies durch Ihre bevorzugte Verschlüsselungsmethode
            byte[] data = Encoding.UTF8.GetBytes(password);
            // Verwenden Sie hier Ihre Verschlüsselungslogik
            return Convert.ToBase64String(data);
        }

    }
}
