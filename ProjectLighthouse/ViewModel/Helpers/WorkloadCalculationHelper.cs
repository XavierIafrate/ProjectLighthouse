using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class WorkloadCalculationHelper
    {
        public static Tuple<TimeSpan, DateTime> GetMachineWorkload(List<ScheduleItem> items)
        {
            double secondsOfRuntime = 0;
            DateTime lastItemFinished = DateTime.MinValue;

            items = items.OrderBy(x => x.StartDate).ToList();

            for (int i = 0; i < items.Count; i++)
            {
                ScheduleItem item = items[i];

                if (item is LatheManufactureOrder order)
                {
                    secondsOfRuntime += GetTimeToMakeOrder(order, includeSetting: true);
                }
                else
                {
                    if (i == 0)
                    {
                        lastItemFinished = DateTime.Now;
                    }

                    secondsOfRuntime += item.TimeToComplete;
                }

                if (i == 0)
                {
                    lastItemFinished = DateTime.Now.AddSeconds(secondsOfRuntime).AddDays(7);
                }
                else
                {
                    lastItemFinished = item.StartDate.AddSeconds(item.TimeToComplete).AddDays(7);
                }
            }

            return new(TimeSpan.FromSeconds(secondsOfRuntime), lastItemFinished);
        }

        public static int GetTimeToMakeOrder(LatheManufactureOrder order, bool includeSetting)
        {
            int runtime = includeSetting ? 86400 / 2 : 0;

            for (int i = 0; i < order.OrderItems.Count; i++)
            {
                LatheManufactureOrderItem item = order.OrderItems[i];
                runtime += item.GetCycleTime() * Math.Max(item.TargetQuantity - item.QuantityMade, 0);
            }

            return runtime;
        }

    }
}
