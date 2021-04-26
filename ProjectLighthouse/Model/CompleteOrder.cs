using System.Collections.ObjectModel;

namespace ProjectLighthouse.Model
{
    public class CompleteOrder
    {
        public LatheManufactureOrder Order { get; set; }
        public ObservableCollection<LatheManufactureOrderItem> OrderItems { get; set; }
    }
}
