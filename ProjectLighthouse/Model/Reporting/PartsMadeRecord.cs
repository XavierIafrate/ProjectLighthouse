using System;

namespace ProjectLighthouse.Model.Reporting
{
    public class PartsMadeRecord
    {
        public DateTime Date;
        public string ProductName;
        public string BarID;
        public double BarCost;
        public string OrderID;
        public int QuantityProduced;
        public int QuantityRejected;
        public int CycleTime;
        public int PartsPerBar;
        public string Material;
    }
}
