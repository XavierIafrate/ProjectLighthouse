using ProjectLighthouse.Model.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.Model.Material
{
    public class BarStockRequirementOverview
    {
        public BarStock BarStock { get; set; }
        public List<LatheManufactureOrder> Orders { get; set; }
        public double BarsRequiredForOrders { get; set; }
        public double FreeBar { get; set; }
        public StockStatus Status { get; set; }
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
                    Status = StockStatus.OnOrder;
                }
                else if(BarStock.InStock < (int)Math.Floor(BarStock.SuggestedStock * 0.1))
                {
                    Status = StockStatus.LowStock;
                }
                else
                {
                    Status = StockStatus.StockOk;
                }

            }
            else
            {
                Status = StockStatus.OrderNow;
            }

            // TODO flag orders without bar in time
        }

        public enum StockStatus
        {
            OrderNow = 1,
            OnOrder = 2,
            LowStock = 3,
            StockOk = 4,
        }

        public override string ToString()
        {
            return BarStock.Id;
        }
    }
}
