using DocumentFormat.OpenXml.Spreadsheet;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.Model.Scheduling
{
    public partial class ProductionSchedule
    {
        public class MachineSchedule : BaseObject
        {
            public event EventHandler OnScheduleUpdated;
            public event EventHandler OnHolidaysUpdated;


            private Lathe lathe;
            public Lathe Lathe
            {
                get { return lathe; }
                set
                {
                    lathe = value;
                    OnPropertyChanged();
                }
            }


            private List<ScheduleItem> scheduleItems = new();
            public List<ScheduleItem> ScheduleItems
            {
                get { return scheduleItems; }
                set
                {
                    scheduleItems = value;
                    OnPropertyChanged();
                }
            }


            private List<Optimisation> optimisedSequences = new();
            public List<Optimisation> OptimisedSequences
            {
                get { return optimisedSequences; }
                set
                {
                    optimisedSequences = value;
                    OnPropertyChanged();
                }
            }

            private List<Advisory> activeAdvisories = new();
            public List<Advisory> ActiveAdvisories
            {
                get { return activeAdvisories; }
                set
                {
                    activeAdvisories = value;
                    OnPropertyChanged();
                }
            }

            private List<Warning> activeWarnings = new();
            public List<Warning> ActiveWarnings
            {
                get { return activeWarnings; }
                set
                {
                    activeWarnings = value;
                    OnPropertyChanged();
                }
            }

            public List<DateTime> Holidays = new();



            public MachineSchedule(Lathe lathe)
            {
                this.Lathe = lathe;
            }

            public void SetScheduleItems(List<ScheduleItem> items)
            {
                List<ScheduleItem> itemsToAdd = items
                        .Where(x => x.AllocatedMachine == Lathe.Id && x.StartDate > DateTime.MinValue)
                        .OrderBy(x => x.StartDate)
                        .ToList();

                List<ScheduleItem> nonCancelledItemsOnMachine = new();


                for (int i = 0; i < itemsToAdd.Count; i++)
                {
                    ScheduleItem item = itemsToAdd[i];

                    if (item is LatheManufactureOrder order)
                    {
                        if (order.State == OrderState.Cancelled) continue;
                    }

                    nonCancelledItemsOnMachine.Add(item);
                }

                this.ScheduleItems = nonCancelledItemsOnMachine;

                ProcessMessages();
            }

            private void ProcessMessages()
            {
                List<Optimisation> optimisations = new();
                List<Advisory> advisories = new();
                List<Warning> warnings = new();

                LatheManufactureOrder? previousOrder = null;

                for (int i = 0; i < ScheduleItems.Count; i++)
                {
                    ScheduleItem item = ScheduleItems[i];

                    if (item is LatheManufactureOrder order)
                    {
                        if (order.State < OrderState.Complete && previousOrder != null)
                        {
                            optimisations.AddRange(GetOrderOptimisations(order, previousOrder));

                            List<Advisory> orderAdvisories = GetOrderAdvisories(order);
                            advisories.AddRange(orderAdvisories);
                            order.SetAdvisories(orderAdvisories);

                            List<Warning> orderWarnings = GetOrderWarnings(order, previousOrder);
                            warnings.AddRange(orderWarnings);
                            order.SetWarnings(orderWarnings);

                        };

                        previousOrder = order;
                    }
                    else
                    {
                        previousOrder = null;
                    }
                }

                optimisations = BuildOptimisedSequences(optimisations);

                this.OptimisedSequences = optimisations;
                this.ActiveAdvisories = advisories;
                this.ActiveWarnings = warnings;
            }

            private List<Optimisation> BuildOptimisedSequences(List<Optimisation> optimisations)
            {
                if (optimisations.Count == 0) return optimisations;

                List<Optimisation.OptimisationType> uniqueTypes = optimisations.Select(x => x.Type).Distinct().ToList();
                List<Optimisation> denoisedSequences = new();


                foreach (Optimisation.OptimisationType optimisationType in uniqueTypes)
                {
                    List<Optimisation> foundOptimisationsOfType = optimisations.Where(o => o.Type == optimisationType).ToList();
                    List<Optimisation> denoisedSequencesOfType = DenoiseType(foundOptimisationsOfType);

                    denoisedSequences.AddRange(denoisedSequencesOfType);
                }

                return denoisedSequences;
            }

            private List<Optimisation> DenoiseType(List<Optimisation> foundOptimisationsOfType)
            {
                List<Optimisation> results = new();

                for (int i = 0; i < foundOptimisationsOfType.Count; i++)
                {
                    Optimisation currentOptimisation = foundOptimisationsOfType[i];
                    bool overlap = false;


                    for (int j = 0; j < results.Count; j++)
                    {
                        if (HasOverlap(results[j], currentOptimisation))
                        {
                            overlap = true;
                            results[j] = MergeSequence(results[j], currentOptimisation);
                            break;
                        }
                    }

                    if (!overlap)
                    {
                        results.Add(currentOptimisation);
                    }
                }

                return results;
            }

            private static bool HasOverlap(Optimisation optimisation, Optimisation currentOptimisation)
            {
                foreach (ScheduleItem item in currentOptimisation.AffectedItems)
                {
                    if (optimisation.AffectedItems.Contains(item))
                    {
                        return true;
                    }
                }

                return false;
            }

            private Optimisation MergeSequence(Optimisation main, Optimisation optimisation)
            {
                foreach (ScheduleItem item in optimisation.AffectedItems)
                {
                    if (!main.AffectedItems.Contains(item))
                    {
                        main.AffectedItems.Add(item);
                    }
                }

                return main;
            }

            public void SetHolidays(List<DateTime> holidays)
            {
                this.Holidays = holidays;
                OnHolidaysUpdated?.Invoke(this, EventArgs.Empty);
            }

            private static List<Optimisation> GetOrderOptimisations(LatheManufactureOrder order, LatheManufactureOrder previousOrder)
            {
                List<Optimisation> optimisations = new();
                if (order.GroupId == previousOrder.GroupId)
                {
                    Optimisation o = new()
                    {
                        AffectedItems = new()
                        {
                            order,
                            previousOrder,
                        },
                        Type = Optimisation.OptimisationType.SameArchetype,
                        Implemented = true,
                    };

                    optimisations.Add(o);
                }

                if (order.BarID == previousOrder.BarID)
                {
                    Optimisation o = new()
                    {
                        AffectedItems = new()
                        {
                            order,
                            previousOrder,
                        },

                        Type = Optimisation.OptimisationType.SameBarId,
                        Implemented = true,
                    };

                    optimisations.Add(o);
                }
                else if (order.Bar.Size == previousOrder.Bar.Size && order.Bar.IsHexagon == previousOrder.Bar.IsHexagon)
                {
                    Optimisation o = new()
                    {
                        AffectedItems = new()
                        {
                            order,
                            previousOrder,
                        },

                        Type = Optimisation.OptimisationType.SameBarForm,
                        Implemented = true,
                    };

                    optimisations.Add(o);
                }

                return optimisations;
            }

            private List<Advisory> GetOrderAdvisories(LatheManufactureOrder order)
            {
                List<Advisory> advisories = new();

                if (order.Bar.MajorDiameter < Lathe.SoftMinDiameter)
                {
                    Advisory advisory = new() { Item = order, Type = Advisory.AdvisoryType.BelowSoftMinDiameter };

                    advisories.Add(advisory);
                }

                if (order.Bar.MajorDiameter > Lathe.SoftMaxDiameter && order.Bar.MajorDiameter <= Lathe.MaxDiameter)
                {
                    Advisory advisory = new() { Item = order, Type = Advisory.AdvisoryType.AboveSoftMaxDiameter };

                    advisories.Add(advisory);
                }


                return advisories;
            }

            private static List<Warning> GetOrderWarnings(LatheManufactureOrder order, LatheManufactureOrder previousOrder)
            {
                List<Warning> warnings = new();
                DateTime deadline = order.GetStartDeadline();

                if (order.StartDate > deadline)
                {
                    Warning newWarning = new() { Item = order, Type = Warning.WarningType.WillNotStartOnTime };

                    warnings.Add(newWarning);
                }


                if (App.Constants.OpeningHours.IsOutOfHours(order.GetSettingStartDateTime()))
                {
                    Warning newWarning = new() { Item = order, Type = Warning.WarningType.StartsOutOfHours };

                    warnings.Add(newWarning);
                }
                else if (App.Constants.OpeningHours.IsOutOfHours(order.StartDate))
                {
                    Warning newWarning = new() { Item = order, Type = Warning.WarningType.StartsOutOfHours };

                    warnings.Add(newWarning);
                }

                if (order.StartDate.Date == previousOrder.StartDate.Date)
                {
                    Warning newWarning = new() { Item = order, Type = Warning.WarningType.DoubleBooking };

                    warnings.Add(newWarning);
                }

                if (order.StartDate.Date < DateTime.Today && order.State < OrderState.Running && order.StartDate > DateTime.MinValue)
                {
                    Warning newWarning = new() { Item = order, Type = Warning.WarningType.NotRunningOnTime };

                    warnings.Add(newWarning);
                }


                return warnings;
            }

            internal List<ScheduleItem> GetItems(DateTime? minDate, DateTime? maxDate)
            {
                List<ScheduleItem> itemsInWindow = new();

                foreach (ScheduleItem item in ScheduleItems)
                {
                    DateTime absoluteStart = item.StartDate.Date;
                    DateTime absoluteEnd = item.StartDate.Date;
                    if (item is LatheManufactureOrder order)
                    {
                        absoluteStart = order.GetSettingStartDateTime().Date;
                        absoluteEnd = order.EndsAt().Date;
                    }

                    if (minDate != null && maxDate != null)
                    {
                        if (absoluteEnd >= minDate && absoluteStart <= maxDate)
                        {
                            itemsInWindow.Add(item);
                        }
                    }
                    else if (minDate is not null)
                    {
                        if (absoluteEnd >= minDate)
                        {
                            itemsInWindow.Add(item);
                        }
                    }
                    else if (maxDate is not null)
                    {
                        if (absoluteStart <= maxDate)
                        {
                            itemsInWindow.Add(item);
                        }
                    }
                }

                return itemsInWindow;
            }


            internal void Add(ScheduleItem updatedItem)
            {
                List<ScheduleItem> items = this.ScheduleItems;
                items.Add(updatedItem);

                SetScheduleItems(items);
            }

            internal void Remove(ScheduleItem updatedItem)
            {
                List<ScheduleItem> items = this.ScheduleItems;
                items.Remove(updatedItem);

                SetScheduleItems(items);
            }

            public void Refresh()
            {
                OnScheduleUpdated?.Invoke(this, EventArgs.Empty);
                SetScheduleItems(ScheduleItems);
            }
        }
    }
}
