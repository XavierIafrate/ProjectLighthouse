using ProjectLighthouse.Model.Research;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectLighthouse.ViewModel.ValueConverters
{
    public class ResearchStageToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ResearchStage stage) return null;

            string brushName = stage switch
            {
                ResearchStage.Research => "Orange",
                ResearchStage.Design => "Purple",
                ResearchStage.Prototyping => "Blue",
                ResearchStage.Production => "Green",
                ResearchStage.Marketing => "OnBackground",
                _ => throw new ArgumentOutOfRangeException("Invalid ResearchStage enumeration value"),
            };

            if (parameter is string faded)
            {
                brushName += faded;
            }

            return (Brush)App.Current.Resources[brushName];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
