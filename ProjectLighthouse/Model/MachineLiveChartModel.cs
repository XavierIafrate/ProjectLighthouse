using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    public class MachineLiveChartModel
    {
        public SeriesCollection series { get; set; }
        public string title { get; set; }
        public int partCounterAll { get; set; }
        public int partCounterTarget { get; set; }
        public DateTime dataReadAt { get; set; }
        public DateTime tick2 { get; set; }

        public Func<double, string> TimeFormatter = value => new DateTime((long) value).ToString("dd MMMM yyyy");
        public Func<double, string> QuantityFormatter = value => string.Format($"{value:#,##0}");
    }
}
