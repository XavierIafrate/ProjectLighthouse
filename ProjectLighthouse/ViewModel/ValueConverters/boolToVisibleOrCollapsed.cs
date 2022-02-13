using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class boolToVisibleOrCollapsed : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool b)
            {
                return null;
            }

            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
