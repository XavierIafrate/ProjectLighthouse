using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel
{
    public class RequestsEngine
    {
        public static Tuple<List<TurnedProduct>, TimeSpan> GetRecommendedOrderQuantities(List<TurnedProduct> turnedProducts, TurnedProduct requiredProduct, int qtyOfRequired, TimeSpan maxRuntime)
        {
            List<TurnedProduct> result = new();
            TimeSpan currentRuntime = new();
            result.Add(requiredProduct);

            turnedProducts = turnedProducts
                .Where(p => p.IsScheduleCompatible(requiredProduct) && Math.Abs(p.MajorLength - requiredProduct.MajorLength) <= 30)
                .OrderByDescending(p => p.GetRecommendedQuantity())
                .ToList();

            
            currentRuntime = requiredProduct.GetTimeToMake(qtyOfRequired);
            

            foreach (TurnedProduct product in turnedProducts)
            {
                double secondsLeftToAllocate = (maxRuntime - currentRuntime).TotalSeconds;

                if (secondsLeftToAllocate <= 0)
                {
                    return new(result, currentRuntime);
                }

                
            }

            return new(result, currentRuntime);
        }

        private static int GetQuantityToMakeInTimeSpan(TurnedProduct product, TimeSpan allottedTime)
        {
            int qtyForStock = product.GetRecommendedQuantity();
            TimeSpan timeToMakeStock = product.GetTimeToMake(qtyForStock);

            if (timeToMakeStock <= allottedTime)
            {
                return qtyForStock;
            }
            else
            {
                return 0;
            }
        }
    }
}
