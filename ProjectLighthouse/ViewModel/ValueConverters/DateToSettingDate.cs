using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class DateToSettingDate : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime date)
            {
                return null;
            }

            if (date.Year < 1970)
            {
                return "Pending";
            }

            return (double)(Math.Ceiling((date.Date - DateTime.Now.Date).TotalDays)) switch
            {
                -1 => "Yesterday",
                0 => "Today",
                1 => "Tomorrow",
                _ => date.ToString("ddd, dd MMMM"),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
