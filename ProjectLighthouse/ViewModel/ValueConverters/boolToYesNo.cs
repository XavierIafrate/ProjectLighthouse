using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    class boolToYesNo : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "YES" : "NO";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "YES")
            {
                return true;
            }
            else if ((string)value == "NO")
            {
                return false;
            }
            else
            {
                return null;
            }
        }
    }
}
