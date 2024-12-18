using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class intToStringCycleTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int cycleTime;

            if (value is null)
            {
                cycleTime = 0;
            }
            else if (value is int intgr)
            {
                cycleTime = intgr;
            }
            else
            {
                throw new InvalidCastException();
            }


            if (cycleTime == 0)
            {
                return "Unknown";
            }

            int min = (cycleTime - (cycleTime % 60)) / 60;
            int sec = cycleTime % 60;

            return ($"{min}:{sec:00}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
