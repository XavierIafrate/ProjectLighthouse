using DocumentFormat.OpenXml.Drawing.Charts;
using LiveChartsCore;
using LiveChartsCore.Defaults;
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
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.View.Orders;
using ProjectLighthouse.ViewModel.Commands.Orders;
using ProjectLighthouse.ViewModel.Commands.Printing;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
        public List<BreakdownCode> BreakdownCodes;
        public List<MachineBreakdown> MachineBreakdowns;
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
        public List<MachineBreakdown> SelectedOrderBreakdowns { get; set; }

        public List<MachineOperatingBlock> MachineOperatingBlocks { get; set; }

        public int TotalTimeSeconds { get; set; }
        public int RunTimeSeconds { get; set; }
        public double Availability { get; set; }
        public double UnknownPct { get; set; }
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
                //Search();
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
        //public EditManufactureOrderCommand EditCommand { get; set; }
        public CreateNewOrderCommand NewOrderCommand { get; set; }
        //public GetProgramPlannerCommand GetProgramPlannerCmd { get; set; }
        #endregion

        #region Icon Brushes
        public Brush ToolingIconBrush { get; set; }
        public Brush ProgramIconBrush { get; set; }
        public Brush BarVerifiedIconBrush { get; set; }
        public Brush BarAllocatedIconBrush { get; set; }
        #endregion

        //private WorkloadWindow _workloadWindow;

        #endregion Variables

        public OrderViewModel()
        {
            //InitialiseVariables();
            //Refresh();
        }



        #region Data Refreshing


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
            if ((order.ModifiedAt ?? DateTime.MinValue).AddDays(1) > DateTime.Now)
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

        private void FilterOrders()
        {
            switch (SelectedFilter)
            {
                case "All Active":
                    FilteredOrders = Orders.Where(n => n.State < OrderState.Complete
                            //|| n.ModifiedAt.AddDays(1) > DateTime.Now // TODO fix
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

        private void LoadOrderCard()
        {
            if (SelectedOrder == null)
            {
                return;
            }

            //LoadOrderObjects();
            //SetUiElements();
            List<Lot> orderLots = Lots.Where(x => x.Order == SelectedOrder.Name).ToList();

            Task.Run(() => GetProgramCandidates());
            Task.Run(() => GetOrderBreakdowns());
        }

        private void GetOrderBreakdowns()
        {
            if (SelectedOrder is null)
            {
                SelectedOrderBreakdowns = new();
                OnPropertyChanged(nameof(SelectedOrderBreakdowns));
                return;
            }

            SelectedOrderBreakdowns = MachineBreakdowns
                .Where(x => x.OrderName == SelectedOrder.Name)
                .OrderBy(x => x.BreakdownStarted)
                .ToList();
            SelectedOrderBreakdowns
                .ForEach(x => x.BreakdownMeta = BreakdownCodes.Find(b => b.Id == x.BreakdownCode));
            OnPropertyChanged(nameof(SelectedOrderBreakdowns));
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
    }
}
