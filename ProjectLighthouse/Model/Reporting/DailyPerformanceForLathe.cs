using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Analytics;
using ProjectLighthouse.Model.Orders;

namespace ProjectLighthouse.Model.Reporting
{
    public class DailyPerformanceForLathe
    {
        public Lathe Lathe { get; set; }
        public Lot[] Lots { get; set; }
        public MachineOperatingBlock[] OperatingBlocks { get; set; }
        public LatheManufactureOrder[] Orders { get; set; }
    }
}
