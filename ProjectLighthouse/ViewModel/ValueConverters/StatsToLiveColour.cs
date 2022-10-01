using ProjectLighthouse.Model.Analytics;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class StatsToLiveColour : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MachineStatistics stats)
            {
                return null;
            }

            Brush result = stats.Status switch
            {
                "Running" => (Brush)App.Current.Resources["Green"],
                "Setting" => (Brush)App.Current.Resources["Blue"],
                "Breakdown" => (Brush)App.Current.Resources["Red"],
                _ => Brushes.Black,
            };
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
