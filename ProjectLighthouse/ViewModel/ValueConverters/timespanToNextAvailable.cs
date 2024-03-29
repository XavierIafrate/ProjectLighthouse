﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class timespanToNextAvailable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan timeSpan = (TimeSpan)value;
            DateTime nextAvailable = DateTime.Now.AddDays(timeSpan.TotalDays + 1);
            if (nextAvailable.DayOfWeek == DayOfWeek.Sunday)
            {
                nextAvailable = nextAvailable.AddDays(1);
            }
            if (nextAvailable.DayOfWeek == DayOfWeek.Saturday)
            {
                nextAvailable = nextAvailable.AddDays(2);
            }
            return $"{nextAvailable:ddd dd MMM}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
