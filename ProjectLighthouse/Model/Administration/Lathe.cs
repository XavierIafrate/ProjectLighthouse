using SQLite;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Administration
{
    public class Lathe
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string FullName { get; set; }
        public string SerialNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string IPAddress { get; set; }
        public string ControllerReference { get; set; }
        public double MaxDiameter { get; set; }
        public double MaxLength { get; set; }
        public double PartOff { get; set; }
        public bool OutOfService { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        [Ignore]
        public List<MaintenanceEvent> Maintenance { get; set; }

        public override string ToString()
        {
            return FullName;
        }
    }
}
