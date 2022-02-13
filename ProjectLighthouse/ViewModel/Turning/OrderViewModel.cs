using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Reporting;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectLighthouse.ViewModel
{
    public class OrderViewModel : BaseViewModel, IRefreshableViewModel
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
                if (value == null)
                {
                    return;
                }
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
                FilterOrders(value);
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

        private Visibility liveInfoVis;
        public Visibility LiveInfoVis
        {
            get { return liveInfoVis; }
            set
            {
                liveInfoVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility cleaningVis;
        public Visibility CleaningVis
        {
            get { return cleaningVis; }
            set
            {
                cleaningVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility cardVis;
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
        public ICommand PrintOrderCommand { get; set; }
        public ICommand EditCommand { get; set; }
        #endregion

        #region Icon Brushes
        public Brush ToolingIconBrush { get; set; }
        public Brush ProgramIconBrush { get; set; }
        public Brush BarVerifiedIconBrush { get; set; }
        public Brush BarAllocatedIconBrush { get; set; }
        #endregion

        public Timer DataRefreshTimer;

        #endregion Variables

        public OrderViewModel()
        {
            InitialiseVariables();
            CreateTimer();
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

            FilteredDrawings = new();
            FilteredNotes = new();
            FilteredOrderItems = new();
            FilteredOrders = new();

            ToolingIconBrush = (Brush)Application.Current.Resources["materialError"];
            ProgramIconBrush = (Brush)Application.Current.Resources["materialError"];
            BarVerifiedIconBrush = (Brush)Application.Current.Resources["materialError"];
            BarAllocatedIconBrush = (Brush)Application.Current.Resources["materialError"];

            PrintOrderCommand = new PrintCommand(this);
            EditCommand = new EditManufactureOrderCommand(this);
        }

        private void CreateTimer()
        {
            DataRefreshTimer = new();

            DataRefreshTimer.Elapsed += OnTimedEvent;
            DataRefreshTimer.Interval = 5 * 60 * 1000; // 5 minutes
            DataRefreshTimer.Enabled = true;
            if (App.CurrentUser.EnableDataSync)
            {
                DataRefreshTimer.Start();
            }
        }

        #region Data Refreshing

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Refresh(silent: true);
        }

        public void Refresh(bool silent = false) // Check behaviour
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                LoadData();
                if (silent)
                {
                    UpdateOrders();
                    LoadOrderCard();
                    OnPropertyChanged(nameof(SelectedOrder));
                }
                else
                {
                    FilterOrders(SelectedFilter);
                }
                OnPropertyChanged(nameof(FilteredOrders));
            }

            App.MainViewModel.LastDataRefresh = DateTime.Now;

            if (!silent)
            {
                if (FilteredOrders.Count > 0)
                {
                    SelectedOrder = FilteredOrders[0];
                }
                else
                {
                    SelectedOrder = null;
                }
            }

            OnPropertyChanged(nameof(SelectedOrder));
        }

        #endregion

        #region Loading
        private void LoadData()
        {
            Orders.Clear();
            Orders = DatabaseHelper.Read<LatheManufactureOrder>().ToList();

            OrderItems.Clear();
            OrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();

            Notes.Clear();
            Notes = DatabaseHelper.Read<Note>().ToList();

            Drawings.Clear();
            Drawings = DatabaseHelper.Read<TechnicalDrawing>().ToList();

            OrderDrawings.Clear();
            OrderDrawings = DatabaseHelper.Read<OrderDrawing>().ToList();

            Lathes.Clear();
            Lathes = DatabaseHelper.Read<Lathe>().ToList();

            Lots.Clear();
            Lots = DatabaseHelper.Read<Lot>().ToList();

            MachineStatistics = MachineStatsHelper.GetStats();
        }

        private void UpdateOrders()
        {
            for (int i = 0; i < FilteredOrders.Count; i++)
            {
                LatheManufactureOrder masterOrder = Orders.Find(x => x.Name == FilteredOrders[i].Name);
                if (FilteredOrders[i].IsUpdated(masterOrder))
                {
                    FilteredOrders[i].Update(masterOrder);
                }
            }
        }

        private void FilterOrders(string filter)
        {
            FilteredOrders.Clear();

            List<LatheManufactureOrder> orders = new();
            switch (filter)
            {
                case "All Active":
                    orders = Orders.Where(n => n.State < OrderState.Complete || n.ModifiedAt.AddDays(1) > DateTime.Now || !n.IsClosed).ToList();
                    break;

                case "Not Ready":
                    orders = Orders.Where(n => n.State == OrderState.Problem).ToList();
                    break;

                case "Ready":
                    orders = Orders.Where(n => n.State == OrderState.Ready || n.State == OrderState.Prepared).ToList();
                    break;

                case "Complete":
                    orders = Orders.Where(n => n.State > OrderState.Running && n.IsClosed).OrderByDescending(n => n.CreatedAt).ToList();
                    break;

                case "All":
                    orders = Orders.OrderByDescending(n => n.CreatedAt).ToList();
                    break;
            }

            for (int i = 0; i < orders.Count; i++)
            {
                FilteredOrders.Add(orders[i]);
            }
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

            FilteredOrders.Clear();
            for (int i = 0; i < Results.Count; i++)
            {
                FilteredOrders.Add(Results[i]);
            }
            OnPropertyChanged(nameof(FilteredOrders));
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

            SetUiElements();
            LoadOrderObjects();
        }

        private void SetUiElements()
        {
            if (SelectedOrder == null)
            {
                return;
            }

            // Quick indicator icons
            ProgramIconBrush = (Brush)Application.Current.Resources[SelectedOrder.HasProgram ? "materialPrimaryGreen" : "materialError"];
            OnPropertyChanged(nameof(ProgramIconBrush));

            ToolingIconBrush = (Brush)Application.Current.Resources[SelectedOrder.IsReady ? "materialPrimaryGreen" : "materialError"];
            OnPropertyChanged(nameof(ToolingIconBrush));

            BarVerifiedIconBrush = (Brush)Application.Current.Resources[SelectedOrder.BarIsVerified ? "materialPrimaryGreen" : "materialError"];
            OnPropertyChanged(nameof(BarVerifiedIconBrush));

            BarAllocatedIconBrush = (Brush)Application.Current.Resources[SelectedOrder.BarIsAllocated ? "materialPrimaryGreen" : "materialError"];
            OnPropertyChanged(nameof(BarAllocatedIconBrush));


            ModifiedVis = string.IsNullOrEmpty(SelectedOrder.ModifiedBy)
               ? Visibility.Collapsed
               : Visibility.Visible;

            SetLiveMachineInfo();

            ArchiveVis = SelectedOrder.IsClosed
                ? Visibility.Visible
                : Visibility.Collapsed;

            CleaningVis = SelectedOrder.ItemNeedsCleaning
                ? Visibility.Visible
                : Visibility.Collapsed;

            NoDrawingsFoundVis = FilteredDrawings.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            DrawingsFoundVis = FilteredDrawings.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            RunInfoText = !string.IsNullOrEmpty(SelectedOrder.AllocatedMachine)
                ? $"Assigned to {SelectedOrder.AllocatedMachine}, starting {SelectedOrder.StartDate:dddd, MMMM d}{GetDaySuffix(SelectedOrder.StartDate.Day)}"
                : "Not scheduled";
        }

        private void SetLiveMachineInfo()
        {
            DisplayStats = MachineStatistics.Find(x => x.MachineID == SelectedOrder.AllocatedMachine);
            if (DisplayStats == null || SelectedOrder.State != OrderState.Running)
            {
                LiveInfoVis = Visibility.Collapsed;
            }
            else if(DisplayStats.DataTime.AddHours(1) < DateTime.Now)
            {
                LiveInfoVis = Visibility.Collapsed;
            }
            else
            {
                LiveInfoVis = Visibility.Visible;
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
            FilteredNotes.Clear();
            if (App.CurrentUser.Role == UserRole.Administrator)
            {
                FilteredNotes = Notes
                   .Where(n => n.DocumentReference == SelectedOrder.Name)
                   .OrderBy(x => x.DateSent)
                   .ToList();
            }
            else
            {
                FilteredNotes = Notes
                .Where(n =>
                    n.DocumentReference == SelectedOrder.Name &&
                    !n.IsDeleted)
                .OrderBy(x => x.DateSent)
                .ToList();
            }
            

            FormatNotes(FilteredNotes);


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

        private static List<Note> FormatNotes(List<Note> notes)
        {
            string name = "";
            DateTime lastTimeStamp = DateTime.MinValue;

            for (int i = 0; i < notes.Count; i++)
            {
                notes[i].ShowEdit = false;
                notes[i].ShowHeader = notes[i].SentBy != name
                    || DateTime.Parse(notes[i].DateSent) > lastTimeStamp.AddHours(6);
                if (i < notes.Count - 1)
                {
                    notes[i].ShowSpacerUnder = DateTime.Parse(notes[i + 1].DateSent) > DateTime.Parse(notes[i].DateSent).AddHours(6);
                }
                lastTimeStamp = DateTime.Parse(notes[i].DateSent);
                name = notes[i].SentBy;
            }

            return notes;
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
            DataRefreshTimer.Enabled = false;
            string order = SelectedOrder.Name;
            EditLMOWindow editWindow = new(
                o: SelectedOrder,
                i: FilteredOrderItems.ToList(),
                l: Lots.Where(n => n.Order == SelectedOrder.Name).ToList(),
                n: FilteredNotes.ToList()
                );

            editWindow.Owner = Application.Current.MainWindow;
            _ = editWindow.ShowDialog();

            if (editWindow.SaveExit)
            {
                Refresh(silent: false);
                for (int i = 0; i < FilteredOrders.Count; i++)
                {
                    if (FilteredOrders[i].Name == order)
                    {
                        SelectedOrder = FilteredOrders[i];
                    }
                }   
            }

            

            editWindow = null;

            DataRefreshTimer.Enabled = true;
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