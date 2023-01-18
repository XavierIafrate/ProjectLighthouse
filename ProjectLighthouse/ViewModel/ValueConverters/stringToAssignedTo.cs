using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class stringToAssignedTo : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string person)
            {
                return null;
            }

            return person == App.CurrentUser.UserName ? "you" : person;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
