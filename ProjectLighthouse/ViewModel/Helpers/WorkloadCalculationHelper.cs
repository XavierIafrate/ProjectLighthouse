using ProjectLighthouse.Model;
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

            items = items.OrderBy(x => x.StartDate).ToList(); //otherwise bad things happen

            for (int i = 0; i < items.Count; i++)
            {
                ScheduleItem item = items[i];

                if (item is LatheManufactureOrder)
                {
                    LatheManufactureOrder order = item as LatheManufactureOrder;

                    if (order.State == OrderState.Running)
                    {
                        foreach (LatheManufactureOrderItem orderItem in order.OrderItems)
                        {
                            secondsOfRuntime += Math.Max(orderItem.TargetQuantity - orderItem.QuantityMade, 0) * orderItem.CycleTime;
                        }
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        lastItemFinished = DateTime.Now;
                    }

                    secondsOfRuntime += Math.Abs((item.StartDate - lastItemFinished).TotalSeconds);
                    secondsOfRuntime += item.TimeToComplete;
                    secondsOfRuntime += 86400 / 2; // Setting Time
                }

                if (i == 0)
                {
                    lastItemFinished = DateTime.Now.AddSeconds(secondsOfRuntime);
                }
                else
                {
                    lastItemFinished = item.StartDate.AddSeconds(item.TimeToComplete);
                }

            }


            return new(lastItemFinished - DateTime.Now, lastItemFinished);
        }

    }
}
