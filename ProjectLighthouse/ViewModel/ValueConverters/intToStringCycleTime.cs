using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class intToStringCycleTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is not int cycleTime) throw new InvalidCastException();

            if (cycleTime == 0)
            {
                return "Unknown";
            }

            TimeSpan t = TimeSpan.FromSeconds(cycleTime);
            return ($"{t.Minutes}m {t.Seconds}s");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
