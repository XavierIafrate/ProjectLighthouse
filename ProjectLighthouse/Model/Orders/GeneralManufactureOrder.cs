using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using SQLite;
using System;
using System.Reflection;
using System.Linq;
using ProjectLighthouse.Model.Core;
using System.Runtime.CompilerServices;
#if DEBUG
using System.Diagnostics;
#endif

namespace ProjectLighthouse.Model.Orders
{
    public class GeneralManufactureOrder : ScheduleItem
    {
        private int nonTurnedItemId;
        public int NonTurnedItemId

        {
            get { return nonTurnedItemId; }
            set
            {
                nonTurnedItemId = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }


        private int requiredQuantity;
        [UpdateWatch]
        public int RequiredQuantity
        {
            get { return requiredQuantity; }
            set
            {
                requiredQuantity = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private int finishedQuantity;
        [UpdateWatch]
        public int FinishedQuantity
        {
            get { return finishedQuantity; }
            set
            {
                finishedQuantity = value;
                OnPropertyChanged();
            }
        }


        private int deliveredQuantity;
        [UpdateWatch]
        public int DeliveredQuantity
        {
            get { return deliveredQuantity; }
            set
            {
                deliveredQuantity = value;
                OnPropertyChanged();
            }
        }


        private DateTime? requiredDate;
        [UpdateWatch]
        public DateTime? RequiredDate
        {
            get { return requiredDate; }
            set
            {
                requiredDate = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }



        [Ignore]
        public NonTurnedItem Item { get; set; }

        public override OrderState State
        {
            get
            {
                OrderState result;

                if (IsCancelled)
                {
                    result = OrderState.Cancelled;
                }
                else if (IsComplete)
                {
                    result = OrderState.Complete;
                }
                else if (HasStarted)
                {
                    result = OrderState.Running;
                }
                else if (ReadyToRun)
                {
                    result = OrderState.Prepared;
                }
                else if (
                       (ToolingOrdered || ToolingReady)
                    && (MaterialOrdered || MaterialReady)
                    && (GaugingOrdered || GaugingReady)
                    && ProgramReady)
                {
                    result = OrderState.Ready;
                }
                else
                {
                    result = OrderState.Problem;
                }

                if (result == OrderState.Ready && StartDate <= DateTime.Today.AddDays(7) && StartDate.Date != DateTime.MinValue)
                {
                    result = OrderState.Problem;
                }
                OnPropertyChanged(nameof(ReadyToRun));
                return result;
            }
        }

        [Ignore]
        public bool ReadyToRun
        {
            get { return MaterialReady && ToolingReady && ProgramReady && GaugingReady; }
        }


        private bool toolingOrdered;
        [UpdateWatch]
        public bool ToolingOrdered
        {
            get { return toolingOrdered; }
            set
            {
                toolingOrdered = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

        private bool toolingReady;
        [UpdateWatch]
        public bool ToolingReady
        {
            get { return toolingReady; }
            set
            {
                toolingReady = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(ReadyToRun));
            }
        }


        private bool programReady;
        [UpdateWatch]
        public bool ProgramReady
        {
            get { return programReady; }
            set
            {
                programReady = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(ReadyToRun));
            }
        }


        private bool gaugingOrdered;
        [UpdateWatch]
        public bool GaugingOrdered
        {
            get { return gaugingOrdered; }
            set
            {
                gaugingOrdered = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

        private bool gaugingReady;
        [UpdateWatch]
        public bool GaugingReady
        {
            get { return gaugingReady; }
            set
            {
                gaugingReady = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(ReadyToRun));
            }
        }

        private bool materialOrdered;
        [UpdateWatch]
        public bool MaterialOrdered
        {
            get { return materialOrdered; }
            set
            {
                materialOrdered = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

        private bool materialReady;
        [UpdateWatch]
        public bool MaterialReady
        {
            get { return materialReady; }
            set
            {
                materialReady = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(ReadyToRun));
            }
        }



        public bool IsUpdated(GeneralManufactureOrder OtherOrder)
        {
            if (OtherOrder.Name != Name)
            {
                throw new InvalidOperationException($"Cannot compare order {Name} with record {OtherOrder.Name}");
            }

            PropertyInfo[] properties = typeof(GeneralManufactureOrder).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                bool watchPropForChanges = property.GetCustomAttribute<UpdateWatch>() != null;
                if (!watchPropForChanges)
                {
                    continue;
                }

                if (!Equals(property.GetValue(this), property.GetValue(OtherOrder)))
                {
#if DEBUG
                    Debug.WriteLine($"'{property.Name}' has been modified:");
                    Debug.WriteLine($"   After: '{property.GetValue(this)}'");
                    Debug.WriteLine($"  Before: '{property.GetValue(OtherOrder)}'");
#endif
                    return true;
                }

            }

            return false;
        }

        public override object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<GeneralManufactureOrder>(serialised);
        }

        public int CalculateTimeToComplete()
        {
            DateTime start = StartDate == DateTime.MinValue ? DateTime.Today.AddDays(1) : StartDate;
            DateTime cursor = start;

            int remaining = RequiredQuantity;

            if (Lots != null)
            {
                if (Lots.Count > 0)
                {
                    remaining -= Lots.Where(x => x.IsAccepted).Sum(x => x.Quantity);
                    cursor = Lots.Where(x => x.IsAccepted).Max(x => x.DateProduced);
                }
            }


            while (remaining > 0)
            {
                OpeningHours.Day cursorOpeningHours = App.Constants.OpeningHours.Data[cursor.DayOfWeek];
                if (!cursorOpeningHours.OpensOnDay)
                {
                    cursor = cursor.AddDays(1);
                    continue;
                }


                TimeSpan availableTime = cursorOpeningHours.GetOpeningHoursTimeSpan();
                int totalOnDay = (int)Math.Floor(availableTime.TotalSeconds / Item.CycleTime);

                remaining -= totalOnDay;
                remaining = Math.Max(remaining, 0);

                cursor = cursor.AddDays(1);
            }

            return (int)Math.Round((cursor - start).TotalSeconds);
        }

        public override void ValidateAll()
        {
            base.ValidateAll();
            ValidateProperty(nameof(RequiredQuantity));
            ValidateProperty(nameof(RequiredDate));
            ValidateProperty(nameof(NonTurnedItemId));
        }

        public override void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            base.ValidateProperty(propertyName);

            if (propertyName == nameof(NonTurnedItemId))
            {
                ClearErrors(propertyName);

                if (NonTurnedItemId == 0)
                {
                    AddError(propertyName, "Item must be set");
                }

                return;
            }
            else if (propertyName == nameof(RequiredQuantity))
            {
                ClearErrors(propertyName);

                if (RequiredQuantity <= 0)
                {
                    AddError(propertyName, "Required Quantity must be greater than zero");
                }

                return;
            }
            else if (propertyName == nameof(RequiredDate))
            {
                ClearErrors(propertyName);

                if (RequiredDate == DateTime.MinValue)
                {
                    AddError(propertyName, "Required Date must be set");
                }

                return;
            }
        }
    }
}
