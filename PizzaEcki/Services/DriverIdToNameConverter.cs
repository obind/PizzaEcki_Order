using PizzaEcki.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PizzaEcki.Services
{
    public class DriverIdToNameConverter : IValueConverter
    {
        private static DatabaseManager _databaseManager = new DatabaseManager(); // Statische Instanz des DatabaseManagers

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is int driverId))
                return null;

            var drivers = _databaseManager.GetAllDrivers();
            var driver = drivers.Find(d => d.Id == driverId);
            return driver != null ? driver.Name : "Unbekannt";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
