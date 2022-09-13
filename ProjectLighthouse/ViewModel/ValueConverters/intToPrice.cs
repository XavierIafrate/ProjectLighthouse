using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class intToPrice : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dPrice = 0;
            if (value is double val)
            {
                dPrice = val / 100;
            }
            else if (value is int intVal)
            {

                dPrice = System.Convert.ToDouble((int)intVal) / 100;
            }
            return $"£{dPrice:0.00}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
