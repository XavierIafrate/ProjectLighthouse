using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class divideDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double val) throw new Exception($"doubleToHalfValue Convert: Expected double not received (value)");
            if (!double.TryParse(parameter.ToString(), out double par))
            {
                throw new Exception($"doubleToHalfValue Convert: Expected double not received (parameter)");
            }


            return val / par;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double val) throw new Exception($"doubleToHalfValue ConvertBack: Expected double not received (value)");
            if (parameter is not double par) throw new Exception($"doubleToHalfValue ConvertBack: Expected double not received (parameter)");

            return val * par;
        }
    }
}
