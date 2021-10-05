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
        public int Priority { get; set; }

        public BarStockRequirementOverview(BarStock barStock, List<LatheManufactureOrder> orders)
        {
            BarStock = barStock;
            Orders = orders;

            foreach (LatheManufactureOrder order in orders)
            {
                BarsRequiredForOrders += order.NumberOfBars;
            }

            FreeBar = BarStock.InStock + BarStock.OnOrder - BarsRequiredForOrders;

            if (FreeBar >= 0)
            {
                
                if (BarStock.InStock < BarsRequiredForOrders)
                {
                    Priority = 1; // Awaiting stock
                }
                else
                {
                    Priority = 2; // all good
                }

            }
            else
            {
                Priority = 0; // Need to buy
            }
        }

        public override string ToString()
        {
            return BarStock.Id;
        }
    }
}
