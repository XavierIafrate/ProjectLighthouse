using ProjectLighthouse.Model.Universal;
using System;

namespace ProjectLighthouse.Model.Reporting
{
    public class DailyPerformance
    {
        public DateTime Date { get; set; }
        public DailyPerformanceForLathe[] LathePerformance { get; set; }
        public Delivery[] Deliveries { get; set; }
    }
}
