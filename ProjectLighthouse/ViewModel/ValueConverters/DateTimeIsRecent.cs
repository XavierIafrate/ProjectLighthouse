using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class DateTimeIsRecent : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime date) return null;

            bool isRecent = date.AddHours(4) > DateTime.Now;

            if (targetType == typeof(bool))
            {
                return isRecent;
            }
            else if(targetType == typeof(Visibility))
            {
                return isRecent ? Visibility.Visible : Visibility.Collapsed;
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
