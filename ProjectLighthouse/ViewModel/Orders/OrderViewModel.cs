using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Analytics;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.Model.Reporting;
using ProjectLighthouse.View.Orders;
using ProjectLighthouse.ViewModel.Commands.Orders;
using ProjectLighthouse.ViewModel.Commands.Printing;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ViewModel.Commands.Orders;
using ViewModel.Helpers;
using DateTimePoint = LiveChartsCore.Defaults.DateTimePoint;

namespace ProjectLighthouse.ViewModel.Orders
{
    public class OrderViewModel : BaseViewModel
    {
        #region Variables

        #region Main Dataset
        public List<LatheManufactureOrder> Orders { get; set; }
        public List<LatheManufactureOrderItem> OrderItems { get; set; }
        public List<Note> Notes { get; set; }
        public List<TechnicalDrawing> Drawings { get; set; }
        public List<OrderDrawing> OrderDrawings { get; set; }
        public List<Lathe> Lathes { get; set; }
        public List<MachineStatistics> MachineStatistics { get; set; }
        public List<Lot> Lots { get; set; }
        public List<BarStock> BarStock { get; set; }
        public List<MaterialInfo> MaterialInfo { get; set; }

        public List<Product> Products { get; set; }
        public List<ProductGroup> ProductGroups { get; set; }
        public List<NcProgram> Programs;
        #endregion

        #region Observable
        private List<LatheManufactureOrder> filteredOrders;

        public List<LatheManufactureOrder> FilteredOrders
        {
            get { return filteredOrders; }
            set
            {
                filteredOrders = value;
                OnPropertyChanged();
            }
        }

        public List<LatheManufactureOrderItem> FilteredOrderItems { get; set; }
        public List<Note> FilteredNotes { get; set; }
        public List<TechnicalDrawing> FilteredDrawings { get; set; }
        public BarStock SelectedOrderBar { get; set; }
        public ProductGroup SelectedProductGroup { get; set; }
        public Product SelectedProduct { get; set; }
        public List<NcProgram> ProgramCandidates { get; set; }
        #endregion

        #region Charting
        public Axis[] XAxes { get; set; } =
        {
            new Axis
            {
                Labeler = value => new DateTime((long) Math.Max(0, value)).ToString("dd/MM"),
                LabelsRotation = 0,
                UnitWidth = TimeSpan.FromDays(1).Ticks,
                MinStep = TimeSpan.FromDays(1).Ticks,
                ForceStepToMin=true

            }
        };

        public Axis[] YAxes { get; set; } =
        {
            new Axis
            {
                MinLimit = 0
            }
        };

        public List<ISeries> Series { get; set; }

        public Axis[] CycleTimeYAxes { get; set; } =
        {
            new Axis
            {
                Labeler = value => $"{Math.Floor(value/60):0}m {value%60}s",
                MinLimit = 0,
                MinStep= 30,
            }
        };

        public Axis[] CycleTimeXAxes { get; set; } =
        {
            new Axis
            {
                Labeler = value => new DateTime((long) Math.Max(0, value)).ToString("MMM yy"),
                LabelsRotation = 0,

            }
        };
        public List<ISeries> CycleTimeSeries { get; set; }

        private string? runBeforeText;
        public string? RunBeforeText
        {
            get { return runBeforeText; }
            set
            {
                runBeforeText = value;
                OnPropertyChanged();
            }
        }


        private bool runInMaterialBefore;
        public bool RunInMaterialBefore
        {
            get { return runInMaterialBefore; }
            set
            {
                runInMaterialBefore = value;
                OnPropertyChanged();
            }
        }

        private string lastMadeText;
        public string LastMadeText
        {
            get { return lastMadeText; }
            set
            {
                lastMadeText = value;
                OnPropertyChanged();
            }
        }



        #endregion

        #region User Demands
        private LatheManufactureOrder selectedOrder;
        public LatheManufactureOrder SelectedOrder
        {
            get { return selectedOrder; }
            set
            {
                selectedOrder = value;
                LoadOrderCard();
                OnPropertyChanged();
            }
        }

