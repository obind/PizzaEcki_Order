using System;
using System.Globalization;
using System.Windows.Data;

namespace PizzaEcki.Services
{
    public class BoolToJaNeinConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue == 1 ? "Ja" : "Nein";
            }
            else if (value is string stringValue)
            {
                return stringValue == "1" || stringValue.ToLower() == "ja" ? "Ja" : "Nein";
            }
            return "Nein";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
