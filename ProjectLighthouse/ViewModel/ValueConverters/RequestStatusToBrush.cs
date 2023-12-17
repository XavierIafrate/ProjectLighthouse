using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using static ProjectLighthouse.Model.Requests.Request;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class RequestStatusToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not RequestStatus status) return null;

            if (status == RequestStatus.Pending)
            {
                return (Brush)App.Current.Resources["Blue"];
            }
            else if (status == RequestStatus.Declined)
            {
                return (Brush)App.Current.Resources["Red"];
            }
            else
            {
                return (Brush)App.Current.Resources["Green"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
