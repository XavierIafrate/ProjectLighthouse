using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class ObjectIsNotNull : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool notNull = value is not null;

            if(targetType == typeof(string))
            {
                return notNull ? "yes" : "no";
            }
            else if (targetType == typeof(bool))
            {
                return notNull;
            }
            else if (targetType == typeof(Visibility))
            {
                return notNull ? Visibility.Visible : Visibility.Collapsed;
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
