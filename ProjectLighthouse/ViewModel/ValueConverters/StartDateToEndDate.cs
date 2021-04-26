using ProjectLighthouse.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class StartDateToEndDate : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LatheManufactureOrder order = value as LatheManufactureOrder;
            DateTime EndTime = order.StartDate.AddSeconds(order.TimeToComplete);
            return EndTime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
