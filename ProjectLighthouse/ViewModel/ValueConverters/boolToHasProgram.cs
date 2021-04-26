using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class boolToHasProgram : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasProgram = (bool)value;

            if (hasProgram)
            {
                return "Has a program";
            }
            else
            {
                return "Does not have a program";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hasProgram = (string)value;

            if (hasProgram == "Has a program")
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
