using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public static class ExtensionMethods
    {

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
