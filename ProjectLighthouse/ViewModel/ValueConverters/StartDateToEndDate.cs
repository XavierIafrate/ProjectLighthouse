using ProjectLighthouse.Model.Orders;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class StartDateToEndDate : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not LatheManufactureOrder order)
            {
                return null;
            }

            return order.StartDate.AddSeconds(order.TimeToComplete);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
