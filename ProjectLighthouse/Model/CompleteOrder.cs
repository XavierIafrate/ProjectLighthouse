using System.Collections.Generic;

namespace ProjectLighthouse.Model
{
    public class CompleteOrder
    {
        public LatheManufactureOrder Order { get; set; }
        public List<LatheManufactureOrderItem> OrderItems { get; set; }
    }
}
