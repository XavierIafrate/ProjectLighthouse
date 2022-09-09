using SQLite;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model
{
    public partial class LatheManufactureOrder : ScheduleItem, ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public string POReference { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }


        public OrderState State
        {
            get
            {
                DateTime startDate = StartDate > DateTime.MinValue.AddDays(15) ? StartDate : DateTime.MinValue.AddDays(15);

                if (IsCancelled)
                {
                    return OrderState.Cancelled;
                }
                else if (IsComplete)
                {
                    return OrderState.Complete;
                }
                else if (HasStarted)
                {
                    return OrderState.Running;
                }
                else if (BarIsAllocated && BarIsVerified && HasProgram && ToolingReady && GaugingReady && ColletsReady)
                {
                    return OrderState.Prepared;
                }
                else if (BarIsVerified && HasProgram && startDate.AddDays(-14) > DateTime.Now)
                {
                    return OrderState.Ready;
                }
                else
                {
                    return OrderState.Problem;
                }
            }
            set { }
        }

        [Ignore]
        public bool AllToolingReady
        {
            get
            {
                return ToolingReady && GaugingReady && ColletsReady;
            }
        }
        public bool ToolingReady { get; set; }
        public bool GaugingReady { get; set; }
        public bool ColletsReady { get; set; }

        public bool HasProgram { get; set; }
        public bool HasStarted { get; set; }
        public bool BarIsAllocated { get; set; }
        public bool BarIsVerified { get; set; }
        public bool IsComplete { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsClosed { get; set; }

        public string Status { get; set; }
        public DateTime CompletedAt { get; set; }
        public string BarID { get; set; }
        public double NumberOfBars { get; set; }
        public double BarsInStockAtCreation { get; set; }
        public double MajorDiameter { get; set; }
        public bool ItemNeedsCleaning { get; set; }
        public string ToolingGroup { get; set; }
        public bool IsResearch { get; set; }
        public int SpareBars { get; set; }
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
                State = State,
                AllocatedMachine = AllocatedMachine,
                StartDate = StartDate,
                CompletedAt = CompletedAt,
                ToolingReady = ToolingReady,
                ColletsReady = ColletsReady,
                GaugingReady = GaugingReady,
                HasProgram = HasProgram,
                HasStarted = HasStarted,
                BarID = BarID,
                NumberOfBars = NumberOfBars,
                ItemNeedsCleaning = ItemNeedsCleaning,
                BarIsAllocated = BarIsAllocated,
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
                || BarIsAllocated != OtherOrder.BarIsAllocated
                || BarIsVerified != OtherOrder.BarIsVerified
                || HasProgram != OtherOrder.HasProgram
                || ToolingReady != OtherOrder.ToolingReady
                || GaugingReady != OtherOrder.GaugingReady
                || ColletsReady != OtherOrder.ColletsReady
                || POReference != OtherOrder.POReference
                || NumberOfBars != OtherOrder.NumberOfBars
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
            HasStarted = otherOrder.HasStarted;
            BarIsAllocated = otherOrder.BarIsAllocated;
            BarIsVerified = otherOrder.BarIsVerified;
            HasProgram = otherOrder.HasProgram;
            ToolingReady = otherOrder.ToolingReady;
            GaugingReady = otherOrder.GaugingReady;
            ColletsReady = otherOrder.ColletsReady;
            POReference = otherOrder.POReference;
            NumberOfBars = otherOrder.NumberOfBars;
            TimeToComplete = otherOrder.TimeToComplete;
            ModifiedAt = otherOrder.ModifiedAt;
            ModifiedBy = otherOrder.ModifiedBy;
            POReference = otherOrder.POReference;
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
