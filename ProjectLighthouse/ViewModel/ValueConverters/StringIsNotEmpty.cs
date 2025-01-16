using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    class StringIsNotEmpty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string strVal)
            {
                if (value is null)
                {
                    strVal = string.Empty;
                }
                else
                {
                    throw new ArgumentException("StringIsNotEmpty converter expected string");
                }
            }

            bool isEmpty = string.IsNullOrWhiteSpace(strVal);

            if (targetType == typeof(Visibility))
            {
                return isEmpty
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
