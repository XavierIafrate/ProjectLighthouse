using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using SQLite;
using System;
using System.Reflection;
#if DEBUG
using System.Diagnostics;
#endif

namespace ProjectLighthouse.Model.Orders
{
    public class GeneralManufactureOrder : ScheduleItem
    {
        public int NonTurnedItemId { get; set; }

        [UpdateWatch]
        public int RequiredQuantity { get; set; }
        [UpdateWatch]
        public int FinishedQuantity { get; set; }
        [UpdateWatch]
        public int DeliveredQuantity { get; set; }
        [UpdateWatch]
        public DateTime? RequiredDate { get; set; } // Convert to full


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
                else if (MaterialReady && ToolingReady && ProgramReady && GaugingReady) 
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
                return result;
            }
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
    }
}
