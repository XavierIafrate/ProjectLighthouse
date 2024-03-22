using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class CycleTimeChangeColour : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int change) return null;

            string suffix = string.Empty;
            if (parameter is string str)
            {
                suffix =str;
            }

            string targetBrush = change > 0 ? "Red" : "Teal";
            targetBrush += suffix;

            return (Brush)Application.Current.Resources[targetBrush];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
