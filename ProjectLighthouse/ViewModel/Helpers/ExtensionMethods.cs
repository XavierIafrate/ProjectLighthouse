using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ViewModel.Helpers
{
    public static class ExtensionMethods
    {
        public static (int, int, bool) CalculateOrderRuntime(this List<LatheManufactureOrderItem> items, int? defaultCycleTime = null)
        {
            // TODO: Setting time

            List<LatheManufactureOrderItem> orderedItems = items.OrderByDescending(x => x.CycleTime).ToList();
            bool estimated = false;
            int cycleTime = 0;

            int orderTime = 0;

            for (int i = 0; i < orderedItems.Count; i++)
            {
                if (orderedItems[i].CycleTime > 0)
                {
                    orderTime += orderedItems[i].CycleTime * orderedItems[i].TargetQuantity;
                }
                else if (cycleTime > 0)
                {
                    orderTime += cycleTime * orderedItems[i].TargetQuantity;
                }
                else if (defaultCycleTime is not null)
                {
                    orderTime += (int)defaultCycleTime * orderedItems[i].TargetQuantity;
                    estimated = true;
                }
                else
                {
                    orderTime += orderedItems[i].GetCycleTime() * orderedItems[i].TargetQuantity;
                    estimated = true;
                }

                cycleTime = orderedItems[i].CycleTime;
            }

            return new(orderTime, cycleTime, estimated);
        }

        public static (int, int, bool) CalculateOrderRuntime(this ObservableCollection<LatheManufactureOrderItem> items, int? defaultCycleTime = null)
        {
            return items.ToList().CalculateOrderRuntime(defaultCycleTime);
        }

        public static double CalculateNumberOfBars(this List<LatheManufactureOrderItem> items, BarStock bar, int spareBars, double partOff = 2)
        {
            double numberOfBars = 0;

            for (int i = 0; i < items.Count; i++)
            {
                LatheManufactureOrderItem item = items[i];
                double partsPerBar = Math.Floor((bar.Length - 300) / (item.MajorLength + item.PartOffLength + partOff));
                numberOfBars += item.TargetQuantity / partsPerBar;
            }

            return Math.Ceiling(numberOfBars * 1.02) + spareBars;
        }

        public static double CalculateNumberOfBars(this ObservableCollection<LatheManufactureOrderItem> items, BarStock bar, int spareBars, double partOff = 2)
        {
            return items.ToList().CalculateNumberOfBars(bar, spareBars, partOff);
        }
    }
}
