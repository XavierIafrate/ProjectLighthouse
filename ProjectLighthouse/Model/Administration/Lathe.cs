using Microsoft.Xaml.Behaviors;
using ProjectLighthouse.Model.Core;
using SQLite;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Administration
{
    public class Lathe : Machine
    {
        public string IPAddress { get; set; }
        public string ControllerReference { get; set; }
        public double MaxDiameter { get; set; }
        public double MaxLength { get; set; }
        public double PartOff { get; set; }
        public string Remarks { get; set; }

        [Ignore]
        public List<MaintenanceEvent> Maintenance { get; set; }
        [Ignore]
        public List<Attachment> Attachments { get; set; }
        [Ignore]
        public List<Attachment> ServiceRecords { get; set; }

        public override string ToString()
        {
            return FullName;
        }
    }
}
