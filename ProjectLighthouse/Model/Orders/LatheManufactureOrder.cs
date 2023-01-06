using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media.Media3D;

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
        [Ignore]
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


        [Ignore]
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

        public int TargetCycleTime { get; set; }
        public bool TargetCycleTimeEstimated { get; set; }

        [Ignore]
        public BarStock Bar { get; set; }
        [Ignore]
        public List<LatheManufactureOrderItem> OrderItems { get; set; } = new();
        [Ignore]
        public List<Note> Notes { get; set; } = new();
        [Ignore]
        public List<TechnicalDrawing> Drawings { get; set; } = new();
        [Ignore]
        public List<OrderDrawing> DrawingsReferences { get; set; } = new();
        [Ignore]
        public Lathe AssignedLathe { get; set; } = new();
        [Ignore]
        public List<BarIssue> BarIssues { get; set; } = new();

        #region Helpers
        public DateTime Deadline;

        public object Clone()
        {

            // TODO refactor
            return new LatheManufactureOrder
            {
                Id = Id,
                Name = Name,
                POReference = POReference,
                CreatedAt = CreatedAt,
                CreatedBy = CreatedBy,
                ModifiedAt = ModifiedAt,
                ModifiedBy = ModifiedBy,
                TimeToComplete = TimeToComplete,
                IsComplete = IsComplete,
                IsClosed = IsClosed,
                Status = Status,
                IsCancelled = IsCancelled,
                AllocatedMachine = AllocatedMachine,
                StartDate = StartDate,
                CompletedAt = CompletedAt,
                ToolingReady = ToolingReady,
                ToolingOrdered = ToolingOrdered,
                NumberOfBarsIssued = NumberOfBarsIssued,
                BarToolingReady = BarToolingReady,
                BarToolingOrdered = BarToolingOrdered,
                GaugingReady = GaugingReady,
                GaugingOrdered = GaugingOrdered,
                BaseProgramExists= BaseProgramExists,
                HasProgram = HasProgram,
                HasStarted = HasStarted,
                BarID = BarID,
                NumberOfBars = NumberOfBars,
                BarIsVerified = BarIsVerified,
                BarsInStockAtCreation = BarsInStockAtCreation,
                IsResearch = IsResearch,
                SpareBars = SpareBars,
                OrderItems = new(OrderItems ?? new()),
                TargetCycleTime = TargetCycleTime,
                TargetCycleTimeEstimated = TargetCycleTimeEstimated,
                GroupId = GroupId,
                MaterialId = MaterialId,
            };
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

        public void Update(LatheManufactureOrder otherOrder)
        {
            if (otherOrder.Name != Name)
            {
                throw new InvalidOperationException($"Cannot update order {Name} with record {otherOrder.Name}");
            }

            IsCancelled = otherOrder.IsCancelled;
            IsComplete = otherOrder.IsComplete;
            IsClosed = otherOrder.IsClosed;

            NumberOfBarsIssued = otherOrder.NumberOfBarsIssued;

            ToolingOrdered = otherOrder.ToolingOrdered;
            GaugingOrdered = otherOrder.GaugingOrdered;
            BarToolingOrdered = otherOrder.BarToolingOrdered;
            BaseProgramExists = otherOrder.BaseProgramExists;
            BarIsVerified = otherOrder.BarIsVerified;

            ToolingReady = otherOrder.ToolingReady;
            GaugingReady = otherOrder.GaugingReady;
            BarToolingReady = otherOrder.BarToolingReady;
            HasProgram = otherOrder.HasProgram;

            HasStarted = otherOrder.HasStarted;

            POReference = otherOrder.POReference;
            NumberOfBars = otherOrder.NumberOfBars;
            TimeToComplete = otherOrder.TimeToComplete;
            ModifiedAt = otherOrder.ModifiedAt;
            ModifiedBy = otherOrder.ModifiedBy;
            POReference = otherOrder.POReference;
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
    }
}
