using System.Collections.Generic;
using System.Collections.ObjectModel;
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


        public SettingsWindow()
        {
            InitializeComponent();
            _dbManager = new DatabaseManager();
            LoadDrivers();
            LoadDishes(); 
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


        private void EditDriverButton_Click(object sender, RoutedEventArgs e)
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
            if (dialog.ShowDialog() == true)
            {
                _dbManager.AddOrUpdateDish(dialog.Dish);
                Dishes.Add(dialog.Dish);  // Fügt das neue Gericht zur ObservableCollection hinzu
            }
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

    }
}
