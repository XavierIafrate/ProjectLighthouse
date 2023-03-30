using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class DateTimeToRelativeTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return "unknown";
            if (value is not DateTime date) throw new ArgumentException("DateTimeToRelativeTime expected a DateTime");

            TimeSpan span = DateTime.Now - date;

            if (span.TotalSeconds < 120)
            {
                return "a moment ago";
            }
            else if (span.TotalMinutes < 60)
            {
                return $"{span.TotalMinutes:0} minutes ago";
            }
            else if (span.TotalHours < 2)
            {
                return "an hour ago";
            }
            else if (date.Date == DateTime.Today)
            {
                return $"{span.TotalHours:0} hours ago";
            }
            else if (date.AddDays(1).Date == DateTime.Today)
            {
                return "yesterday";
            }
            else if (date.AddDays(2).Date == DateTime.Today)
            {
                return "two days ago";
            }
            else if (span.TotalDays < 14)
            {
                return $"{span.TotalDays:0} days ago";
            }
            else if (span.TotalDays < 28)
            {
                return $"{Math.Round(span.TotalDays / 7):0} weeks ago";
            }
            else if (span.TotalDays < 370)
            {
                int months = (int)Math.Round(span.TotalDays / (365.25 / 12));
                return months == 1 ? "a month ago" : $"{months:0} months ago";
            }
            else
            {
                int years = (int)Math.Round(span.TotalDays / 365.25);
                return years == 1 ? "a year ago" : $"{years:0} years ago";
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
