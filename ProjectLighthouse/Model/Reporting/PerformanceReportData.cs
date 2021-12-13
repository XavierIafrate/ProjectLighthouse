using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Reporting
{
    public class PerformanceReportData
    {
        public DateTime FromDate    { get; set; }
        public DateTime ToDate { get; set; }
        public List<DailyPerformance> Days { get; set; }
    }
}
