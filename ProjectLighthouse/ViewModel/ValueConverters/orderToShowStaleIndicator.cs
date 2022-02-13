using ProjectLighthouse.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class orderToShowStaleIndicator : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not LatheManufactureOrder order)
            {
                return null;
            }
            Visibility result = order.ModifiedAt.AddDays(3) < DateTime.Now
                    && order.State == OrderState.Problem
                    && order.CreatedAt.AddDays(3) < DateTime.Now
                            ? Visibility.Visible
                            : Visibility.Collapsed;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
