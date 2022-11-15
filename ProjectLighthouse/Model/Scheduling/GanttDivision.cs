using ProjectLighthouse.Model.Scheduling;
using System.Collections.Generic;

namespace Model.Scheduling
{
    public class GanttDivision
    {
        public string Title { get; set; }
        public List<ScheduleItem> Events { get; set; }
    }
}
