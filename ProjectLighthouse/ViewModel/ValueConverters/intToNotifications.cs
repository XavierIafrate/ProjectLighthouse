﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class intToNotifications : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int count) return null;

            if (count == 0)
            {
                return "No notifications";
            }
            else if (count == 1)
            {
                return "You have 1 notification";
            }
            else if (count > 9)
            {
                return $"You have 9+ notifications.";
            }
            else
            {
                return $"You have {count:0} notifications.";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
