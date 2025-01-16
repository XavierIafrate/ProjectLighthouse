using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class NumberEqualToZero : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool eqZero;

            if (value is int intgr)
            {
                eqZero = intgr == 0;
            }
            else if (value is double dbl)
            {
                eqZero = dbl == 0.0;
            }
            else if (value is null)
            {
                eqZero = true;
            }
            else
            {
                throw new NotImplementedException();
            }

            if (targetType == typeof(bool))
            {
                return eqZero;
            }
            else if (targetType == typeof(Visibility))
            {
                return eqZero ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (targetType == typeof(Brush))
            {
                return eqZero ? (Brush)Application.Current.Resources["Teal"] : (Brush)Application.Current.Resources["Red"];
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
