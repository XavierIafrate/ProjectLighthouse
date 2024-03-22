using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class NumberGreaterThanZero : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool gtZero;

            if (value is int intgr)
            {
                gtZero = intgr > 0;
            }
            else if (value is double dbl)
            {
                gtZero = dbl > 0.0;
            }
            else
            {
                throw new NotImplementedException();
            }

            if (targetType == typeof(bool))
            {
                return gtZero;
            }
            else if (targetType == typeof(Visibility))
            {
                return gtZero ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (targetType == typeof(Brush))
            {
                return gtZero ? (Brush)Application.Current.Resources["Teal"] : (Brush)Application.Current.Resources["Red"];
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
