﻿using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Analytics;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Reporting;
using ProjectLighthouse.View;
using ProjectLighthouse.View.Orders;
using ProjectLighthouse.ViewModel.Commands.Orders;
using ProjectLighthouse.ViewModel.Commands.Printing;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;

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
        #endregion

        #region Observable
        public ObservableCollection<LatheManufactureOrder> FilteredOrders { get; set; }
        public List<LatheManufactureOrderItem> FilteredOrderItems { get; set; }
        public List<Note> FilteredNotes { get; set; }
        public List<TechnicalDrawing> FilteredDrawings { get; set; }
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

        private string runInfoText;
        public string RunInfoText
        {
            get { return runInfoText; }
            set
            {
                runInfoText = value;
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
                searchTerm = value.ToUpper();
                OnPropertyChanged();
                Search();
            }
        }
        #endregion

        #region Visibility variables

        private bool liveInfoAvailable;
        public bool LiveInfoAvailable
        {
            get { return liveInfoAvailable; }
            set
            {
                liveInfoAvailable = value;
                OnPropertyChanged();
            }
        }

        public bool NoNotes { get; set; }

        private Visibility cardVis = Visibility.Hidden;
        public Visibility CardVis
        {
            get { return cardVis; }
            set
            {
                cardVis = value;
                OnPropertyChanged();

                NothingVis = value == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                OnPropertyChanged(nameof(NothingVis));
            }
        }

        public Visibility NothingVis { get; set; }

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

        private Visibility drawingsFoundVis;
        public Visibility DrawingsFoundVis
        {
            get { return drawingsFoundVis; }
            set
            {
                drawingsFoundVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility noDrawingsFoundVis;
        public Visibility NoDrawingsFoundVis
        {
            get { return noDrawingsFoundVis; }
            set
            {
                noDrawingsFoundVis = value;
                OnPropertyChanged();
            }
        }

        #endregion Visibility variables

        #region Commands
        public PrintOrderCommand PrintOrderCommand { get; set; }
        public EditManufactureOrderCommand EditCommand { get; set; }

        public CreateNewOrderCommand NewOrderCommand { get; set; }
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

        private void InitialiseVariables()
        {
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

            FilteredDrawings = new();
            FilteredNotes = new();
            FilteredOrderItems = new();
            FilteredOrders = new();

            ToolingIconBrush = (Brush)Application.Current.Resources["Red"];
            ProgramIconBrush = (Brush)Application.Current.Resources["Red"];
            BarVerifiedIconBrush = (Brush)Application.Current.Resources["Red"];
            BarAllocatedIconBrush = (Brush)Application.Current.Resources["Red"];

            PrintOrderCommand = new(this);
            EditCommand = new(this);
            NewOrderCommand = new(this);
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

            // If available, scope to user selection
            if (userSelection != null)
            {
                if (FilteredOrders.Any(x => x.Id == userSelection))
                {
                    SelectedOrder = FilteredOrders.First(x => x.Id == userSelection);
                }
            }
        }

        private void CheckForReopenedOrders()
        {
            List<LatheManufactureOrder> notClosed = Orders.Where(x => x.IsClosed && x.State < OrderState.Complete).ToList();
            for (int i = 0; i < notClosed.Count; i++)
            {
                notClosed[i].MarkAsNotClosed();
                Orders.Find(x => x.Id == notClosed[i].Id).IsClosed = false;
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
                Orders.Find(x => x.Id == doneButNotClosed[i].Id).IsClosed = true;
            }
        }

        private bool OrderCanBeClosed(LatheManufactureOrder order, List<LatheManufactureOrderItem> items, List<Lot> lots)
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

            BarStock = DatabaseHelper.Read<BarStock>().ToList();

            MachineStatistics = MachineStatsHelper.GetStats();
        }

        private void FilterOrders()
        {
            FilteredOrders.Clear();

            List<LatheManufactureOrder> orders = new();
            switch (SelectedFilter)
            {
                case "All Active":
                    orders = Orders.Where(x => x.State == OrderState.Running)
                        .OrderBy(x => x.AllocatedMachine)
                        .ToList();
                    orders.AddRange(
                        Orders.Where(n => n.State < OrderState.Complete
                        || n.ModifiedAt.AddDays(1) > DateTime.Now
                        || !n.IsClosed)
                        .Where(o => o.State != OrderState.Running)
                        .ToList());
                    break;

                case "Not Ready":
                    orders = Orders.Where(n => n.State == OrderState.Problem).ToList();
                    break;

                case "No Program":
                    orders = Orders.Where(n => !n.HasProgram && n.State < OrderState.Complete && n.StartDate > DateTime.MinValue)
                        .OrderBy(x => x.StartDate)
                        .ToList();
                    break;

                case "Ready":
                    orders = Orders.Where(n => n.State == OrderState.Ready || n.State == OrderState.Prepared).ToList();
                    break;

                case "Complete":
                    orders = Orders.Where(n => n.State > OrderState.Running && n.IsClosed).OrderByDescending(n => n.ModifiedAt).ToList();
                    break;

                case "Development":
                    orders = Orders.Where(n => n.IsResearch).OrderByDescending(n => n.CreatedAt).ToList();
                    break;

                case "All":
                    orders = Orders.OrderByDescending(n => n.CreatedAt).ToList();
                    break;
            }

            for (int i = 0; i < orders.Count; i++)
            {
                FilteredOrders.Add(orders[i]);
            }

            if (FilteredOrders.Count > 0) SelectedOrder = FilteredOrders.First();
            OnPropertyChanged(nameof(FilteredOrders));
        }

        #endregion Loading

        public void Search() // use linq
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                Refresh();
                return;
            }

            List<LatheManufactureOrder> Results = new();
            List<string> FoundOrders = new();

            foreach (LatheManufactureOrder order in Orders)
            {
                if (order.Name.Contains(SearchTerm))
                {
                    Results.Add(order);
                    FoundOrders.Add(order.Name);
                    continue;
                }

                if (order.POReference != null)
                {
                    if (order.POReference.Contains(SearchTerm) && order.POReference != "N/A")
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

                if (item.ProductName.Contains(SearchTerm))
                {
                    FoundOrdersByItem.Add(item.AssignedMO);
                    continue;
                }
            }

            Results.AddRange(Orders.Where(x => FoundOrdersByItem.Contains(x.Name)));

            Results = Results.OrderByDescending(x => x.ModifiedAt).ToList();

            FilteredOrders.Clear();
            for (int i = 0; i < Results.Count; i++)
            {
                FilteredOrders.Add(Results[i]);
            }
            //CardVis = FilteredOrders.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            OnPropertyChanged(nameof(FilteredOrders));
            if (FilteredOrders.Count > 0) SelectedOrder = FilteredOrders.First();
        }

        private void LoadOrderCard()
        {
            if (SelectedOrder == null)
            {
                CardVis = Visibility.Hidden; // implicitly sets NothingVis
                return;
            }
            else
            {
                CardVis = Visibility.Visible;
            }

            LoadOrderObjects();
            SetUiElements();
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

            NoDrawingsFoundVis = FilteredDrawings.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            DrawingsFoundVis = FilteredDrawings.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            NoNotes = FilteredNotes.Count == 0;
            OnPropertyChanged(nameof(NoNotes));

            RunInfoText = !string.IsNullOrEmpty(SelectedOrder.AllocatedMachine)
                ? $"Assigned to {SelectedOrder.AllocatedMachine}, starting {SelectedOrder.StartDate:dddd, MMMM d}{GetDaySuffix(SelectedOrder.StartDate.Day)}"
                : "Not scheduled";
        }

        private void SetLiveMachineInfo()
        {
            DisplayStats = MachineStatistics.Find(x => x.MachineID == SelectedOrder.AllocatedMachine);
            if (DisplayStats == null || SelectedOrder.State != OrderState.Running)
            {
                LiveInfoAvailable = false;
            }
            else if (DisplayStats.DataTime.AddHours(1) < DateTime.Now)
            {
                LiveInfoAvailable = false;
            }
            else
            {
                LiveInfoAvailable = true;
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
            FilteredNotes = null;
            OnPropertyChanged(nameof(FilteredNotes));
            FilteredNotes = Notes
            .Where(n =>
                n.DocumentReference == SelectedOrder.Name &&
                !n.IsDeleted)
            .OrderBy(x => x.DateSent)
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

            OnPropertyChanged(nameof(FilteredOrderItems));
            OnPropertyChanged(nameof(FilteredNotes));
            OnPropertyChanged(nameof(FilteredDrawings));
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
            LMOContructorWindow window = new(null, null)
            {
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();

            if (window.Cancelled)
            {
                return;
            }
            else
            {
                Refresh();
            }
        }

        private static string GetDaySuffix(int day)
        {
            return day switch
            {
                1 or 21 or 31 => "st",
                2 or 22 => "nd",
                3 or 23 => "rd",
                _ => "th",
            };
        }
    }
}