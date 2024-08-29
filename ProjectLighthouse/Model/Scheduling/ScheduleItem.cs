using ABI.Windows.ApplicationModel.Activation;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProjectLighthouse.Model.Scheduling
{
    public abstract class ScheduleItem : BaseObject, ISchedulableObject, IObjectWithValidation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        private DateTime? modifiedAt;
        public DateTime? ModifiedAt
        {
            get 
            { 
                if (modifiedAt is null)
                {
                    return null;
                }
                else if (modifiedAt == DateTime.MinValue)
                {
                    return null;
                }
                else
                {
                    return modifiedAt;
                }
            }
            set
            {
                modifiedAt = value;
                OnPropertyChanged();
            }
        }

        private string? modifiedBy;
        public string? ModifiedBy
        {
            get { return modifiedBy; }
            set
            {
                modifiedBy = value;
                OnPropertyChanged();
            }
        }

        private string poReference = string.Empty;
        [UpdateWatch]
        public string POReference
        {
            get { return poReference; }
            set
            {
                poReference = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private OrderState orderState = OrderState.Problem;
        [UpdateWatch]
        public virtual OrderState State
        {
            get
            {
                return orderState;
            }
            set
            {
                orderState = value;
                OnPropertyChanged();
            }
        }


        private int timeToComplete;
        [UpdateWatch]
        public int TimeToComplete
        {
            get { return timeToComplete; }
            set { timeToComplete = value; OnPropertyChanged(); }
        }

        private DateTime startDate = DateTime.MinValue;
        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; OnPropertyChanged(); }
        }

        private string allocatedMachine;

        public string AllocatedMachine
        {
            get { return allocatedMachine; }
            set { allocatedMachine = value; OnPropertyChanged(); }
        }

        private string? assignedTo;
        [UpdateWatch]
        public string? AssignedTo
        {
            get
            {
                return assignedTo;
            }
            set
            {
                assignedTo = value;
                OnPropertyChanged();
            }
        }


        private bool hasStarted;
        [UpdateWatch]
        public bool HasStarted
        {
            get
            {
                return hasStarted;
            }
            set
            {
                hasStarted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

        private bool isComplete;
        [UpdateWatch]
        public bool IsComplete
        {
            get
            {
                return isComplete;
            }
            set
            {
                isComplete = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

        private bool isCancelled;
        [UpdateWatch]
        public bool IsCancelled
        {
            get
            {
                return isCancelled;
            }
            set
            {
                isCancelled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }


        [UpdateWatch]
        public bool IsClosed { get; set; }


        [UpdateWatch]
        public string? ScheduleLock { get; set; }

        [Ignore]
        public ProductionSchedule.ScheduleLock? ScheduleLockData
        {
            get
            {
                if (string.IsNullOrEmpty(ScheduleLock)) return null;
                try
                {
                    return ProductionSchedule.ScheduleLock.Parse(ScheduleLock);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    ScheduleLock = null;
                    return;
                }

                ScheduleLock = value.Serialise();
                OnPropertyChanged();
            }
        }

        private bool editing;
        [Ignore]
        public bool Editing
        {
            get { return editing; }
            set { editing = value; OnPropertyChanged(); }
        }


        public event EventHandler OnScheduleLockChanged;

        private List<ProductionSchedule.Warning> warnings = new();
        [Ignore]
        public List<ProductionSchedule.Warning> Warnings
        {
            get { return warnings; }
            set { warnings = value; }
        }

        private List<ProductionSchedule.Advisory> advisories = new();
        [Ignore]
        public List<ProductionSchedule.Advisory> Advisories
        {
            get { return advisories; }
            set { advisories = value; }
        }

        public event EventHandler OnAdvisoriesChanged;
        public event EventHandler OnWarningsChanged;


        internal void SetAdvisories(List<ProductionSchedule.Advisory> orderAdvisories)
        {
            this.Advisories = orderAdvisories;
            OnAdvisoriesChanged?.Invoke(this, EventArgs.Empty);
        }

        internal void SetWarnings(List<ProductionSchedule.Warning> orderWarnings)
        {
            this.Warnings = orderWarnings;
            OnWarningsChanged?.Invoke(this, EventArgs.Empty);
        }


        private List<Lot> lots;
        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<Lot> Lots
        {
            get { return lots; }
            set
            {
                lots = value;
                OnPropertyChanged();
            }
        }

        private List<Note> notes;
        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<Note> Notes
        {
            get
            {
                return notes;
            }
            set
            {
                notes = value;
                OnPropertyChanged();
            }
        }

        public event Action EditMade;
        public void NotifyEditMade()
        {
            EditMade?.Invoke();
        }

        public event Action RequestToEdit;
        public void RequestEdit()
        {
            RequestToEdit?.Invoke();
        }

        public virtual DateTime EndsAt()
        {
            return StartDate.AddSeconds(TimeToComplete);
        }

        public void UpdateStartDate(DateTime date, string machine)
        {
            StartDate = date == DateTime.MinValue ? DateTime.MinValue : date;
            AllocatedMachine = string.IsNullOrEmpty(machine) ? null : machine;

            string table;
            if (this is LatheManufactureOrder)
            {
                table = nameof(LatheManufactureOrder);
            }
            else if (this is MachineService)
            {
                table = nameof(MachineService);
            }
            else if (this is GeneralManufactureOrder)
            {
                table = nameof(GeneralManufactureOrder);
            }
            else
            {
                throw new NotImplementedException();
            }

            string dbMachineEntry = string.IsNullOrEmpty(AllocatedMachine) ? "NULL" : $"'{AllocatedMachine}'";

            DatabaseHelper.ExecuteCommand($"UPDATE {table} SET StartDate = {StartDate.Ticks}, AllocatedMachine={dbMachineEntry} WHERE Id={Id}");
        }

        public virtual object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ScheduleItem>(serialised);
        }

        public virtual bool IsUpdated(ScheduleItem otherItem)
        {
            if (otherItem.Name != Name)
            {
                throw new InvalidOperationException($"Items being compared must have the same name");
            }


            return GetListOfChanges(otherItem).Count > 0;
        }

        public virtual List<string> GetListOfChanges(ScheduleItem otherItem)
        {
            PropertyInfo[] properties;
            if (this is LatheManufactureOrder)
            {
                if (otherItem is not LatheManufactureOrder)
                {
                    throw new InvalidOperationException($"Lathe order must be compared to Lathe order");

                }
                properties = typeof(LatheManufactureOrder).GetProperties();
            }
            else if (this is GeneralManufactureOrder)
            {
                if (otherItem is not GeneralManufactureOrder)
                {
                    throw new InvalidOperationException($"General order must be compared to General order");

                }
                properties = typeof(GeneralManufactureOrder).GetProperties();
            }
            else
            {
                throw new NotImplementedException();
            }

            List<string> results = new();

            bool mod = false;

            StringBuilder sb = new();

            foreach (PropertyInfo property in properties)
            {
                bool watchPropForChanges = property.GetCustomAttribute<UpdateWatch>() != null;
                if (!watchPropForChanges)
                {
                    continue;
                }

                if (!Equals(property.GetValue(this), property.GetValue(otherItem)))
                {
                    if (!mod)
                    {
                        results.Add($"{DateTime.Now:s} | {App.CurrentUser.UserName}");
                    }
                    sb.AppendLine($"\t{property.Name} modified");
                    sb.AppendLine($"\t\tfrom: '{property.GetValue(this) ?? "null"}'");
                    sb.AppendLine($"\t\tto  : '{property.GetValue(otherItem) ?? "null"}'");
                    results.Add(sb.ToString());
                    mod = true;
                }
            }

            return results;
        }

        public virtual void ValidateAll()
        {
            ValidateProperty(nameof(POReference));
        }

        public virtual void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(POReference))
            {
                ClearErrors(propertyName);

                if (POReference != POReference.Trim())
                {
                    AddError(propertyName, "PO Reference must not have leading or trailing whitespace");
                }

                return;
            }
        }

        internal void TakeChanges(ScheduleItem freshCopy)
        {
            PropertyInfo[] properties;
            if (this is LatheManufactureOrder)
            {
                if (freshCopy is not LatheManufactureOrder)
                {
                    throw new InvalidOperationException($"Lathe order must be compared to Lathe order");

                }
                properties = typeof(LatheManufactureOrder).GetProperties();
            }
            else if (this is GeneralManufactureOrder)
            {
                if (freshCopy is not GeneralManufactureOrder)
                {
                    throw new InvalidOperationException($"General order must be compared to General order");

                }
                properties = typeof(GeneralManufactureOrder).GetProperties();
            }
            else
            {
                throw new NotImplementedException();
            }

            foreach (PropertyInfo property in properties)
            {
                bool watchPropForChanges = property.GetCustomAttribute<UpdateWatch>() != null;
                if (!watchPropForChanges)
                {
                    continue;
                }

                if (!Equals(property.GetValue(this), property.GetValue(freshCopy)))
                {
                    if (property.GetSetMethod() != null)
                    {
                        property.SetValue(this, property.GetValue(freshCopy));
                    }
                }
            }
        }

        private bool lockedForEditing;
        [Ignore]
        public bool LockedForEditing
        {
            get { return lockedForEditing; }
            set { lockedForEditing = value; OnPropertyChanged(); }
        }
    }
}
