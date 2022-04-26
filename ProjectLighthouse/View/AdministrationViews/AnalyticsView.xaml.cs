using System;
using System.Windows.Controls;
using System.Windows;

namespace ProjectLighthouse.View
{
    public partial class AnalyticsView : UserControl
    {
        public Func<double, string> TimeFormatter = value => new DateTime((long)value).ToString("dd MMMM yyyy");
        public Func<double, string> QuantityFormatter = value => $"{value:#,##0}";
        //public Func<double, string> ThousandPoundFormatter = value => $"£{value:#,##0}";
        //public Func<double, string> SecondsToDaysFormatter { get; set; }
        //public Func<double, string> PercantageStringFormat = value => string.Format("{0}%", value);

        public AnalyticsView()
        {
            InitializeComponent();
            SetGraphFormatters();
        }

        private void SetGraphFormatters()
        {
            AllTimeParts_TimeAxis.LabelFormatter = TimeFormatter;
            AllTimeParts_CountAxis.LabelFormatter = QuantityFormatter;
        }
    }
}
