using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PizzaEcki.Services
{
    public class PhoneNumberToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string phoneNumber = value as string;
            switch (phoneNumber)
            {
                case "1":
                    return "Selbstabholer";
                case "2":
                    return "Mitnehmer";
                default:
                    return phoneNumber;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
