using SQLite;

namespace ProjectLighthouse.Model.Assembly
{
    public class Routing
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string RoutingID { get; set; }
        public string Description { get; set; }
        public string Workstation { get; set; }
        public int SetupTime { get; set; }
        public int CycleTime { get; set; }
        public int QuantityPerCycle { get; set; }
    }
}
