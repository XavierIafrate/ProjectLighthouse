using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class intToHours : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double seconds = (double)value;
            double hours = TimeSpan.FromSeconds(seconds).TotalHours;

            hours *= 2;
            hours = Math.Round(hours);
            hours /= 2;

            return $"{hours:0.0}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
