﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public List<LatheManufactureOrder> Orders { get; set; }
        public List<LatheManufactureOrderItem> OrderItems { get; set; }

        public CalendarDay(DateTime date, List<LatheManufactureOrder> orders)
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