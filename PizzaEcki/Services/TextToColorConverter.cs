using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PizzaEcki.Services
{
    public class TextToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;
            switch (status)
            {
                case "Selbstabholer":
                    return Brushes.Yellow;
                case "Mitnehmer":
                    return Brushes.Red;
                case "1":
                    return Brushes.Yellow;
                case "2":
                    return Brushes.Red;
                default:
                    return Brushes.White; // Standardfarbe, falls keine Übereinstimmung
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
