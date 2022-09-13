using ProjectLighthouse.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class LatheManufactureOrderToCycleTimeInfo : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not LatheManufactureOrder order) return null;

            if (order.TargetCycleTime == 0)
            {
                return "No data is available for a target cycle time on this order";
            }

            if (order.TargetCycleTimeEstimated)
            {
                return $"An estimated cycle time of {GetCycleTimeText(order.TargetCycleTime)} was used as we have not run these products before.";
            }
            else
            {
                return $"A target cycle time of {GetCycleTimeText(order.TargetCycleTime)} is used based on existing data.";
            }
        }

        private string GetCycleTimeText(int seconds)
        {
            int min = (int)Math.Floor((double)seconds / 60);
            int sec = seconds % 60;

            if (min == 0)
            {
                return $"{sec:0} seconds";
            }
            else
            {
                return $"{min:0}m {sec:0}s";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
