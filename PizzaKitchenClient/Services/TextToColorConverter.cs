using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace PizzaKitchenClient.Services
{
    public class TextToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            switch (text)
            {
                case "Selbstabholer":
                    return Brushes.Yellow;
                case "Mitnehmer":
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
