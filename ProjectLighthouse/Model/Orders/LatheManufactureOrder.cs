using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProjectLighthouse.Model.Orders
{
    public partial class LatheManufactureOrder : ScheduleItem, ICloneable
    {
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
                else if (BarIsAllocated && HasProgram && AllToolingReady && BarIsVerified) // TODO check logic here
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

        private bool toolingOrdered;
        [UpdateWatch]
        public bool ToolingOrdered
        {
            get
            {
                return toolingOrdered;
            }
            set
            {
                toolingOrdered = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

        private bool barToolingOrdered;
        [UpdateWatch]
        public bool BarToolingOrdered
        {
            get
            {
                return barToolingOrdered;
            }
            set
            {
                barToolingOrdered = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

        private bool gaugingOrdered;
        [UpdateWatch]
        public bool GaugingOrdered
        {
            get
            {
                return gaugingOrdered;
            }
            set
            {
                gaugingOrdered = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

        private bool baseProgramExists;
        [UpdateWatch]
        public bool BaseProgramExists
        {
            get
            {
                return baseProgramExists;
            }
            set
            {
                baseProgramExists = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

        private bool barIsVerified;
        [UpdateWatch]
        public bool BarIsVerified
        {
            get
            {
                return barIsVerified;
            }
            set
            {
                barIsVerified = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

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

        private bool toolingReady;
        [UpdateWatch]
        public bool ToolingReady
        {
            get
            {
                return toolingReady;
            }
            set
            {
                toolingReady = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

        private bool barToolingReady;
        [UpdateWatch]
        public bool BarToolingReady
        {
            get
            {
                return barToolingReady;
            }
            set
            {
                barToolingReady = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

        private bool gaugingReady;
        [UpdateWatch]
        public bool GaugingReady
        {
            get
            {
                return gaugingReady;
            }
            set
            {
                gaugingReady = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

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

        private bool hasProgram;
        [UpdateWatch]
        public bool HasProgram
        {
            get 
            { 
                return hasProgram; 
            }
            set
            {
                hasProgram = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(State));
            }
        }

        #endregion




        public DateTime CompletedAt { get; set; }

        private DateTime? scheduledEnd;

        [UpdateWatch]
        public DateTime? ScheduledEnd
        {
            get
            {
                return scheduledEnd;
            }
            set
            {
                scheduledEnd = value;
                OnPropertyChanged();
            }
        }

        private int timeToSet;
        [UpdateWatch]
        public int TimeToSet
        {
            get { return timeToSet; }
            set { timeToSet = value; OnPropertyChanged(); }
        }

         // MaintenanceTime :(


        public DateTime GetSettingStartDateTime()
        {
            if (DateTime.MinValue.AddHours(TimeToSet) > StartDate)
            {
                return DateTime.MinValue;
            }

            return StartDate.AddHours(TimeToSet * -1);
        }


        #region Bar Configuration
        [UpdateWatch]
        public string BarID { get; set; }

        private double numberOfBars;
        [UpdateWatch]
        public double NumberOfBars
        {
            get 
            { 
                return numberOfBars;
            }
            set
            {
                numberOfBars = value; 
                OnPropertyChanged(); 
            }
        }

        private int spareBars;
        [UpdateWatch]
        public int SpareBars
        {
            get
            {
                return spareBars;
            }
            set
            {
                spareBars = value;
                OnPropertyChanged();
            }
        }


        [UpdateWatch]
        public int NumberOfBarsIssued { get; set; }
        public double BarsInStockAtCreation { get; set; }
        #endregion

        public double MajorDiameter { get; set; }
        public int GroupId { get; set; }
        public int MaterialId { get; set; }

        private bool isResearch;
        [UpdateWatch]
        public bool IsResearch {

            get 
            { 
                return isResearch; 
            }
            set
            {
                isResearch = value;
                OnPropertyChanged();
            }
        }


        private string requiredFeatures;
        [UpdateWatch]
        public string RequiredFeatures
        {
            get { return requiredFeatures; }
            set 
            { 
                requiredFeatures = value; 
                OnPropertyChanged(); 
            }
        }

        [Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<string> RequiredFeaturesList
        {
            get
            {
                if (RequiredFeatures is null) return new();
                return RequiredFeatures.Split(";").ToList();
            }
            set
            {
                if (value.Count > 0)
                {
                    RequiredFeatures = string.Join(";", value);
                    OnPropertyChanged();
                    return;
                }
                RequiredFeatures = null;
                OnPropertyChanged();
            }
        }



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

        private BarStock bar;
        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public BarStock Bar
        {
            get
            {
                return bar;
            }
            set
            {
                bar = value;
                OnPropertyChanged();
            }
        }

        private List<LatheManufactureOrderItem> orderItems;

        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<LatheManufactureOrderItem> OrderItems
        {
            get { return orderItems; }
            set { orderItems = value; OnPropertyChanged(); }
        }


        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<TechnicalDrawing> Drawings { get; set; }

        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<OrderDrawing> DrawingsReferences { get; set; }

        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public Lathe AssignedLathe { get; set; }

        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<BarIssue> BarIssues { get; set; }

        private Product product;
        [Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public Product Product
        {
            get { return product; }
            set { product = value; OnPropertyChanged(); }
        }

        private ProductGroup productGroup;
        [Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public ProductGroup ProductGroup
        {
            get { return productGroup; }
            set { productGroup = value; OnPropertyChanged(); }
        }


        [SQLite.Ignore]
        [CsvHelper.Configuration.Attributes.Ignore]
        public List<MachineBreakdown> Breakdowns { get; set; } = new();

        public override object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<LatheManufactureOrder>(serialised);
        }

        public override string ToString()
        {
            return Name;
        }

        public bool RequiresBar()
        {
            return DateTime.Now.AddDays(App.Constants.BarRequisitionDays) > StartDate
                && StartDate.Year != DateTime.MinValue.Year && NumberOfBars > NumberOfBarsIssued;
        }

        public DateTime GetStartDeadline()
        {

            List<LatheManufactureOrderItem> items = OrderItems
                .Where(x => x.RequiredQuantity > 0 && x.DateRequired > DateTime.MinValue)
                .OrderByDescending(x => x.DateRequired)
                .ToList();

            if (items.Count == 0)
            {
                return DateTime.MaxValue;
            }

            DateTime cursor = DateTime.MaxValue;

            for (int i = 0; i < items.Count; i++)
            {
                int secondsToMake = items[i].GetTimeToMakeRequired();

                if (cursor > items[i].DateRequired.ChangeTime(12, 0, 0, 0))
                {
                    cursor = items[i].DateRequired.ChangeTime(12, 0, 0, 0);
                }

                cursor = cursor.AddSeconds(secondsToMake * -1);
            }

            return cursor;
        }

        internal TimeSpan GetTimeToMakeRequired()
        {
            TimeSpan result = TimeSpan.Zero;

            for (int i = 0; i < OrderItems.Count; i++)
            {
                result = result.Add(TimeSpan.FromSeconds(OrderItems[i].GetTimeToMakeRequired()));
            }

            return result;
        }


        public void MarkAsClosed()
        {
            DatabaseHelper.ExecuteCommand($"UPDATE {nameof(LatheManufactureOrder)} SET IsClosed = {true} WHERE Id={Id}");
        }

        public void MarkAsNotClosed()
        {
            DatabaseHelper.ExecuteCommand($"UPDATE {nameof(LatheManufactureOrder)} SET IsClosed = {false} WHERE Id={Id}");
        }

        public override DateTime EndsAt()
        {
            if (ScheduledEnd is not null)
            {
                return (DateTime)ScheduledEnd;
            }
            else if (IsResearch)
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
