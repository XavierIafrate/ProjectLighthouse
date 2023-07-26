using ProjectLighthouse.Model.Requests;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class RequestToRequestedLeadTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return null;
            if (value is not Request request) throw new ArgumentException("Request expected");

            double weeks = (request.DateRequired - request.DateRaised).TotalDays / 7;
            weeks *= 2;
            weeks = Math.Round(weeks);
            weeks /= 2;

            return weeks;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
