using PizzaEcki.Database;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PizzaEcki.Services
{
    public class NameToDriverIdConverter : IValueConverter
    {
        private static DatabaseManager _databaseManager = new DatabaseManager(); // Statische Instanz des DatabaseManagers

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is string driverName))
                return null;

            var drivers = _databaseManager.GetAllDrivers();
            var driver = drivers.FirstOrDefault(d => d.Name.Equals(driverName, StringComparison.OrdinalIgnoreCase));
            return driver?.Id ?? -1; // Gibt -1 zurück, wenn kein Fahrer gefunden wird
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is int driverId))
                return null;

            var drivers = _databaseManager.GetAllDrivers();
            var driver = drivers.FirstOrDefault(d => d.Id == driverId);
            return driver?.Name ?? "Unbekannt"; // Gibt "Unbekannt" zurück, wenn keine ID zugeordnet werden kann
        }
    }
}