using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.View.UserControls.Core
{
    public class IntegerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strParameter = string.Empty;

            if (parameter is string str)
            {
                strParameter  = str;
            }

            if (value is int integer)
            {
                if (strParameter == "hide-zero" && integer == 0)
                {
                    return "";
                }

                return integer;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int integer) return integer;
            if (value is string str)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return 0;
                }

                if(int.TryParse(str, out integer)) return integer;

                return 0;
            }

            throw new NotImplementedException();
        }
    }
}
