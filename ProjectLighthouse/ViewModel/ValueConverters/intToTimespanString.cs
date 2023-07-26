using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class intToTimespanString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double seconds;

            if (value is int intSeconds)
            {
                seconds = (double)intSeconds;
            }
            else if(value is double dblSeconds)
            {
                seconds = dblSeconds;
            }
            else
            {
                return null;
            }

            seconds = Math.Abs(seconds);
            string message;

            if (seconds < 120)
            {
                message = $"{seconds:0}s";
            }
            else if (seconds < 3600)
            {
                message = $"{Math.Floor(seconds / 60):0}m {seconds % 60:0}s";
            }
            else if (seconds < 86400)
            {
                message = $"{Math.Floor(seconds / 3600):0}h {(seconds % 3600) / 60:0}m";
            }
            else
            {
                message = $"{Math.Floor(seconds / 86400):0}d {(seconds % 86400)/3600:0.0}h";
            }

            return message;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
