using ProjectLighthouse.Model.Orders;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Scheduling
{
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public List<ScheduleItem> Orders { get; set; }
        public List<LatheManufactureOrderItem> OrderItems { get; set; }

        public CalendarDay(DateTime date, List<ScheduleItem> orders)
        {
            Date = date;
            if (orders != null)
            {
                Orders = orders;
            }
        }

        public override string ToString()
        {
            return $"{Date:s}";
        }
    }
}
