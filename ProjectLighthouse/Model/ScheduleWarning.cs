using System;

namespace ProjectLighthouse.Model
{
    public class ScheduleWarning : ScheduleItem
    {
        public string WarningText { get; set; }
        public bool Important { get; set; }
    }
}
