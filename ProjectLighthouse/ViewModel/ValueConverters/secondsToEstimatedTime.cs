using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class secondsToEstimatedTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = "Error";

            if (value is int seconds)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
                if (timeSpan.TotalHours < 1)
                {
                    result = $"{timeSpan.Minutes}m";
                }
                else if (timeSpan.TotalDays < 1)
                {
                    result = $"{timeSpan.Hours}h {timeSpan.Minutes}m";
                }
                else
                {
                    result = $"{timeSpan.Days}d {timeSpan.Hours}h";
                }

                return result;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
