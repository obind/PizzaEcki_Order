﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Input;
using PizzaEcki.Database;
using PizzaEcki.Models;
using SharedLibrary;

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
            PopulatePrinterComboBox();

            HappyHourStartTimePicker.Value = DateTime.Today.Add(Properties.Settings.Default.HappyHourStart);
            HappyHourEndTimePicker.Value = DateTime.Today.Add(Properties.Settings.Default.HappyHourEnd);

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
        private void SaveHappyHourTimesButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.HappyHourStart = HappyHourStartTimePicker.Value?.TimeOfDay ?? TimeSpan.Zero;
            Properties.Settings.Default.HappyHourEnd = HappyHourEndTimePicker.Value?.TimeOfDay ?? TimeSpan.Zero;
            Properties.Settings.Default.Save(); // Sehr wichtig, um die Einstellungen zu speichern

            MessageBox.Show("Happy Hour Zeiten wurden gespeichert.");
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
                    _dbManager.UnassignDriverFromOrders(selectedDriver.Id);
                    _dbManager.DeleteDriver(selectedDriver.Id);

                    // Hier müsstest du die Liste der Fahrer aktualisieren, zum Beispiel:
                    LoadDrivers();
                }
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie einen Fahrer zum Löschen aus.");
            }
        }
        private void PopulatePrinterComboBox()
        {
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                PrinterComboBox.Items.Add(printer);
            }

            // Optional: Wähle den aktuell eingestellten Drucker aus, wenn einer gespeichert ist
            // PrinterComboBox.SelectedItem = Properties.Settings.Default.SelectedPrinter;
        }
        private void SavePrinterSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (PrinterComboBox.SelectedItem != null)
            {
                string selectedPrinter = PrinterComboBox.SelectedItem.ToString();
                PizzaEcki.Properties.Settings.Default.SelectedPrinter = selectedPrinter;
                PizzaEcki.Properties.Settings.Default.Save();

                MessageBox.Show("Drucker wurde gespeichert: " + selectedPrinter);
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie einen Drucker aus der Liste.");
            }
        }
    }
}
