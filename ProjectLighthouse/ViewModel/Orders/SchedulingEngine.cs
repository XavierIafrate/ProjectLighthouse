using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Orders
{
    public class SchedulingEngine
    {
        public List<LatheManufactureOrder> Orders { get; set; }
        public List<Lathe> Lathes { get; set; }
        public List<BarStock> BarStock { get; set; }
        public List<MachineSchedule> MachineSchedules { get; set; }

        public List<ScheduleThread> Threads { get; set; }

        public void MakeThreads()
        {
            Threads = new();
            Orders = Orders.OrderBy(x => x.Bar.Size).ToList();

            for (int i = 0; i < Orders.Count; i++)
            {
                Orders[i].Deadline = Orders[i].GetStartDeadline();
            }

            IEnumerable<IGrouping<double, LatheManufactureOrder>> groups = Orders.GroupBy(x => x.Bar.Size);
            Random random = new();
            for (int i = 0; i < groups.Count(); i++)
            {
                int newKey = random.Next(1, 256);

                while (Threads.Any(x => x.Key == newKey))
                {
                    newKey = random.Next(1, 256);
                }

                ScheduleThread newThread = new(newKey, groups.ElementAt(i).ToList());

                List<LatheManufactureOrder> activeOrdersOnNewThread = newThread.Orders.Where(x => x.State == OrderState.Running).ToList();

                // Make sub function
                for (int j = 1; j < activeOrdersOnNewThread.Count; j++)
                {
                    newThread.Orders.Remove(activeOrdersOnNewThread[j]);
                    int extraKey = random.Next(1, 256);
                    while (Threads.Any(x => x.Key == newKey))
                    {
                        extraKey = random.Next(1, 256);
                    }
                    List<LatheManufactureOrder> extraThreadOrders = new();
                    extraThreadOrders.Add(activeOrdersOnNewThread[j]);
                    ScheduleThread extraThread = new(extraKey, extraThreadOrders);
                    Threads.Add(extraThread);
                }

                Threads.Add(newThread);
            }
        }



        public void CategoriseThreads(bool SoftLimits = true)
        {
            for (int i = 0; i < Lathes.Count; i++)
            {
                Lathe currLathe = Lathes[i];
                for (int j = 0; j < Threads.Count; j++)
                {
                    CategoriseThread(Threads[j], currLathe, SoftLimits);
                }
            }
        }

        private void CategoriseThread(ScheduleThread thread, Lathe lathe, bool softlimit)
        {
            int min = softlimit ? lathe.SoftMinDiameter : 0;
            int max = softlimit ? lathe.SoftMaxDiameter : lathe.MaxDiameter;
            if (thread.Bar.Size >= min && thread.Bar.Size <= max)
            {
                thread.AvailableToMachines.Add(lathe);
            }
        }

        public void CreateBlankSchedule()
        {
            MachineSchedules = new();
            for (int i = 0; i < Lathes.Count; i++)
            {
                MachineSchedule machine = new() { Lathe = Lathes[i], Threads = new() };

                ScheduleThread activeThread = null;

                foreach (ScheduleThread thread in Threads)
                {
                    if (thread.Orders.Any(x => x.State == OrderState.Running && x.AllocatedMachine == Lathes[i].Id))
                    {
                        activeThread = thread;
                        activeThread.StartDate = thread.Orders.Min(x => x.StartDate);
                        break;
                    }

                }
                if (activeThread != null)
                {
                    machine.Threads.Add(activeThread);
                }

                MachineSchedules.Add(machine);


            }
        }

        public void ScheduleRequirements()
        {
            List<ScheduleThread> threadsToSchedule = new(Threads);
            threadsToSchedule = threadsToSchedule.OrderByDescending(x => x.Deadline).ToList();

            for (int i = 0; i < MachineSchedules.Count; i++)
            {
                for (int j = 0; j < MachineSchedules[i].Threads.Count; j++)
                {
                    threadsToSchedule.Remove(MachineSchedules[i].Threads[j]);
                }
            }

            for (int i = 0; i < MachineSchedules.Count; i++)
            {
                List<ScheduleThread> available = threadsToSchedule
                    .Where(x => x.AvailableToMachines.Any(x => x.Model == MachineSchedules[i].Lathe.Model))
                    .ToList();
                for (int j = 0; j < available.Count; j++)
                {
                    MachineSchedules[i].AddThreadToSchedule(available[j], DateTime.Now);
                    threadsToSchedule.Remove(available[j]);
                }
            }
        }

        public class MachineSchedule
        {
            public Lathe Lathe { get; set; }
            public List<ScheduleThread> Threads { get; set; }

            public void AddThreadToSchedule(ScheduleThread thread, DateTime startDate)
            {
                thread.StartDate = startDate;
                Threads.Add(thread);
            }
        }

        public class ScheduleThread
        {
            public int Key { get; set; }
            public List<LatheManufactureOrder> Orders { get; set; }
            public DateTime Deadline { get; set; }
            public List<Lathe> AvailableToMachines { get; set; } = new();
            public DateTime StartDate { get; set; }
            public BarStock Bar { get; set; }

            public ScheduleThread(int key, List<LatheManufactureOrder> orders)
            {
                Key = key;
                Orders = orders;
                Bar = orders.First().Bar;
                Deadline = orders.Min(x => x.Deadline);
            }
        }
    }
}
