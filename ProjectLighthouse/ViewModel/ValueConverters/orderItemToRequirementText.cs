using ProjectLighthouse.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class orderItemToRequirementText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LatheManufactureOrderItem item = value as LatheManufactureOrderItem;

            if (item.RequiredQuantity > 0)
            {
                return String.Format("({0:#,##0} pcs for {1:dd/MM})", item.RequiredQuantity, item.DateRequired);
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
