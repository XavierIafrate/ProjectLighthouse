using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class WorkloadCalculationHelper
    {
        public static TimeSpan GetMachineWorkload(List<ScheduleItem> allItems)
        {
            double secondsOfRuntime = 0;


            List<ScheduleItem> items = allItems.Where(x => x is not ScheduleWarning && x.EndsAt() > DateTime.Now).OrderBy(x => x.StartDate).ToList();

            for (int i = 0; i < items.Count; i++)
            {
                ScheduleItem item = items[i];

                DateTime endsAt = item.EndsAt();

                if (item is LatheManufactureOrder order)
                {
                    endsAt = order.EndsAt();
                }

                if (i != items.Count - 1)
                {
                    ScheduleItem nextItem = items[i + 1];
                    if (endsAt.AddHours(18).Date >= nextItem.StartDate)
                    {
                        endsAt = nextItem.StartDate;
                    }
                }

                secondsOfRuntime += (endsAt - item.StartDate).TotalSeconds;
            }

            return TimeSpan.FromSeconds(secondsOfRuntime);
        }

        public static List<ScheduleItem> GetItemsForDayForMachine(List<ScheduleItem> items, DateTime day, string machineId)
        {
            List<ScheduleItem> result = new();

            List<ScheduleItem> possibleItems = items
                .Where(x => x.StartDate <= day.AddDays(1) && x.AllocatedMachine == machineId)
                .OrderByDescending(x => x.StartDate)
                .ToList();

            if (possibleItems.Count == 0)
            {
                return new();
            }

            for (int i = 0; i < possibleItems.Count; i++)
            {
                ScheduleItem item = possibleItems[i];


                DateTime plannedEndDate;

                if (item is LatheManufactureOrder order)
                {
                    plannedEndDate = order.IsResearch
                        ? (order.EndsAt() > order.StartDate.AddDays(2) ? order.EndsAt() : order.StartDate.AddDays(2))
                        : order.EndsAt(); // Must be adjusted time knowing the cycle time
                }
                else if (item is MachineService maint)
                {
                    plannedEndDate = maint.EndsAt();
                }
                else
                {
                    throw new Exception("Unexpected type");
                }

                if (plannedEndDate >= day)
                {
                    result.Add(item);
                }
                else
                {
                    break;
                }
            }

            result = result.OrderBy(x => x.StartDate).ToList();

            return result;
        }
    }
}
