using LiveCharts;
using System;

namespace ProjectLighthouse.Model.Analytics
{
    public class MachineLiveChartModel
    {
        public SeriesCollection Series { get; set; }
        public string Title { get; set; }
        public int PartCounterAll { get; set; }
        public int PartCounterTarget { get; set; }
        public DateTime DataReadAt { get; set; }
        public DateTime Tick2 { get; set; }

        public Func<double, string> TimeFormatter = value => new DateTime((long)value).ToString("dd MMMM yyyy");
        public Func<double, string> QuantityFormatter = value => $"{value:#,##0}";
    }
}
