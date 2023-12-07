using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class OrderResourceHelper
    {
        public static int CalculateOrderRuntime(LatheManufactureOrder order, List<LatheManufactureOrderItem> items)
        {
            int totalTime = 0;

            foreach (LatheManufactureOrderItem item in items)
            {
                if (item.CycleTime > 0)
                {
                    totalTime += item.CycleTime * (item.TargetQuantity + item.QuantityReject); // factor in scrap time
                }
                else
                {
                    totalTime += item.PlannedCycleTime() * item.TargetQuantity;
                }
            }

            totalTime += (int)order.NumberOfBars * 30;

            return totalTime;
        }

        public static TimeModel GetCycleResponse(List<LatheManufactureOrderItem> items)
        {
            List<(double, int)> values = new();

            foreach (LatheManufactureOrderItem item in items)
            {
                if (item.CycleTime == 0) continue;
                values.Add((item.MajorLength, item.CycleTime));
            }

            try
            {
                return GetCycleResponse(values);
            }
            catch
            {
                throw;
            }
        }

        public static TimeModel GetCycleResponse(List<TurnedProduct> products)
        {
            List<(double, int)> values = new();

            foreach (TurnedProduct product in products)
            {
                if (product.CycleTime == 0) continue;
                values.Add((product.MajorLength, product.CycleTime));
            }

            try
            {
                return GetCycleResponse(values);
            }
            catch
            {
                throw;
            }
        }

        private static List<(double, int)> RemoveTail(List<(double, int)> items)
        {
            List<(double, int)> newItems = new();

            if (items.Count <= 2)
            {
                return items;
            }

            if (items[0].Item2 > items[1].Item2)
            {
                return items;
            }

            int initialValue = items.First().Item2;
            bool latch = false;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Item2 > initialValue)
                {
                    if (!latch)
                    {
                        newItems.Add(items[i-1]);
                    }

                    latch = true;
                    newItems.Add(items[i]);
                }
            }

            return newItems;
        }


        // TODO calculate flat start
        private static TimeModel GetCycleResponse(List<(double, int)> items)
        {
            items = items.Where(x => x.Item2 > 0).OrderBy(x => x.Item1).ToList();

            if (items.Count == 0)
            {
                throw new Exception("Not enough data");
            }

            if (items.All(x => x.Item2 == items.First().Item2))
            {
                return new() { Intercept = items.First().Item2, Gradient = 0, Floor = items.First().Item2, RecordCount = items.Count, CoefficientOfDetermination = 1 };
            }

            items = RemoveTail(items);

            int numPoints = items.Count;


            if (numPoints == 0)
            {
                throw new Exception("Not enough data");
            }

            int min = items.Min(x => x.Item2);

            double meanX = items.Average(point => point.Item1);
            double meanY = items.Average(point => (double)point.Item2);

            double sumXSquared = items.Sum(point => Math.Pow(point.Item1, 2));
            double sumXY = items.Sum(point => point.Item1 * (double)point.Item2);


            double a1 = (sumXY / numPoints - meanX * meanY) / (sumXSquared / numPoints - meanX * meanX);

            // no decrease
            if (a1 < 0 || double.IsNaN(a1))
            {
                // TODO Handle capped gradient properly
                return new TimeModel() { Intercept = meanY, Gradient = 0, Floor = min, RecordCount=0, CoefficientOfDetermination=0 };
            }

            double b1 = (meanY - a1 * meanX);

            double r2;

            double rss = 0;
            double tss = 0;
            double minLength = items.Min(x => x.Item1);

            foreach ((double, int) item in items)
            {
                double bestFitVal = a1 * item.Item1 + b1;
                rss += Math.Pow((double)item.Item2 - bestFitVal, 2);
                tss += Math.Pow((double)item.Item2 - meanY, 2);
            }
            
            r2 = 1 - (rss / tss);

            r2 = double.IsNaN(r2) ? 1 : r2;

            TimeModel model = new() { Intercept = b1, Gradient = a1, Floor = min, RecordCount = numPoints, CoefficientOfDetermination = r2 };

            return model;
        }
    }
}
