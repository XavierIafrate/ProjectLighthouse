using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel
{
    public class RequestsEngine
    {
        public static List<LatheManufactureOrderItem> GetRecommendedOrderItems(List<TurnedProduct> turnedProducts, TurnedProduct requiredProduct, int qtyOfRequired, TimeSpan maxRuntime, DateTime? RequiredProductDueDate = null)
        {
            List<LatheManufactureOrderItem> unfilteredItems = new();
            if (RequiredProductDueDate != null)
            {
                unfilteredItems.Add(new(requiredProduct, qtyOfRequired) { DateRequired = (DateTime)RequiredProductDueDate });
            }
            else
            {
                unfilteredItems.Add(new(requiredProduct, qtyOfRequired));
            }

            turnedProducts = turnedProducts
            .Where(p => p.IsScheduleCompatible(requiredProduct) && Math.Abs(p.MajorLength - requiredProduct.MajorLength) <= 40)
            .OrderByDescending(p => p.GetRecommendedQuantity())
            .Take(3) // Max other products on order
            .ToList();

            foreach (TurnedProduct product in turnedProducts)
            {
                unfilteredItems.Add(new(product));
            }

            List<LatheManufactureOrderItem> filteredItems = CapQuantitiesForTimeSpan(unfilteredItems, maxRuntime);

            return filteredItems;
        }

        public static List<LatheManufactureOrderItem> CapQuantitiesForTimeSpan(List<LatheManufactureOrderItem> items, TimeSpan maxRuntime, bool regenerateTargets = false)
        {

            if (items == null)
            {
                return null;
            }
            List<LatheManufactureOrderItem> cleanedItems = new();
            double permittedSeconds = maxRuntime.TotalSeconds;

            foreach (LatheManufactureOrderItem item in items)
            {
                int possibleQuantity = GetQuantityPossible(permittedSeconds, item.CycleTime);

                if (regenerateTargets)
                {
                    item.TargetQuantity = item.RequiredQuantity + item.RecommendedQuantity;
                }

                if (item.TargetQuantity < 10)
                {
                    break;
                }

                if (possibleQuantity < item.RequiredQuantity) // requirement alone will max time
                {
                    item.TargetQuantity = item.RequiredQuantity;
                    cleanedItems.Add(item);
                    break;
                }
                else if (possibleQuantity < item.TargetQuantity) // Target needs to be capped
                {
                    item.TargetQuantity = RoundQuantity(possibleQuantity);
                    cleanedItems.Add(item);
                    break;
                }
                else
                {
                    cleanedItems.Add(item);
                }

                permittedSeconds -= item.CycleTime * item.TargetQuantity;

            }

            return cleanedItems;
        }

        private static int RoundQuantity(int originalNumber)
        {
            int divisor = originalNumber switch
            {
                > 15000 => 1000,
                > 5000 => 500,
                > 1000 => 200,
                > 100 => 50,
                > 10 => 10,
                _ => 1,
            };
            return originalNumber - (originalNumber % divisor);
        }

        private static int GetQuantityPossible(double secondsAllotted, int cycleTime)
        {
            if (cycleTime == 0)
            {
                cycleTime = 120;
            }

            double numPossible = secondsAllotted / Convert.ToDouble(cycleTime);
            return (int)Math.Floor(numPossible);
        }
    }
}
