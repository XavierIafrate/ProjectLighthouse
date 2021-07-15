using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class intToDays : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int seconds = (int)value;
            double dblSeconds = double.Parse(seconds.ToString());

            double days = dblSeconds / 86400;
            days *= 4;
            days = Math.Round(days, 0);
            days /= 4;

            return String.Format("{0:0.00}", days);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
