using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    class dateToLastModified : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime date) return null;

            if (date.Date == DateTime.Today)
            {
                return $"today at {date:h:mm tt}";
            }
            else if (date.AddDays(1).Date == DateTime.Today)
            {
                return $"yesterday at {date:h:mm tt}";
            }
            else if(date.Year == DateTime.Now.Year)
            {
                return $"{date:dddd, d}{GetDaySuffix(date.Day)} {date:MMMM}";
            }
            else
            {
                return $"{date:dddd, d}{GetDaySuffix(date.Day)} {date:MMMM yyyy}";
            }
        }

        private string GetDaySuffix(int day)
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
