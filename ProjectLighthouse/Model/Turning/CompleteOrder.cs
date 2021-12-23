using System.Collections.Generic;

namespace ProjectLighthouse.Model
{
    public class CompleteOrder
    {
        public LatheManufactureOrder Order { get; set; }
        public List<LatheManufactureOrderItem> OrderItems { get; set; }


        public CompleteOrder(LatheManufactureOrder order, List<LatheManufactureOrderItem> items)
        {
            Order = order;
            OrderItems = items;
        }

        public event System.Action EditMade;
        public void NotifyEditMade()
        {
            if (EditMade != null)
            {
                EditMade();
            }
        }
    }
}
