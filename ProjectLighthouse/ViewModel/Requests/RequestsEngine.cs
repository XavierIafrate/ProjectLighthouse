using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Requests
{
    public class RequestsEngine
    {
        public static List<LatheManufactureOrderItem> GetRecommendedOrderItems(List<TurnedProduct> turnedProducts, TurnedProduct requiredProduct, int qtyOfRequired, TimeSpan maxRuntime, DateTime? RequiredProductDueDate = null, int numberOfItems = 4, bool enforceMOQ = true)
        {
            List<LatheManufactureOrderItem> recommendedItems = new();
            if (RequiredProductDueDate != null)
            {
                recommendedItems.Add(new(requiredProduct, qtyOfRequired) { DateRequired = (DateTime)RequiredProductDueDate });
            }
            else
            {
                recommendedItems.Add(new(requiredProduct, qtyOfRequired));
            }

            List<TurnedProduct> compatibleProducts = new();

            compatibleProducts.AddRange(turnedProducts
                .Where(p => p.IsScheduleCompatible(requiredProduct)
                    && !p.IsSpecialPart
                    && Math.Abs(p.MajorLength - requiredProduct.MajorLength) <= 40
                    && p.Id != requiredProduct.Id)
                .OrderByDescending(p => p.GetRecommendedQuantity())
                .ThenBy(p => p.QuantityInStock)
                );


            foreach (TurnedProduct product in compatibleProducts)
            {
                LatheManufactureOrderItem newItem = new(product);
                recommendedItems.Add(newItem);
            }

            List<LatheManufactureOrderItem> filteredItems = CapQuantitiesForTimeSpan(recommendedItems, maxRuntime, enforceMOQs: enforceMOQ);

            filteredItems = filteredItems.Take(numberOfItems).ToList();

            return filteredItems;
        }

        public static List<LatheManufactureOrderItem> CapQuantitiesForTimeSpan(List<LatheManufactureOrderItem> items, TimeSpan maxRuntime, bool regenerateTargets = false, bool enforceMOQs = false)
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

                int minimumQuantity = GetMiniumumOrderQuantity(item);
                bool minimumQuantityEnforced = false;

                if (enforceMOQs && item.TargetQuantity < minimumQuantity)
                {
                    minimumQuantityEnforced = true;
                    item.TargetQuantity = minimumQuantity;
                }

                if (possibleQuantity < item.RequiredQuantity) // requirement alone will max time
                {
                    item.TargetQuantity = item.RequiredQuantity;
                    cleanedItems.Add(item);
                    break;
                }
                else if (enforceMOQs && minimumQuantity > possibleQuantity)
                {
                    break;
                }
                else if (possibleQuantity < item.TargetQuantity) // Target needs to be capped
                {
                    if (!minimumQuantityEnforced)
                    {
                        item.TargetQuantity = RoundQuantity(possibleQuantity);
                        cleanedItems.Add(item);
                    }

                    break;
                }
                else
                {
                    if (item.RequiredQuantity > 0)
                    {
                        item.TargetQuantity = RoundQuantity(item.TargetQuantity, true);
                    }
                    cleanedItems.Add(item);
                }

                permittedSeconds -= item.GetCycleTime() * item.TargetQuantity;
            }

            return cleanedItems;
        }

        public static int GetMiniumumOrderQuantity(LatheManufactureOrderItem item)
        {
            return item.MajorDiameter switch
            {
                > 35 => 10,
                > 30 => 50,
                > 25 => 100,
                > 20 => 200,
                > 15 => 300,
                _ => 500,
            };
        }

        public static int GetMiniumumOrderQuantity(TurnedProduct item)
        {
            return item.MajorDiameter switch
            {
                > 35 => 10,
                > 30 => 50,
                > 25 => 100,
                > 20 => 200,
                > 15 => 300,
                _ => 500,
            };
        }

        private static int RoundQuantity(int originalNumber, bool roundUp = false)
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

            if (roundUp)
            {
                originalNumber += divisor;
            }

            return originalNumber - originalNumber % divisor;
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

        public static List<TurnedProduct> PopulateInsightFields(List<TurnedProduct> products, List<LatheManufactureOrder> activeOrders, List<Request> recentlyDeclinedRequests)
        {
            for (int i = 0; i < products.Count; i++)
            {
                for (int j = 0; j < activeOrders.Count; j++)
                {
                    if (activeOrders[j].OrderItems.Any(x => x.ProductName == products[i].ProductName))
                    {
                        products[i].AppendableOrder = activeOrders[j];
                        products[i].LighthouseGuaranteedQuantity = activeOrders[j].OrderItems.Find(x => x.ProductName == products[i].ProductName).RequiredQuantity;
                    }
                    else if (activeOrders[j].GroupId == products[i].GroupId)
                    {
                        products[i].ZeroSetOrder = activeOrders[j];
                    }
                }

                if (products[i].ZeroSetOrder == null && products[i].AppendableOrder == null && recentlyDeclinedRequests.Any(x => x.Product == products[i].ProductName))
                {
                    products[i].DeclinedRequest = recentlyDeclinedRequests.OrderByDescending(x => x.LastModified).First(x => x.Product == products[i].ProductName);
                }
            }

            return products;
        }
    }
}
