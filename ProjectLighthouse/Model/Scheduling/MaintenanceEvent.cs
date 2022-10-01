using SQLite;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Scheduling
{
    public class MaintenanceEvent
    {
        public static List<MaintenanceEvent> Where { get; internal set; }
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public string Lathe { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        [NotNull]
        public DateTime StartingDate { get; set; }
        public DateTime LastCompleted { get; set; }
        [NotNull]
        public int IntervalMonths { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}
