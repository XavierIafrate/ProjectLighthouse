using LighthouseMonitoring.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LighthouseMonitoring.ViewModel.ValueConverters
{
    public class MachineStatusToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MachineStatus status) throw new Exception("value passed is not status");

            string brushName = status switch
            {
                MachineStatus.Unknown => "Purple",
                MachineStatus.Running => "Green",
                MachineStatus.Setting => "Blue",
                MachineStatus.Breakdown => "Red",
                MachineStatus.Idle => "Orange",
                _ => throw new Exception("invalid status value")
            };

            return (Brush)Application.Current.Resources[brushName];

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
