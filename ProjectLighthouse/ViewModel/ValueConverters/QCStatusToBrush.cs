using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class QCStatusToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string status) return (Brush)Application.Current.Resources["OnBackground"];
            if (status == "Accepted")
            {
                return (Brush)Application.Current.Resources["Green"];
            }
            else if (status == "Rejected")
            {
                return (Brush)Application.Current.Resources["Red"];
            }
            else
            {
                return (Brush)Application.Current.Resources["Blue"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
