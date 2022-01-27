using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class DateHighlighter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime)values[0];
            var dates = values[1] as HashSet<DateTime>;
            return dates.Contains(date);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
