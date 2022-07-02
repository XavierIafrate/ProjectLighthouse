using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    class isResearchToHiddenCollapsed : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not TechnicalDrawing.Type type) {
                return null;
            }
            return type == TechnicalDrawing.Type.Research? Visibility.Visible : Visibility.Collapsed;   
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
