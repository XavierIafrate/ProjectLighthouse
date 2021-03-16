using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class DateToDateWithCounter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //TODO Check for date rounding
            DateTime inputDate = (DateTime)value;
            double leadTime = (inputDate - DateTime.Now).TotalDays;
            return String.Format("{0:dd/MM/yy} ({1} days lead time)", inputDate, leadTime);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
