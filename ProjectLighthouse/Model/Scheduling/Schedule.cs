using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.Model.Scheduling
{
    public class Schedule
    {
        Dictionary<string, List<ScheduleItem>> Machines { get; set; }

        public Schedule(List<ScheduleItem> items)
        {
            Machines = new();
            BuildSchedules(items);
        }

        private void BuildSchedules(List<ScheduleItem> items)
        {
            string[] machines = items.Select(x => x.AllocatedMachine).ToArray();
            for (int i = 0; i < machines.Length; i++)
            {
                List<ScheduleItem> itemsOnMachine = items
                    .Where(x => x.AllocatedMachine == machines[i])
                    .OrderBy(x => x.StartDate)
                    .ToList();

                Machines.Add(machines[i], itemsOnMachine);
            }
        }

        public static TimeSpan GetScheduleLoss(List<ScheduleItem> items, DateTime start, DateTime end)
        {
            TimeSpan booked = new();
            List<ScheduleItem> orderedItems = items
                .Where(x => x.EndsAt() > start && x.StartDate < end)
                .OrderBy(x => x.StartDate)
                .ToList();

            DateTime? anchor = null;
            DateTime? cursor = null;

            foreach (ScheduleItem item in orderedItems)
            {
                DateTime itemEnds = item.EndsAt();

                anchor = item.StartDate.AddHours(-6);

                if (cursor is null)
                {
                    cursor = itemEnds;
                }
                else if (anchor < cursor)
                {
                    continue;
                }

                cursor = itemEnds;

                List<ScheduleItem> extendingItems = orderedItems
                    .Where(x => x.StartDate.AddHours(-6) > anchor && x.StartDate.AddHours(-6) < cursor)
                    .ToList();
                int count = 0;

                while (extendingItems.Count != count)
                {
                    cursor = extendingItems.Max(x => x.EndsAt());
                    count = extendingItems.Count;

                    extendingItems = orderedItems
                        .Where(x => x.StartDate.AddHours(-6) < cursor && x.EndsAt() > anchor)
                        .ToList();


                }

                booked = booked.Add((TimeSpan)((cursor > end ? end : cursor) - (anchor < start ? start : anchor)));
                anchor = cursor;
            }

            TimeSpan result = (end - start) - booked;
            return result;
        }
    }
}
