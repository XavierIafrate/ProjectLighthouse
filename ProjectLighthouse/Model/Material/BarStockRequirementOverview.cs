using System.Collections.Generic;
using System.Linq;
using ProjectLighthouse.Model.Orders;

namespace ProjectLighthouse.Model.Material
{
    public class BarStockRequirementOverview
    {
        public BarStock BarStock { get; set; }
        public List<LatheManufactureOrder> Orders { get; set; }
        public double BarsRequiredForOrders { get; set; }
        public double FreeBar { get; set; }
        public int Priority { get; set; }
        public bool UrgentProblem { get; set; }
        public bool IsHexagon { get; set; }
        public bool IsDormant { get; set; }

        public BarStockRequirementOverview(BarStock barStock, List<LatheManufactureOrder> orders)
        {
            BarStock = barStock;
            Orders = orders;

            BarsRequiredForOrders = System.Math.Max(orders.Sum(x => x.NumberOfBars) - orders.Sum(x => x.NumberOfBarsIssued), 0);
            IsHexagon = BarStock.IsHexagon;
            IsDormant = BarStock.IsDormant;

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

            // TODO flag orders without bar in time
        }

        public override string ToString()
        {
            return BarStock.Id;
        }
    }
}
