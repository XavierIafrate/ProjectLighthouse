using ProjectLighthouse.Model.Administration;
using SQLite;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model
{
    public partial class LatheManufactureOrder : ScheduleItem, ICloneable
    {
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
                    result =  OrderState.Cancelled;
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
                       (ToolingOrdered||ToolingReady) 
                    && (BarToolingOrdered||BarToolingReady) 
                    && (GaugingOrdered||GaugingReady) 
                    && (BaseProgramExists||HasProgram) 
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

        public bool ToolingOrdered { get; set; }
        public bool BarToolingOrdered { get; set; }
        public bool GaugingOrdered { get; set; }
        public bool BaseProgramExists { get; set; }
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
        public bool ToolingReady { get; set; }
        public bool BarToolingReady { get; set; }
        public bool GaugingReady { get; set; }


        [Ignore]
        public bool BarIsAllocated
        {
            get { return NumberOfBarsIssued >= NumberOfBars; }
        }


        public bool HasProgram { get; set; }

        #endregion

        #region Running

        public bool HasStarted { get; set; }

        #endregion

        #region Completed Order
        public bool IsComplete { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsClosed { get; set; }
        #endregion

        public DateTime CompletedAt { get; set; }

        #region Bar Configuration
        public string BarID { get; set; }
        public double NumberOfBars { get; set; }
        public int SpareBars { get; set; }
        public int NumberOfBarsIssued { get; set; }
        public double BarsInStockAtCreation { get; set; }
        #endregion

        public double MajorDiameter { get; set; }
        public bool ItemNeedsCleaning { get; set; }
        public string ToolingGroup { get; set; }
        public bool IsResearch { get; set; }

        public int TargetCycleTime { get; set; }
        public bool TargetCycleTimeEstimated { get; set; }

        public BarStock Bar;
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
                NumberOfBarsIssued = NumberOfBarsIssued,
                BarToolingReady = BarToolingReady,
                GaugingReady = GaugingReady,
                HasProgram = HasProgram,
                HasStarted = HasStarted,
                BarID = BarID,
                NumberOfBars = NumberOfBars,
                ItemNeedsCleaning = ItemNeedsCleaning,
                BarIsVerified = BarIsVerified,
                BarsInStockAtCreation = BarsInStockAtCreation,
                IsResearch = IsResearch,
                SpareBars = SpareBars,
                OrderItems = new(OrderItems ?? new()),
                TargetCycleTime = TargetCycleTime,
                TargetCycleTimeEstimated = TargetCycleTimeEstimated,
                ToolingGroup = ToolingGroup,
            };
        }

        public override string ToString()
        {
            return Name;
        }

        public bool IsUpdated(LatheManufactureOrder OtherOrder)
        {
            return IsCancelled != OtherOrder.IsCancelled
                || IsComplete != OtherOrder.IsComplete
                || IsClosed != OtherOrder.IsClosed
                || State != OtherOrder.State
                || HasStarted != OtherOrder.HasStarted
                || BarIsVerified != OtherOrder.BarIsVerified
                || BaseProgramExists != OtherOrder.BaseProgramExists
                || HasProgram != OtherOrder.HasProgram
                || ToolingReady != OtherOrder.ToolingReady
                || NumberOfBars != OtherOrder.NumberOfBars
                || NumberOfBarsIssued != OtherOrder.NumberOfBarsIssued
                || GaugingOrdered != OtherOrder.GaugingOrdered
                || GaugingReady != OtherOrder.GaugingReady
                || BarToolingOrdered != OtherOrder.BarToolingOrdered
                || BarToolingReady != OtherOrder.BarToolingReady
                || POReference != OtherOrder.POReference
                || TimeToComplete != OtherOrder.TimeToComplete
                || ModifiedAt != OtherOrder.ModifiedAt
                || SpareBars != OtherOrder.SpareBars
                || IsResearch != OtherOrder.IsResearch
                || ToolingGroup != OtherOrder.ToolingGroup
                || BarID != OtherOrder.BarID;
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
    }
}
