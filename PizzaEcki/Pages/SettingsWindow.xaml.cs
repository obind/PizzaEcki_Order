using System.Collections.Generic;
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
            DatabaseManager dbManager = new DatabaseManager();
            List<Dish> dishesFromDb = dbManager.GetAllDishes();
            DishListView.ItemsSource = dishesFromDb;
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
                // Update database and reload ListView
                _dbManager.AddOrUpdateDish(dialog.Dish);
                LoadDishes();
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

        //private void DeleteDriverButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Driver selectedDriver = DriversList.SelectedItem as Driver;
        //    if (selectedDriver != null)
        //    {
        //        _dbManager.DeleteDriver(selectedDriver);
        //        LoadDrivers();
        //    }
        //}
    }
}
