using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ProjectLighthouse.Model.Scheduling
{
    public partial class ProductionSchedule : BaseObject
    {
        private List<Lathe> lathes;
        public List<Lathe> Lathes
        {
            get { return lathes; }
            set
            {
                lathes = value;
                OnPropertyChanged();
            }
        }


        private List<ScheduleItem> scheduleItems = new();

        public event EventHandler OnHolidaysChanged;
        public List<DateTime> Holidays { get; set; } = new();


        private List<ScheduleItem> unallocatedItems = new();
        public List<ScheduleItem> UnallocatedItems
        {
            get { return unallocatedItems; }
            set
            {
                unallocatedItems = value;
                OnPropertyChanged();
            }
        }


        private List<MachineSchedule> machineSchedules;
        public List<MachineSchedule> MachineSchedules
        {
            get { return machineSchedules; }
            set
            {
                machineSchedules = value;
                OnPropertyChanged();
            }
        }


        private List<Optimisation> optimisations;
        public List<Optimisation> Optimisations
        {
            get { return optimisations; }
            set
            {
                optimisations = value;
                OnPropertyChanged();
            }
        }

        private List<Advisory> advisories;
        public List<Advisory> Advisories
        {
            get { return advisories; }
            set
            {
                advisories = value;
                OnPropertyChanged();
            }
        }

        private List<Warning> warnings;
        public List<Warning> Warnings
        {
            get { return warnings; }
            set
            {
                warnings = value;
                OnPropertyChanged();
            }
        }


        public ProductionSchedule(List<Lathe> lathes, List<ScheduleItem> scheduleItems, List<DateTime> holidays)
        {
            this.Holidays = holidays;

            this.Lathes = lathes.OrderBy(x => x.Id).ToList();
            List<ScheduleItem> itemsNotCancelled = new();
            foreach (ScheduleItem item in scheduleItems)
            {
                if (item is LatheManufactureOrder order)
                {
                    if (order.State == OrderState.Cancelled) continue;
                }

                itemsNotCancelled.Add(item);
            }

            this.scheduleItems = itemsNotCancelled;

            BuildSchedule();
        }

        private void BuildSchedule()
        {
            List<MachineSchedule> mcSchedules = new();
            for (int i = 0; i < this.Lathes.Count; i++)
            {
                MachineSchedule newSchedule = new(this.Lathes[i])
                {
                    Holidays = this.Holidays
                };

                newSchedule.SetScheduleItems(this.scheduleItems);

                // Hide decomissioned lathes
                if (newSchedule.Lathe.OutOfService && newSchedule.ScheduleItems.Last().EndsAt() < DateTime.Today) continue;

                mcSchedules.Add(newSchedule);
            }

            MachineSchedules = mcSchedules;

            List<ScheduleItem> unscheduled = new();
            for (int i = 0; i < this.scheduleItems.Count; i++)
            {
                ScheduleItem item = this.scheduleItems[i];
                if (item is LatheManufactureOrder order)
                {
                    if (string.IsNullOrEmpty(order.AllocatedMachine) && !order.IsCancelled)
                    {
                        unscheduled.Add(item);
                    }
                }
                else if (item.StartDate == DateTime.MinValue)
                {
                    unscheduled.Add(item);
                }
            }

            this.UnallocatedItems = unscheduled;

            GetOptimisations();
            GetAdvisories();
            GetWarnings();
        }

        internal ScheduleItem? Find(string searchString)
        {
            string sanitisedString = searchString.ToUpperInvariant().Trim();

            ScheduleItem? result = null;

            for (int i = 0; i < MachineSchedules.Count; i++)
            {
                result = MachineSchedules[i].ScheduleItems.FindLast(x => x.Name.ToUpperInvariant().Contains(sanitisedString));

                if (result is not null)
                {
                    if (result.StartDate.Date == DateTime.MinValue.Date)
                    {
                        result = null; // protect against bad data
                    }

                    return result;
                }
            }

            return result;
        }

        public void RescheduleItem(RescheduleInformation changeData)
        {
            MachineSchedule? sourceSchedule = MachineSchedules.Find(x => x.Lathe.Id == changeData.originMachineId);
            MachineSchedule? destinationSchedule = MachineSchedules.Find(x => x.Lathe.Id == changeData.desiredMachineId);

            if (!string.IsNullOrEmpty(changeData.desiredMachineId) && destinationSchedule == null)
            {
                throw new Exception($"Could not find destination Machine Schedule");
            }

            ScheduleItem? foundItem = null;

            if (sourceSchedule != null)
            {
                foundItem = sourceSchedule.ScheduleItems.Find(x => x == changeData.item);

                if (foundItem == null)
                {
                    throw new ArgumentException($"Could not find {changeData.item.Name} on schedule for {sourceSchedule.Lathe.Id}.");
                }
            }
            else
            {
                foundItem = UnallocatedItems.Find(x => x == changeData.item);

                if (foundItem == null)
                {
                    throw new ArgumentException($"Could not find {changeData.item.Name} in schedule or unallocated items.");
                }
            }

            if (sourceSchedule != null && destinationSchedule != null) // On machine to new slot
            {
                if (sourceSchedule != destinationSchedule)
                {
                    foundItem.StartDate = (DateTime)changeData.desiredDate!;
                    foundItem.AllocatedMachine = changeData.desiredMachineId;

                    foundItem.UpdateStartDate((DateTime)changeData.desiredDate!, changeData.desiredMachineId);


                    sourceSchedule.Remove(foundItem);
                    sourceSchedule.Refresh();

                    destinationSchedule.Add(foundItem);
                    destinationSchedule.Refresh();
                }
                else
                {
                    foundItem.StartDate = (DateTime)changeData.desiredDate!;
                    foundItem.UpdateStartDate((DateTime)changeData.desiredDate!, changeData.desiredMachineId);

                    sourceSchedule.Refresh();
                }
            }
            else if (destinationSchedule == null) // from machine to unallocated
            {
                sourceSchedule!.Remove(foundItem);
                sourceSchedule.Refresh();

                foundItem.StartDate = DateTime.MinValue;
                foundItem.AllocatedMachine = null;
                foundItem.UpdateStartDate(changeData.desiredDate ?? DateTime.MinValue, changeData.desiredMachineId);


                List<ScheduleItem> unallocated = this.UnallocatedItems;
                unallocated.Add(foundItem);
                UnallocatedItems = null;
                UnallocatedItems = unallocated;
            }
            else // from unallocated to machine
            {
                List<ScheduleItem> unallocated = this.UnallocatedItems;
                unallocated.Remove(foundItem);
                UnallocatedItems = null;
                UnallocatedItems = unallocated;

                foundItem.StartDate = (DateTime)changeData.desiredDate!;
                foundItem.AllocatedMachine = changeData.desiredMachineId;
                foundItem.UpdateStartDate((DateTime)changeData.desiredDate!, changeData.desiredMachineId);

                destinationSchedule.Add(foundItem);
                destinationSchedule.Refresh();
            }


            GetOptimisations();
            GetAdvisories();
            GetWarnings();
        }

        internal bool AddHoliday(DateTime selection)
        {
            List<DateTime> holidays = Holidays.Append(selection.ChangeTime(0, 0, 0, 0)).OrderBy(x => x).Distinct().ToList();

            try
            {
                SaveHolidays(holidays);
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
                return false;
            }

            Holidays = holidays;
            OnHolidaysChanged?.Invoke(this, EventArgs.Empty);
            foreach (MachineSchedule machineSchedule in MachineSchedules)
            {
                machineSchedule.SetHolidays(Holidays);
            }
            GetWarnings();
            return true;
        }

        internal bool RemoveHoliday(DateTime selection)
        {
            List<DateTime> holidays = Holidays.ToList();
            if (!holidays.Remove(selection.ChangeTime(0,0,0,0)))
            {
                MessageBox.Show("Could not find date in list of holidays");
                return false;
            }

            try
            {
                SaveHolidays(holidays);
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
                return false;
            }

            Holidays = holidays;
            OnHolidaysChanged?.Invoke(this, EventArgs.Empty);
            foreach(MachineSchedule machineSchedule in MachineSchedules) 
            {
                machineSchedule.SetHolidays(Holidays);
            }
            GetWarnings();
            return true;
        }


        private static void SaveHolidays(List<DateTime> holidays)
        {
            try
            {
                string data = Newtonsoft.Json.JsonConvert.SerializeObject(holidays);
                File.WriteAllText(App.ROOT_PATH + @"lib\holidays.json", data);
            }
            catch
            {
                throw;
            }
        }

        private void GetOptimisations()
        {
            List<Optimisation> optimisations = new();
            List<LatheManufactureOrder> activeOrders = new();

            foreach (ScheduleItem item in scheduleItems)
            {
                if (item is not LatheManufactureOrder order) continue;
                if (order.State >= OrderState.Complete) continue;

                activeOrders.Add(order);
            }

            List<LatheManufactureOrder> checkedActiveOrders = new();

            foreach (LatheManufactureOrder order in activeOrders)
            {
                List<LatheManufactureOrder> toCompare = activeOrders.Where(x => !checkedActiveOrders.Contains(x) && x.Id != order.Id).ToList();

                optimisations.AddRange(GetOppurtunitiesForOrder(order, toCompare));

                checkedActiveOrders.Add(order);
            }

            List<Optimisation> implemented = new();
            foreach (MachineSchedule machineSchedule in machineSchedules)
            {
                implemented.AddRange(machineSchedule.OptimisedSequences);
            }

            List<Optimisation> results = implemented;

            // Mark implemented
            foreach (Optimisation optimisation in optimisations)
            {
                Optimisation? implementation = implemented
                    .Find(o =>
                        o.Type == optimisation.Type &&
                        optimisation.AffectedItems
                            .All(i => o.AffectedItems.Any(x => x.Name == i.Name))
                            );

                if (implementation == null)
                {
                    results.Add(optimisation);
                }
            }

            results = results.OrderByDescending(x => x.Type).ToList();

            Optimisations = results;
        }

        private void GetAdvisories()
        {
            List<Advisory> advisories = new();
            foreach (MachineSchedule machineSchedule in machineSchedules)
            {
                advisories.AddRange(machineSchedule.ActiveAdvisories);
            }

            this.Advisories = advisories;
        }

        private void GetWarnings()
        {
            List<Warning> warnings = new();
            foreach (MachineSchedule machineSchedule in machineSchedules)
            {
                warnings.AddRange(machineSchedule.ActiveWarnings);
            }

            this.Warnings = warnings;
        }

        private List<Optimisation> GetOppurtunitiesForOrder(LatheManufactureOrder order, List<LatheManufactureOrder> toCompare)
        {
            List<Optimisation> results = new();

            for (int i = 0; i < toCompare.Count; i++)
            {
                LatheManufactureOrder comparingTo = toCompare[i];
                if (order.BarID == comparingTo.BarID)
                {
                    Optimisation o = new()
                    {
                        AffectedItems = new()
                        {
                            order,
                            comparingTo,
                        },
                        Type = Optimisation.OptimisationType.SameBarId,
                        Implemented = false
                    };
                    results.Add(o);
                }
                else if (order.Bar.Size == comparingTo.Bar.Size && order.Bar.IsHexagon == comparingTo.Bar.IsHexagon)
                {
                    Optimisation o = new()
                    {
                        AffectedItems = new()
                        {
                            order,
                            comparingTo,
                        },
                        Type = Optimisation.OptimisationType.SameBarForm,
                        Implemented = false
                    };
                    results.Add(o);
                }

                if (order.GroupId == comparingTo.GroupId)
                {
                    Optimisation o = new()
                    {
                        AffectedItems = new()
                        {
                            order,
                            comparingTo,
                        },
                        Type = Optimisation.OptimisationType.SameArchetype,
                        Implemented = false
                    };
                    results.Add(o);
                }
            }

            return results;
        }

        public class RescheduleInformation
        {
            public ScheduleItem item;

            public string originMachineId;
            public DateTime originDate;

            public string? desiredMachineId;
            public DateTime? desiredDate;

            public bool createRecord = false;

            public RescheduleInformation(ScheduleItem item, string? destinationMachineId, DateTime? desiredDate)
            {
                this.item = item;

                this.originMachineId = item.AllocatedMachine;
                this.originDate = item.StartDate;

                if (item is MachineService service)
                {
                    if (service.Id == 0 && string.IsNullOrEmpty(service.AllocatedMachine))
                    {
                        this.createRecord = true;
                    }
                }

                if (string.IsNullOrWhiteSpace(destinationMachineId))
                {
                    destinationMachineId = null;
                }

                this.desiredMachineId = destinationMachineId;
                this.desiredDate = desiredDate;
            }
        }

        public class Optimisation
        {
            public List<ScheduleItem> AffectedItems { get; set; }
            public OptimisationType Type { get; set; }
            public bool Implemented { get; set; }

            internal static string GetText(OptimisationType type)
            {
                return type switch
                {
                    OptimisationType.SameArchetype => "Same Archetype",
                    OptimisationType.SameBarId => "Same Bar ID",
                    OptimisationType.SameBarForm => "Same Bar Form",
                    _ => "Unknown"
                };
            }

            public enum OptimisationType
            {
                SameBarForm,
                SameBarId,
                SameArchetype,
            }
        }

        public class Advisory
        {
            public ScheduleItem Item { get; set; }
            public AdvisoryType Type { get; set; }

            internal static string GetText(AdvisoryType type)
            {
                return type switch
                {
                    AdvisoryType.BelowSoftMinDiameter => "Bar diameter below machine soft limit",
                    AdvisoryType.AboveSoftMaxDiameter => "Bar diameter above machine soft limit",
                    AdvisoryType.SignificantOverlap => "Next item in schedule has significant overlap",
                    _ => "Unknown"
                };
            }

            public enum AdvisoryType
            {
                BelowSoftMinDiameter,
                AboveSoftMaxDiameter,
                SignificantOverlap
            }
        }

        public class Warning
        {
            public ScheduleItem Item { get; set; }
            public WarningType Type { get; set; }

            internal static string GetText(WarningType type)
            {
                return type switch
                {
                    WarningType.WillNotStartOnTime => "Will not start on time",
                    WarningType.StartsOutOfHours => "Starting out of hours",
                    WarningType.StartsOnHoliday => "Starting during holiday",
                    WarningType.DoubleBooking => "Machine Double booked on day",
                    WarningType.NotRunningOnTime => "Is not marked as running",
                    WarningType.BlockingOverlap => "Next item in schedule prevents requirement",
                    _ => "Unknown"
                };
            }

            public enum WarningType
            {
                WillNotStartOnTime,
                StartsOutOfHours,
                StartsOnHoliday,
                DoubleBooking,
                NotRunningOnTime,
                BlockingOverlap,
            }
        }

        public class ScheduleLock
        {
            public string MasterType { get; set; }
            public string MasterName { get; set; }

            public string SlaveType { get; set; }
            public string SlaveName { get; set; }

            public int DayOffset { get; set; }

            internal static ScheduleLock Parse(string code)
            {
                try
                {
                    ScheduleLock? l = Newtonsoft.Json.JsonConvert.DeserializeObject<ScheduleLock>(code) ?? throw new ArgumentException("Data is not a valid Schedule Lock");
                    return l;
                }
                catch
                {
                    throw;
                }
            }

            internal string Serialise()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            }
        }
    }
}
