using System.Collections.Generic;

namespace Model.Scheduling
{
    public class GanttData
    {
        public string Title { get; set; } = "Header";
        public List<GanttDivision> Data { get; set; } = new();
    }
}
