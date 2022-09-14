using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.Model
{
    public class BarStockRequirementOverview
    {
        public BarStock BarStock { get; set; }
        public List<LatheManufactureOrder> Orders { get; set; }
        public double BarsRequiredForOrders { get; set; }
        public double FreeBar { get; set; }
        public int Priority { get; set; }


        public BarStockRequirementOverview(BarStock barStock, List<LatheManufactureOrder> orders)
        {
            BarStock = barStock;
            Orders = orders;

            BarsRequiredForOrders = orders.Sum(x => x.NumberOfBars) - orders.Sum(x => x.NumberOfBarsIssued);

            FreeBar = BarStock.InStock + BarStock.OnOrder - BarsRequiredForOrders;

            if (FreeBar >= 0)
            {

                if (BarStock.InStock < BarsRequiredForOrders)
                {
                    Priority = 1; // Awaiting stock
                }
                else
                {
                    Priority = BarsRequiredForOrders > 0 ? 2 : 3; // all good : No Dependent Orders
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
