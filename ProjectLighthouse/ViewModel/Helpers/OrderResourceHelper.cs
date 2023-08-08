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
                    totalTime += item.CycleTime * item.TargetQuantity;
                }
                else
                {
                    totalTime += item.PlannedCycleTime() * item.TargetQuantity;
                }
            }

            return totalTime;
        }

        public static (TimeModel, double) GetCycleResponse(List<LatheManufactureOrderItem> items)
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

        public static (TimeModel, double) GetCycleResponse(List<TurnedProduct> products)
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

        private static (TimeModel, double) GetCycleResponse(List<(double, int)> items)
        {
            int numPoints = items.Count;

            if (numPoints == 0)
            {
                throw new Exception("Not enough data");
            }

            items = items.Where(x => x.Item2 > 0).OrderBy(x => x.Item1).ToList();
            int min = items.Min(x => x.Item2);

            double meanX = items.Average(point => point.Item1);
            double meanY = items.Average(point => (double)point.Item2);

            double sumXSquared = items.Sum(point => Math.Pow(point.Item1, 2));
            double sumXY = items.Sum(point => point.Item1 * (double)point.Item2);


            double a1 = (sumXY / numPoints - meanX * meanY) / (sumXSquared / numPoints - meanX * meanX);

            // no decrease
            if (a1 < 0 || double.IsNaN(a1))
            {
                return (new TimeModel() { Intercept = meanY, Gradient = 0, Floor = min }, 0);
            }

            double b1 = (meanY - a1 * meanX);

            double r2;

            double rss = 0;
            double tss = 0;
            double minLength = items.Min(x => x.Item1);

            foreach ((double, int) item in items)
            {
                double bestFitVal = a1 * (item.Item1 - minLength) + b1;
                rss += Math.Pow((double)item.Item2 - bestFitVal, 2);
                tss += Math.Pow((double)item.Item2 - meanY, 2);
            }

            r2 = 1 - (rss / tss);

            TimeModel model = new() { Intercept = b1, Gradient = a1, Floor = min };

            return (model, r2);
        }
    }
}
