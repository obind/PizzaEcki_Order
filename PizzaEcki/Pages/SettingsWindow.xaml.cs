using System.Windows;
using PizzaEcki.Database;
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
        }

        private void LoadDrivers()
        {
            DriversList.ItemsSource = _dbManager.GetDrivers();
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
