using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    public class CompleteOrder
    {
        public LatheManufactureOrder Order { get; set; }
        public ObservableCollection<LatheManufactureOrderItem> OrderItems { get; set; }
    }
}
