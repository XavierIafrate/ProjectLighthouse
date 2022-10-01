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
            LatheManufactureOrder order = value as LatheManufactureOrder;

            if (value == null)
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
