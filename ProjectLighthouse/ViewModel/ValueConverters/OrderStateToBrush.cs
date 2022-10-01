using ProjectLighthouse.Model.Orders;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class OrderStateToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not OrderState state)
            {
                return null;
            }

            if (state == OrderState.Problem)
            {
                return (Brush)Application.Current.Resources["Red"];
            }
            else if (state is OrderState.Ready or OrderState.Prepared)
            {
                return (Brush)Application.Current.Resources["Green"];

            }
            else if (state is OrderState.Running)
            {
                return (Brush)Application.Current.Resources["Blue"];
            }
            else
            {
                return (Brush)Application.Current.Resources["OnBackground"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