        private MachineStatistics displayStats;
        public MachineStatistics DisplayStats
        {
            get { return displayStats; }
            set
            {
                displayStats = value;
                OnPropertyChanged();
            }
        }

        private string selectedFilter = "All Active";
        public string SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                selectedFilter = value;
                FilterOrders();
                OnPropertyChanged(nameof(FilteredOrders));
            }
        }

        private string searchTerm;
        public string SearchTerm
        {
            get { return searchTerm; }
            set
            {
                searchTerm = value;
                Search();
                OnPropertyChanged();
            }
        }
        #endregion

        #region Visibility variables


        public bool NoNotes { get; set; }


        private Visibility modifiedVis;
        public Visibility ModifiedVis
        {
            get { return modifiedVis; }
            set
            {
                modifiedVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility archiveVis;
        public Visibility ArchiveVis
        {
            get { return archiveVis; }
            set
            {
                archiveVis = value;
                OnPropertyChanged();
            }
        }


        #endregion Visibility variables

        #region Commands
        public PrintOrderCommand PrintOrderCommand { get; set; }
        public EditManufactureOrderCommand EditCommand { get; set; }
        public CreateNewOrderCommand NewOrderCommand { get; set; }
        public GetProgramPlannerCommand GetProgramPlannerCmd { get; set; }
        #endregion

        #region Icon Brushes
        public Brush ToolingIconBrush { get; set; }
        public Brush ProgramIconBrush { get; set; }
        public Brush BarVerifiedIconBrush { get; set; }
        public Brush BarAllocatedIconBrush { get; set; }
        #endregion

        #endregion Variables

        public OrderViewModel()
        {
            InitialiseVariables();
            Refresh();
        }

        private void LoadProductionChart(List<Lot> stockLots, DateTime startingDate)
        {
            if (stockLots.Count == 0)
            {
                Series = null;
                OnPropertyChanged(nameof(Series));
                return;
            }

            var producedValues = new List<DateTimePoint>();
            var scrappedValues = new List<DateTimePoint>();

            int totalProduced = 0;
            int totalScrapped = 0;

            TimeSpan span = stockLots.Max(x => x.DateProduced) - startingDate;

            int daysOfSpan = (int)Math.Ceiling(span.TotalDays);

            if (daysOfSpan > 90)
            {
                Series = null;
                OnPropertyChanged(nameof(Series));
                return;
            }

            string[] labels = new string[daysOfSpan];

            for (int i = 0; i < daysOfSpan; i++)
            {
                DateTime day = startingDate.AddDays(i).Date;
                labels[i] = day.ToString("dd/MM");

                totalProduced += stockLots.Where(x => x.IsAccepted && x.DateProduced.Date == day).Sum(x => x.Quantity);
                totalScrapped += stockLots.Where(x => x.IsReject && x.DateProduced.Date == day).Sum(x => x.Quantity);


                producedValues.Add(new(day, totalProduced));
                scrappedValues.Add(new(day, totalScrapped));
            }

            if (totalProduced + totalScrapped == 0)
            {
                Series = null;
                OnPropertyChanged(nameof(Series));
                return;
            }


            if ((Brush)Application.Current.Resources["Blue"] is not SolidColorBrush blue
                || (Brush)Application.Current.Resources["Red"] is not SolidColorBrush red) return;

            var produced = new ColumnSeries<DateTimePoint>
            {
                Values = producedValues,
                Stroke = null,
                Padding = 2,
                Name = "Total Produced",
                Fill = new SolidColorPaint(new SKColor(blue.Color.R, blue.Color.G, blue.Color.B))
            };

            var scrapped = new ColumnSeries<DateTimePoint>
            {
                Values = scrappedValues,
                Stroke = null,
                Padding = 2,
                Name = "Total Scrapped",
                Fill = new SolidColorPaint(new SKColor(red.Color.R, red.Color.G, red.Color.B))
            };


            Series = new List<ISeries> { scrapped, produced };
            OnPropertyChanged(nameof(Series));
        }


        private void LoadBriefing()
        {
            if (SelectedOrder is null || SelectedProductGroup is null)
            {
                RunBeforeText = null;
                OnPropertyChanged(nameof(RunBeforeText));
                CycleTimeSeries = null;
                OnPropertyChanged(nameof(CycleTimeSeries));
                return;
            }
         
            int MaterialId = SelectedOrder.MaterialId;
            int GroupId = SelectedProductGroup.Id;

            if (SelectedProductGroup is not null)
            {
                SelectedProduct = Products.Find(x => x.Id == SelectedProductGroup.ProductId);
            }
            else
            {
                SelectedProduct = null;
                CycleTimeSeries = null;
                RunBeforeText = null;

                OnPropertyChanged(nameof(RunBeforeText));
                OnPropertyChanged(nameof(SelectedProduct));
                OnPropertyChanged(nameof(CycleTimeSeries));
                OnPropertyChanged(nameof(SelectedProductGroup));
                return;
            }

            OnPropertyChanged(nameof(SelectedProductGroup));
            OnPropertyChanged(nameof(SelectedProduct));

            List<LatheManufactureOrder> otherOrders = Orders
                .Where(x =>
                    x.GroupId == GroupId &&
                    x.Name != SelectedOrder.Name &&
                    x.State == OrderState.Complete &&
                    x.StartDate < SelectedOrder.CreatedAt &&
                   x.StartDate.Date > DateTime.MinValue)
                .OrderBy(x => x.StartDate)
                .ToList();

            if (otherOrders.Count == 0)
            {
                RunBeforeText = null;
                OnPropertyChanged(nameof(RunBeforeText));
                CycleTimeSeries = null;
                OnPropertyChanged(nameof(CycleTimeSeries));
                return;
            }

            LatheManufactureOrder mostRecent = otherOrders.Last();
            int runBeforeCount = otherOrders.Count;
            RunInMaterialBefore = otherOrders.Any(x => x.MaterialId == MaterialId);

            DateTime lastMadeDate = mostRecent.StartDate;


            if (lastMadeDate.AddMonths(11) > DateTime.Now)
            {
                LastMadeText = $"Last made in {lastMadeDate:MMMM}";
            }
            else
            {
                LastMadeText = $"Last made in {lastMadeDate:MMM yy}";
            }

            if (runBeforeCount == 1)
            {
                RunBeforeText = $"This archetype has been set once before";
            }
            else if (runBeforeCount == 2)
            {
                RunBeforeText = $"This archetype has been set twice before";
            }
            else
            {
                RunBeforeText = $"This archetype has been set {otherOrders.Count:0} times";
            }

            OnPropertyChanged(nameof(RunBeforeText));

            Dictionary<MaterialInfo, List<DateTimePoint>> series = new();

            foreach (LatheManufactureOrder order in otherOrders)
            {
                MaterialInfo? material = MaterialInfo.Find(x => x.Id == order.MaterialId);

                if (material is null) continue;

                List<LatheManufactureOrderItem> itemsInvolved = OrderItems.Where(x => order.Name == x.AssignedMO && x.CycleTime > 0).ToList();
                int[] uniqueTimes = itemsInvolved.Select(x => x.CycleTime).Distinct().ToArray();

                if (!series.ContainsKey(material))
                {
                    series.Add(material, new());
                }

                foreach (int time in uniqueTimes)
                {
                    series[material].Add(new(order.StartDate, time));
                }
            }
            CycleTimeSeries = null;
            OnPropertyChanged(nameof(CycleTimeSeries));

            if (otherOrders.Count > 2)
            {
                //SolidColorBrush purple = (Brush)Application.Current.Resources["Purple"] as SolidColorBrush;
                CycleTimeSeries = new List<ISeries>();

                foreach (KeyValuePair<MaterialInfo, List<DateTimePoint>> s in series)
                {
                    ScatterSeries<DateTimePoint> newSeries = new()
                    {
                        Values = s.Value,
                        Name = s.Key.MaterialCode,
                        Stroke = null,
                        TooltipLabelFormatter = (chartPoint) =>
                        $"[{s.Key.MaterialCode}] {new DateTime((long)chartPoint.SecondaryValue):dd/MM/yy}: {Math.Floor(chartPoint.PrimaryValue / 60):0}m {chartPoint.PrimaryValue % 60}s",

                    };


                    CycleTimeSeries.Add(newSeries);
                }
            }
            OnPropertyChanged(nameof(CycleTimeSeries));
        }

        private void InitialiseVariables()
        {
            FilteredDrawings = new();
            FilteredNotes = new();
            FilteredOrderItems = new();
            FilteredOrders = new();

            Notes = new();
            Orders = new();
            MachineStatistics = new();
            OrderItems = new();
            DisplayStats = new();
            Drawings = new();
            OrderDrawings = new();
            Lathes = new();
            Lots = new();
            BarStock = new();
            MaterialInfo = new();
            SelectedOrderBar = new();
            Products = new();
            ProductGroups = new();
            ProgramCandidates = new();

            ToolingIconBrush = (Brush)Application.Current.Resources["Red"];
            ProgramIconBrush = (Brush)Application.Current.Resources["Red"];
            BarVerifiedIconBrush = (Brush)Application.Current.Resources["Red"];
            BarAllocatedIconBrush = (Brush)Application.Current.Resources["Red"];

            PrintOrderCommand = new(this);
            EditCommand = new(this);
            NewOrderCommand = new(this);
            GetProgramPlannerCmd = new(this);
        }

        #region Data Refreshing

        public void Refresh()
        {
            // Store user selection
            int? userSelection = null;
            if (SelectedOrder != null)
            {
                userSelection = SelectedOrder.Id;
            }

            LoadData();
            CheckForClosedOrders();
            CheckForReopenedOrders();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Search();
            }
            else
            {
                FilterOrders();
            }

            if (userSelection is not null)
            {
                SelectedOrder = FilteredOrders.Find(x => x.Id == userSelection);

                if (SelectedOrder is null && FilteredOrders.Count > 0)
                {
                    SelectedOrder = FilteredOrders[0];
                }
            }
            else if (FilteredOrders.Count > 0)
            {
                SelectedOrder = FilteredOrders[0];
            }

        }

        private void CheckForReopenedOrders()
        {
            List<LatheManufactureOrder> notClosed = Orders.Where(x => x.IsClosed && x.State < OrderState.Complete).ToList();
            for (int i = 0; i < notClosed.Count; i++)
            {
                notClosed[i].MarkAsNotClosed();
                Orders.Find(x => x.Id == notClosed[i].Id)!.IsClosed = false;
            }
        }

        private void CheckForClosedOrders()
        {
            List<LatheManufactureOrder> doneButNotClosed = Orders.Where(x => x.State > OrderState.Running && !x.IsClosed).ToList();
            for (int i = 0; i < doneButNotClosed.Count; i++)
            {
                LatheManufactureOrder order = doneButNotClosed[i];
                List<LatheManufactureOrderItem> items = OrderItems.Where(x => x.AssignedMO == order.Name).ToList();
                List<Lot> lots = Lots.Where(x => x.Order == order.Name).ToList();

                if (!OrderCanBeClosed(order, items, lots))
                {
                    continue;
                }

                order.MarkAsClosed();
                Orders.Find(x => x.Id == doneButNotClosed[i].Id)!.IsClosed = true;
            }
        }

        static bool OrderCanBeClosed(LatheManufactureOrder order, List<LatheManufactureOrderItem> items, List<Lot> lots)
        {
            if (order.ModifiedAt.AddDays(1) > DateTime.Now)
            {
                return false;
            }

            if (order.State > OrderState.Running)
            {
                List<LatheManufactureOrderItem> itemsWithBadCycleTimes = items.Where(i => i.CycleTime == 0 && i.QuantityMade > 0).ToList();
                List<Lot> unresolvedLots = lots.Where(l => l.Quantity != 0 && !l.IsDelivered && !l.IsReject && l.AllowDelivery).ToList();

                return itemsWithBadCycleTimes.Count == 0 // ensure cycle time is updated
                    && unresolvedLots.Count == 0; // ensure lots are fully processed
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Loading
        private void LoadData()
        {
            Orders = DatabaseHelper.Read<LatheManufactureOrder>().ToList();

            OrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();

            Notes = DatabaseHelper.Read<Note>().ToList();

            Drawings = DatabaseHelper.Read<TechnicalDrawing>().ToList();

            OrderDrawings = DatabaseHelper.Read<OrderDrawing>().ToList();

            Lathes = DatabaseHelper.Read<Lathe>().ToList();

            Lots = DatabaseHelper.Read<Lot>().ToList();

            Products = DatabaseHelper.Read<Product>().ToList();
            ProductGroups = DatabaseHelper.Read<ProductGroup>().ToList();

            Programs = DatabaseHelper.Read<NcProgram>().Where(x => !x.Inactive).ToList();

            BarStock = DatabaseHelper.Read<BarStock>().ToList();
            MaterialInfo = DatabaseHelper.Read<MaterialInfo>().ToList();
            BarStock.ForEach(x => x.MaterialData = MaterialInfo.Find(y => y.Id == x.MaterialId));

            MachineStatistics = MachineStatsHelper.GetStats();
        }

        private void FilterOrders()
        {
            switch (SelectedFilter)
            {
                case "All Active":
                    FilteredOrders = Orders.Where(n => n.State < OrderState.Complete
                            || n.ModifiedAt.AddDays(1) > DateTime.Now
                            || !n.IsClosed)
                        .OrderBy(x => x.State != OrderState.Running)
                        .ThenBy(x => x.State == OrderState.Running ? x.AllocatedMachine : "")
                        .ToList();
                    break;

                case "Assigned To Me":
                    FilteredOrders = Orders
                        .Where(x => !x.IsClosed && x.AssignedTo == App.CurrentUser.UserName)
                        .Take(200)
                        .ToList();
                    break;

                case "Not Ready":
                    FilteredOrders = Orders.Where(n => n.State == OrderState.Problem).ToList();
                    break;

                case "No Program":
                    FilteredOrders = Orders.Where(n => !n.HasProgram && n.State < OrderState.Complete && n.StartDate > DateTime.MinValue)
                        .OrderBy(x => x.StartDate)
                        .ToList();
                    break;

                case "Ready":
                    FilteredOrders = Orders.Where(n => n.State == OrderState.Ready || n.State == OrderState.Prepared).ToList();
                    break;

                case "Complete":
                    FilteredOrders = Orders.Where(n => n.State > OrderState.Running && n.IsClosed).OrderByDescending(n => n.ModifiedAt).Take(200).ToList();
                    break;

                case "Development":
                    FilteredOrders = Orders
                        .Where(n => n.IsResearch && n.State < OrderState.Complete)
                        .OrderByDescending(n => n.CreatedAt)
                        .Take(200)
                        .ToList();
                    break;

                case "All":
                    FilteredOrders = Orders.OrderByDescending(n => n.CreatedAt).Take(200).ToList();
                    break;
            }

            if (FilteredOrders.Count > 0)
            {
                SelectedOrder = FilteredOrders.First();
            }
        }

        #endregion Loading

        public void Search()
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                Refresh();
                return;
            }

            List<LatheManufactureOrder> Results = new();
            List<string> FoundOrders = new();

            string searchToken = SearchTerm.ToUpperInvariant();

            foreach (LatheManufactureOrder order in Orders)
            {
                if (order.Name.Contains(searchToken))
                {
                    Results.Add(order);
                    FoundOrders.Add(order.Name);
                    continue;
                }

                if (order.BarID.Contains(searchToken))
                {
                    Results.Add(order);
                    FoundOrders.Add(order.Name);
                    continue;
                }

                if (order.AssignedTo?.ToUpperInvariant() == searchToken)
                {
                    Results.Add(order);
                    FoundOrders.Add(order.Name);
                    continue;
                }

                if (order.POReference != null)
                {
                    if (order.POReference.Contains(searchToken) && order.POReference != "N/A")
                    {
                        Results.Add(order);
                        FoundOrders.Add(order.Name);
                        continue;
                    }
                }
            }

            List<string> FoundOrdersByItem = new();
            foreach (LatheManufactureOrderItem item in OrderItems)
            {
                if (FoundOrders.Contains(item.AssignedMO))
                {
                    continue;
                }

                if (item.ProductName.Contains(searchToken))
                {
                    FoundOrdersByItem.Add(item.AssignedMO);
                    continue;
                }
            }

            Results.AddRange(Orders.Where(x => FoundOrdersByItem.Contains(x.Name)));

            Results = Results.OrderByDescending(x => x.ModifiedAt).ToList();

            FilteredOrders = Results;

            if (FilteredOrders.Count > 0)
            {
                SelectedOrder = FilteredOrders.First();
            }
        }

        private void LoadOrderCard()
        {
            if (SelectedOrder == null)
            {
                return;
            }

            LoadOrderObjects();
            SetUiElements();
            List<Lot> orderLots = Lots.Where(x => x.Order == SelectedOrder.Name).ToList();
            Task.Run(() => LoadProductionChart(orderLots, SelectedOrder.StartDate.Date));
            Task.Run(() => LoadBriefing());
            Task.Run(() => GetProgramCandidates());
        }

        void GetProgramCandidates()
        {
            if (SelectedProductGroup is null)
            {
                ProgramCandidates = new();
                return;
            }

            ProgramCandidates = GetPrograms(SelectedProductGroup, SelectedOrder.MaterialId);
            OnPropertyChanged(nameof(ProgramCandidates));
        }

        private List<NcProgram> GetPrograms(ProductGroup productGroup, int materialId)
        {
            List<NcProgram> candidates = Programs
                .Where(x => x.ProductStringIds.Contains($"{productGroup.ProductId:0}"))
                .Where(x => x.GroupStringIds.Count == 0 || x.GroupStringIds.Contains($"{productGroup.Id:0}"))
                .Where(x => x.MaterialsList.Count == 0 || x.MaterialsList.Contains($"{materialId:0}"))
                .OrderByDescending(x => x.GroupStringIds.Count)
                .ThenByDescending(x => x.MaterialsList.Count)
                .ThenBy(x => x.Name.Length)
                .ThenBy(x => x.Name)
                .ToList();

            return candidates;
        }

        private void SetUiElements()
        {
            if (SelectedOrder == null)
            {
                return;
            }

            // Quick indicator icons
            ProgramIconBrush = (Brush)Application.Current.Resources[SelectedOrder.HasProgram ? "Green" : SelectedOrder.BaseProgramExists ? "Orange" : "Red"];
            OnPropertyChanged(nameof(ProgramIconBrush));

            ToolingIconBrush = (Brush)Application.Current.Resources[SelectedOrder.AllToolingReady ? "Green" : (SelectedOrder.ToolingOrdered || SelectedOrder.ToolingReady)
                    && (SelectedOrder.BarToolingOrdered || SelectedOrder.BarToolingReady)
                    && (SelectedOrder.GaugingOrdered || SelectedOrder.GaugingReady) ? "Orange" : "Red"];
            OnPropertyChanged(nameof(ToolingIconBrush));

            BarVerifiedIconBrush = (Brush)Application.Current.Resources[SelectedOrder.BarIsVerified ? "Green" : "Red"];
            OnPropertyChanged(nameof(BarVerifiedIconBrush));

            BarAllocatedIconBrush = (Brush)Application.Current.Resources[SelectedOrder.BarIsAllocated ? "Green" : "Red"];
            OnPropertyChanged(nameof(BarAllocatedIconBrush));


            ModifiedVis = string.IsNullOrEmpty(SelectedOrder.ModifiedBy)
               ? Visibility.Collapsed
               : Visibility.Visible;

            SetLiveMachineInfo();

            ArchiveVis = SelectedOrder.IsClosed
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void SetLiveMachineInfo()
        {
            if (SelectedOrder is null)
            {
                DisplayStats = null;
                return;
            }

            if (SelectedOrder.State != OrderState.Running)
            {
                DisplayStats = null;
                return;
            }


            DisplayStats = MachineStatistics.Find(x => x.MachineID == SelectedOrder.AllocatedMachine);

            if (DisplayStats is null)
            {
                return;
            }

            if (DisplayStats.DataTime.AddMinutes(30) < DateTime.Now)
            {
                DisplayStats = null;
            }
        }

        private void LoadOrderObjects()
        {
            // Order Items
            FilteredOrderItems.Clear();
            FilteredOrderItems = OrderItems
                .Where(i => i.AssignedMO == SelectedOrder.Name)
                .OrderByDescending(n => n.RequiredQuantity)
                .ThenBy(n => n.ProductName)
                .ToList();


            // Order Notes
            FilteredNotes = Notes
                .Where(n =>
                    n.DocumentReference == SelectedOrder.Name)
                .OrderBy(x => x.Id) // Time is not synchronised
                .ToList();

            // Order Drawings
            FilteredDrawings.Clear();
            int[] drawings = OrderDrawings
                .Where(x => x.OrderId == SelectedOrder.Name)
                .Select(x => x.DrawingId)
                .ToArray();

            FilteredDrawings = Drawings
                .Where(d => drawings.Contains(d.Id))
                .ToList();

            SelectedOrderBar = BarStock.Find(x => x.Id == SelectedOrder.BarID);
            SelectedProductGroup = ProductGroups.Find(x => x.Id == SelectedOrder.GroupId);


            OnPropertyChanged(nameof(FilteredOrderItems));
            OnPropertyChanged(nameof(FilteredNotes));
            OnPropertyChanged(nameof(FilteredDrawings));
            OnPropertyChanged(nameof(SelectedOrderBar));
        }

        public void PrintSelectedOrder()
        {
            ReportPdf reportService = new();
            OrderPrintoutData reportData = new()
            {
                Order = SelectedOrder,
                Items = FilteredOrderItems.ToArray(),
                Notes = FilteredNotes.ToArray()
            };

            string path = GetTempPdfPath();

            reportService.Export(path, reportData);
            reportService.OpenPdf(path);
        }

        private static string GetTempPdfPath()
        {
            return System.IO.Path.GetTempFileName() + ".pdf";
        }

        public void CreateProgramPlanner()
        {
            List<LatheManufactureOrder> ordersNeedingProgramming = Orders
                .Where(x =>
                     x.StartDate != DateTime.MinValue &&
                     x.State < OrderState.Complete)
                .OrderBy(x => x.StartDate)
                .ToList();

            // Adds unscheduled items to the bottom
            ordersNeedingProgramming
                .AddRange(
                    Orders.Where(x =>
                         x.StartDate == DateTime.MinValue &&
                         x.State < OrderState.Complete));

            ExcelHelper.CreateProgrammingPlanner(ordersNeedingProgramming);

            //CSVHelper.WriteListToCSV(Orders, "orders");
        }

        public void EditLMO()
        {
            if (SelectedOrder == null)
            {
                return;
            }

            bool editable = true;

            // TODO optimise for R&D
            if (SelectedOrder.ModifiedAt.AddDays(14) < DateTime.Now && SelectedOrder.State >= OrderState.Complete)
            {
                editable = false;
            }

            if (!App.CurrentUser.HasPermission(PermissionType.UpdateOrder))
            {
                editable = false;
            }

            EditLMOWindow editWindow = new(SelectedOrder.Name, editable)
            {
                Owner = Application.Current.MainWindow
            };
            editWindow.ShowDialog();

            Refresh();
        }

        public void CreateNewOrder()
        {
            OrderConstructorWindow window = new()
            {
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            Refresh();
        }
    }
}