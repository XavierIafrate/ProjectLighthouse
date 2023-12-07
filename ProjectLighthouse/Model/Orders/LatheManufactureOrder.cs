using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ProjectLighthouse.Model.Orders
{
    public partial class LatheManufactureOrder : ScheduleItem, ICloneable
    {
        [UpdateWatch]
        public string POReference { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }

        public string Status { get; set; } // Legacy
        public OrderState State
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
                else if (BarIsAllocated && HasProgram && AllToolingReady)
                {
                    result = OrderState.Prepared;
                }
                else if (
                       (ToolingOrdered || ToolingReady)
                    && (BarToolingOrdered || BarToolingReady)
                    && (GaugingOrdered || GaugingReady)
                    && (BaseProgramExists || HasProgram)
                    && BarIsVerified)
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

        [UpdateWatch]
        public string? AssignedTo { get; set; }


        #region Identification Phase

        [UpdateWatch]
        public bool ToolingOrdered { get; set; }
        [UpdateWatch]
        public bool BarToolingOrdered { get; set; }
        [UpdateWatch]
        public bool GaugingOrdered { get; set; }
        [UpdateWatch]
        public bool BaseProgramExists { get; set; }
        [UpdateWatch]
        public bool BarIsVerified { get; set; }

        #endregion

        #region Preparation Phase
        [SQLite.Ignore]
        public bool AllToolingReady
        {
            get
            {
                return ToolingReady && GaugingReady && BarToolingReady;
            }
        }
        [UpdateWatch]
        public bool ToolingReady { get; set; }
        [UpdateWatch]
        public bool BarToolingReady { get; set; }
        [UpdateWatch]
        public bool GaugingReady { get; set; }


        private bool partsWillBePlated;
        [UpdateWatch]
        public bool PartsWillBePlated
        {
            get { return partsWillBePlated; }
            set { partsWillBePlated = value; OnPropertyChanged(); }
        }


        [SQLite.Ignore]
        public bool BarIsAllocated
        {
            get { return NumberOfBarsIssued >= NumberOfBars; }
        }


        [UpdateWatch]
        public bool HasProgram { get; set; }

        #endregion

        #region Running

        [UpdateWatch]
        public bool HasStarted { get; set; }

        #endregion

        #region Completed Order
        [UpdateWatch]
        public bool IsComplete { get; set; }
        [UpdateWatch]
        public bool IsCancelled { get; set; }
        [UpdateWatch]
        public bool IsClosed { get; set; }
        #endregion

        public DateTime CompletedAt { get; set; }

        private DateTime scheduledEnd;
        [UpdateWatch]
        public DateTime ScheduledEnd
        {
            get
            {
                return EndsAt();
                //if (scheduledEnd == DateTime.MinValue)
                //{
                //}
                //else
                //{
                //    return scheduledEnd;
                //}
            }
            set
            {
                scheduledEnd = value;
                OnPropertyChanged();
            }
        }

        #region Bar Configuration
        [UpdateWatch]
        public string BarID { get; set; }
        [UpdateWatch]
        public double NumberOfBars { get; set; }
        [UpdateWatch]
        public int SpareBars { get; set; }
        [UpdateWatch]
        public int NumberOfBarsIssued { get; set; }
        public double BarsInStockAtCreation { get; set; }
        #endregion

        public double MajorDiameter { get; set; }
        public int GroupId { get; set; }
        public int MaterialId { get; set; }
        [UpdateWatch]
        public bool IsResearch { get; set; }

        [UpdateWatch]
        public string TimeCodePlanned { get; set; }
        public bool TimeCodeIsEstimate { get; set; }

        [UpdateWatch]
        public string? TimeCodeActual { get; set; }

        [SQLite.Ignore]
        public TimeModel TimeModelPlanned
        {
            get
            {
                if (TimeCodePlanned is null) return null;
                return new(TimeCodePlanned);
            }
            set
            {
                if (value is null)
                {
                    TimeCodePlanned = null;
                    return;
                }
                TimeCodePlanned = value.ToString();
            }
        }

        [SQLite.Ignore]
        public TimeModel? TimeModelActual
        {
            get
            {
                if (TimeCodeActual is null) return null;
                return new(TimeCodeActual);
            }
            set
            {
                if (value is null)
                {
                    TimeCodeActual = null;
                    return;
                }
                TimeCodeActual = value.ToString();
            }
        }


        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public BarStock Bar { get; set; }

        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<LatheManufactureOrderItem> OrderItems { get; set; } = new();

        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<Note> Notes { get; set; } = new();

        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<TechnicalDrawing> Drawings { get; set; } = new();

        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<OrderDrawing> DrawingsReferences { get; set; } = new();

        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public Lathe AssignedLathe { get; set; } = new();

        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<BarIssue> BarIssues { get; set; } = new();

        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<Lot> Lots { get; set; } = new();

        #region Helpers
        public DateTime Deadline;
        public bool SameBar;

        public object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<LatheManufactureOrder>(serialised);
        }

        public override string ToString()
        {
            return Name;
        }

        public bool IsUpdated(LatheManufactureOrder OtherOrder)
        {
            if (OtherOrder.Name != Name)
            {
                throw new InvalidOperationException($"Cannot compare order {Name} with record {OtherOrder.Name}");
            }

            PropertyInfo[] properties = typeof(LatheManufactureOrder).GetProperties();
            bool mod = false;
            StringBuilder sb = new();

            foreach (PropertyInfo property in properties)
            {
                bool watchPropForChanges = property.GetCustomAttribute<UpdateWatch>() != null;
                if (!watchPropForChanges)
                {
                    continue;
                }

                if (!Equals(property.GetValue(this), property.GetValue(OtherOrder)))
                {
                    if (!mod)
                    {
                        sb.AppendLine($"{DateTime.Now:s} | {App.CurrentUser.UserName}");
                    }
                    sb.AppendLine($"\t{property.Name} modified");
                    sb.AppendLine($"\t\tfrom: '{property.GetValue(this) ?? "null"}'");
                    sb.AppendLine($"\t\tto  : '{property.GetValue(OtherOrder) ?? "null"}'");

                    mod = true;
                }

            }

            if (mod)
            {
                string path = App.ROOT_PATH + @"lib\logs\" + Name + ".log";

                File.AppendAllText(path, sb.ToString());
            }

            return mod;
        }

        public bool RequiresBar()
        {
            return DateTime.Now.AddDays(14) > StartDate && StartDate.Year != DateTime.MinValue.Year && NumberOfBars > NumberOfBarsIssued;
        }

        public DateTime GetStartDeadline()
        {
            DateTime dateTime = DateTime.MaxValue;

            for (int i = 0; i < OrderItems.Count; i++)
            {
                if (OrderItems[i].RequiredQuantity == 0)
                {
                    continue;
                }
                DateTime deadline = OrderItems[i].DateRequired.AddSeconds(OrderItems[i].GetTimeToMakeRequired());
                dateTime = dateTime > deadline
                    ? deadline
                    : dateTime;
            }

            return dateTime;
        }

        public DateTime AnticipatedEndDate()
        {
            if (OrderItems.Count == 0)
            {
                throw new Exception("Items are unknown, cannot calculate time");
            }

            int seconds = OrderResourceHelper.CalculateOrderRuntime(this, OrderItems);

            return StartDate.AddSeconds(seconds);
        }

        #endregion

        public void UpdateStartDate(DateTime date, string machine)
        {
            StartDate = date.Date == DateTime.MinValue.Date ? DateTime.MinValue : date.Date.AddHours(12);
            AllocatedMachine = string.IsNullOrEmpty(machine) ? null : machine;
            string dbMachineEntry = string.IsNullOrEmpty(AllocatedMachine) ? "NULL" : $"'{AllocatedMachine}'";
            DatabaseHelper.ExecuteCommand($"UPDATE {nameof(LatheManufactureOrder)} SET StartDate = {StartDate.Ticks}, AllocatedMachine={dbMachineEntry} WHERE Id={Id}");
        }

        public void MarkAsClosed()
        {
            DatabaseHelper.ExecuteCommand($"UPDATE {nameof(LatheManufactureOrder)} SET IsClosed = {true} WHERE Id={Id}");
        }

        public void MarkAsNotClosed()
        {
            DatabaseHelper.ExecuteCommand($"UPDATE {nameof(LatheManufactureOrder)} SET IsClosed = {false} WHERE Id={Id}");
        }

        public new DateTime EndsAt()
        {
            if (IsResearch)
            {
                return StartDate.AddSeconds(Math.Max(TimeToComplete, 86400 * 1.75));
            }
            else
            {
                return StartDate.AddSeconds(TimeToComplete);
            }
        }
    }
}
