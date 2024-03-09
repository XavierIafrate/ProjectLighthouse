using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class dateTimeToStartDate : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime startDate) return null;

            DayOfWeek DayOfWeekNow = DateTime.Now.DayOfWeek;

            if (startDate.Date == DateTime.Today)
            {
                return "Today";
            }
            else if (startDate.Date == DateTime.MinValue)
            {
                return "Pending";
            }
            else if (startDate.Date.AddDays(1) == DateTime.Today)
            {
                return "Yesterday";
            }
            else if (startDate.Date.AddDays(-1) == DateTime.Today)
            {
                return "Tomorrow";
            }
            else if (startDate.Date.DayOfWeek > DayOfWeekNow && startDate.AddDays(-7) < DateTime.Now && startDate > DateTime.Now)
            {
                return startDate.DayOfWeek;
            }
            else if (startDate.Year == DateTime.Now.Year)
            {
                return $"{startDate:ddd, d}{GetDaySuffix(startDate.Day)} {startDate:MMMM}";
            }
            else
            {
                return $"{startDate:ddd, dd/MM/yy}";
            }
        }

        private static string GetDaySuffix(int day)
        {
            return day switch
            {
                1 or 21 or 31 => "st",
                2 or 22 => "nd",
                3 or 23 => "rd",
                _ => "th",
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
