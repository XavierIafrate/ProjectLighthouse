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

            bool faded = false;
            bool dark = false;
            bool light = false;

            if (parameter is string str)
            {
                faded = str == "faded";
                dark = str == "dark";
                light = str == "light";
            }

            string colour;

            if (state == OrderState.Problem)
            {
                colour = "Red";
            }
            else if (state is OrderState.Ready or OrderState.Prepared)
            {
                colour = "Green";

            }
            else if (state is OrderState.Running)
            {
                colour = "Blue";
            }
            else
            {
                colour = "Black";
            }

            if (faded)
            {
                colour += "Faded";
            }
            else if (dark)
            {
                colour += "Dark";
            }
            else if (light)
            {
                colour += "Light";
            }

            return (Brush)Application.Current.Resources[colour];            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
