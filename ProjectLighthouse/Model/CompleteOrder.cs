using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProjectLighthouse.Model
{
    public class CompleteOrder
    {
        public LatheManufactureOrder Order { get; set; }
        public List<LatheManufactureOrderItem> OrderItems { get; set; }
    }
}
