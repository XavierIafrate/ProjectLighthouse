﻿using ProjectLighthouse.Model;
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
                LatheManufactureOrderItem newItem = new(product);
                unfilteredItems.Add(newItem);
            }

            List<LatheManufactureOrderItem> filteredItems = CapQuantitiesForTimeSpan(unfilteredItems, maxRuntime, enforceMOQs:true);

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

                int cycleTime = item.CycleTime == 0 ? 120 : item.CycleTime;
                permittedSeconds -= cycleTime * item.TargetQuantity;

            }

            return cleanedItems;
        }

        private static int GetMiniumumOrderQuantity(LatheManufactureOrderItem item)
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