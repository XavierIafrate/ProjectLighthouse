using ProjectLighthouse.Model.Orders;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class orderItemToRequirementText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not LatheManufactureOrderItem item)
            {
                return null;
            }

            return item.RequiredQuantity > 0
                ? $"{item.RequiredQuantity:#,##0} pcs -> {item.DateRequired:dd/MM}"
                : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
