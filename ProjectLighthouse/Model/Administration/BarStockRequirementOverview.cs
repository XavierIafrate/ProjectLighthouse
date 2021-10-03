using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    public class BarStockRequirementOverview
    {
        public BarStock BarStock;
        public List<LatheManufactureOrder> Orders;
        public double BarsRequiredForOrders { get; set; }
        public double FreeBar { get; set; }

        public BarStockRequirementOverview(BarStock barStock, List<LatheManufactureOrder> orders)
        {
            BarStock = barStock;
            Orders = orders;

            foreach (LatheManufactureOrder order in orders)
            {
                BarsRequiredForOrders += order.NumberOfBars;
            }

            FreeBar = BarStock.InStock + BarStock.OnOrder - BarsRequiredForOrders;
        }

        public override string ToString()
        {
            return BarStock.Id;
        }
    }
}
