using System;
using System.Globalization;
using System.Windows.Data;

namespace PizzaKitchenClient.Services
{
    public class StringToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dateString)
            {
                if (DateTime.TryParse(dateString, out DateTime dateTime))
                {
                    return dateTime.ToString("HH:mm"); // Formatierung zu Stunden und Minuten
                }
            }
            return ""; // Rückgabe eines leeren Strings, falls die Konvertierung fehlschlägt
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
