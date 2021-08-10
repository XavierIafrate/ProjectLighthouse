using System;
using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for AnalyticsView.xaml
    /// </summary>
    public partial class AnalyticsView : UserControl
    {
        public Func<double, string> TimeFormatter = value => new DateTime((long) value).ToString("dd MMMM yyyy");
        public Func<double, string> QuantityFormatter = value => $"{value:#,##0}";
        public Func<double, string> ThousandPoundFormatter = value => $"£{value:#,##0}";
        //public Func<double, string> SecondsToDaysFormatter { get; set; }
        public Func<double, string> PercantageStringFormat = value => string.Format("{0}%", value);

        public AnalyticsView()
        {
            InitializeComponent();
            
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            totalParts_X.LabelFormatter = TimeFormatter;
            totalParts_Y.LabelFormatter = QuantityFormatter;
            Value_Y.LabelFormatter = ThousandPoundFormatter;
            Gauge.LabelFormatter = PercantageStringFormat;
        }
    }
}
