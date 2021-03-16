using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    class boolToIsReady : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isReady = (bool)value;

            if (isReady)
                return "Ready to go";
            return "Not ready";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string isReady = (string)value;

            if (isReady == "Ready to go")
                return true;
            return false;
        }
    }
}
