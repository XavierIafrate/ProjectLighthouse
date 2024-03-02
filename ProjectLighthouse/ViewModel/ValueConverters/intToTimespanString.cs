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
            else if (value is double dblSeconds)
            {
                seconds = dblSeconds;
            }
            else
            {
                return null;
            }

            seconds = Math.Abs(seconds);
            TimeSpan timespan = TimeSpan.FromSeconds(seconds);
            string message;

            if (seconds < 120)
            {
                message = $"{seconds:0}s";
            }
            else if (seconds < 3600)
            {
                message = $"{timespan.Minutes:0}m {timespan.Seconds:0}s";
            }
            else if (seconds < 86400)
            {
                message = $"{timespan.Hours:0}h {timespan.Minutes:0}m";
            }
            else if (seconds < 86400 * 14)
            {
                message = $"{timespan.Days:0}d {timespan.Hours:0.0}h";
            }
            else
            {
                message = $"{Math.Floor((double)timespan.Days / 7):0}w {timespan.Days%7:0}d";
            }

            return message;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
