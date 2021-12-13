using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class WorkloadCalculationHelper
    {
        public static Tuple<TimeSpan, DateTime> GetMachineWorkload(List<CompleteOrder> orders)
        {
            double secondsOfRuntime = 0;
            DateTime lastOrderFinished = DateTime.MinValue;

            orders = orders.OrderBy(x => x.Order.StartDate).ToList(); //otherwise bad things happen

            for (int i = 0; i < orders.Count; i++)
            {
                LatheManufactureOrder order = orders[i].Order;
                List<LatheManufactureOrderItem> items = orders[i].OrderItems;

                if (order.State == OrderState.Running || order.Status == "Running")
                {
                    foreach (LatheManufactureOrderItem item in items)
                    {
                        secondsOfRuntime += Math.Max(item.TargetQuantity - item.QuantityMade, 0) * item.CycleTime;
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        lastOrderFinished = DateTime.Now;
                    }

                    secondsOfRuntime += Math.Abs((order.StartDate - lastOrderFinished).TotalSeconds);
                    secondsOfRuntime += order.TimeToComplete;
                    secondsOfRuntime += 86400 / 2; // Setting Time
                }

                if (i == 0)
                {
                    lastOrderFinished = DateTime.Now.AddSeconds(secondsOfRuntime);
                }
                else
                {
                    lastOrderFinished = order.StartDate.AddSeconds(order.TimeToComplete);
                }

            }


            return new(lastOrderFinished - DateTime.Now, lastOrderFinished);
        }

    }
}
