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
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectLighthouse.ViewModel
{
    public class OrderViewModel : BaseViewModel, IDisposable, IRefreshableViewModel
    {
        #region Variables

        public List<LatheManufactureOrder> LatheManufactureOrders { get; set; }
        public List<LatheManufactureOrderItem> LMOItems { get; set; }
        public List<Note> Notes { get; set; }


        private ObservableCollection<LatheManufactureOrder> filteredOrders = new();
        public ObservableCollection<LatheManufactureOrder> FilteredOrders
        {
            get { return filteredOrders; }
            set { filteredOrders = value; }
        }

        private ObservableCollection<LatheManufactureOrderItem> filteredLMOItems = new();
        public ObservableCollection<LatheManufactureOrderItem> FilteredLMOItems
        {
            get { return filteredLMOItems; }
            set { filteredLMOItems = value; }
        }

        private ObservableCollection<Note> filteredNotes = new();
        public ObservableCollection<Note> FilteredNotes
        {
            get { return filteredNotes; }
            set { filteredNotes = value; }
        }
                
        public List<TurnedProduct> Products { get; set; }
        public List<TurnedProduct> SelectedProducts { get; set; }
        public List<ProductGroup> ProductGroups { get; set; }
        public ProductGroup SelectedProductGroup { get; set; }
        public List<MachineStatistics> MachineStatistics { get; set; }
        public List<Lot> Lots { get; set; }

        private LatheManufactureOrder selectedLatheManufactureOrder = new();
        public LatheManufactureOrder SelectedLatheManufactureOrder
        {
            get { return selectedLatheManufactureOrder; }
            set
            {
                selectedLatheManufactureOrder = value;
                LoadOrderCard();
                OnPropertyChanged();
            }
        }

        private MachineStatistics displayStats; // Stats for the machine listed on the order currently selected
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

        private string selectedFilter;

        public string SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                selectedFilter = value;
                FilterOrders(value);
                if (FilteredOrders.Count > 0)
                {
                    SelectedLatheManufactureOrder = FilteredOrders.First();
                    CardVis = Visibility.Visible;
                }
                else
                {
                    CardVis = Visibility.Hidden;
                }
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

        #region Visibility variables

        private Visibility liveInfoVis;

        public Visibility LiveInfoVis
        {
            get { return liveInfoVis; }
            set
            {
                liveInfoVis = value;
                OnPropertyChanged("LiveInfoVis");
            }
        }

        private Visibility cleaningVis;

        public Visibility CleaningVis
        {
            get { return cleaningVis; }
            set
            {
                cleaningVis = value;
                OnPropertyChanged("CleaningVis");
            }
        }


        private Visibility cardVis;

        public Visibility CardVis
        {
            get { return cardVis; }
            set
            {
                cardVis = value;
                OnPropertyChanged("CardVis");
                if (value == Visibility.Visible)
                {
                    NothingVis = Visibility.Hidden;
                    return;
                }
                NothingVis = Visibility.Visible;
            }
        }

        private Visibility nothingVis;

        public Visibility NothingVis
        {
            get { return nothingVis; }
            set
            {
                nothingVis = value;
                OnPropertyChanged("NothingVis");
            }
        }

        private Visibility modifiedVis;

        public Visibility ModifiedVis
        {
            get { return modifiedVis; }
            set
            {
                modifiedVis = value;
                OnPropertyChanged("ModifiedVis");
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

        public ICommand PrintOrderCommand { get; set; }
        public ICommand EditCommand { get; set; }

        //public event EventHandler SelectedLatheManufactureOrderChanged;

        public Brush ToolingIconBrush { get; set; }
        public Brush ProgramIconBrush { get; set; }
        public Brush BarVerifiedIconBrush { get; set; }
        public Brush BarAllocatedIconBrush { get; set; }
        public bool StopRefresh { get; set; } = false;

        public Timer liveTimer;

        #endregion Variables

        public OrderViewModel()
        {

            FilteredOrders.CollectionChanged += Items_CollectionChanged;
            Notes = new();
            LatheManufactureOrders = new();
            MachineStatistics = new();
            LMOItems = new();
            DisplayStats = new();
            //ProductGroups = new();
            //SelectedProductGroup = new();
            //SelectedProducts = new();

            ToolingIconBrush = (Brush)Application.Current.Resources["materialError"];
            ProgramIconBrush = (Brush)Application.Current.Resources["materialError"];
            BarVerifiedIconBrush = (Brush)Application.Current.Resources["materialError"];
            BarAllocatedIconBrush = (Brush)Application.Current.Resources["materialError"];

            PrintOrderCommand = new PrintCommand(this);
            EditCommand = new EditManufactureOrderCommand(this);
            //ProductGroups = DatabaseHelper.Read<ProductGroup>();
            //Products = DatabaseHelper.Read<TurnedProduct>();

            liveTimer = new();

            liveTimer.Elapsed += OnTimedEvent;
            liveTimer.Interval = 60000;
            liveTimer.Enabled = true;
            liveTimer.Start();

            GetLatheManufactureOrders();
            FilterOrders("All Active");
            GetLatheManufactureOrderItems();
            GetLatestStats();
        }

        ~OrderViewModel()
        {
            liveTimer.Stop();
            Debug.WriteLine("Timer Stopped through destructor.");
        }

        #region MachineStats Display

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            GetLatestStats();
        }

        private void GetLatestStats()
        {
            Debug.WriteLine("updating live stats");
            if (App.ActiveViewModel != "Orders")
            {
                Debug.WriteLine("cancelled");
                return;
            }

            MachineStatistics = null;
            MachineStatistics = MachineStatsHelper.GetStats() ?? new();
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>().ToList();
            if (MachineStatistics.Count == 0)
                return;

            string latheName;

            try
            {
                Lathe allocatedLathe = lathes.First(n => n.Id == SelectedLatheManufactureOrder.AllocatedMachine);
                if (allocatedLathe == null)
                {
                    LiveInfoVis = Visibility.Collapsed;
                    return;
                }
                else
                {
                    latheName = allocatedLathe.Id;
                }
            }
            catch
            {
                LiveInfoVis = Visibility.Collapsed;
                return;
            }

            try
            {
                DisplayStats = MachineStatistics.First(n => n.MachineID == latheName);
            }
            catch(Exception ex)
            {
                LiveInfoVis = Visibility.Collapsed;
                return;
            }


            if (DisplayStats == null)
            {
                LiveInfoVis = Visibility.Collapsed;
                return;
            }
            if (DisplayStats.DataTime.AddHours(1) < DateTime.Now)
                LiveInfoVis = Visibility.Collapsed;
        }

        #endregion MachineStats Display

        #region Loading

        private void GetLatheManufactureOrders()
        {
            LatheManufactureOrders.Clear();
            LatheManufactureOrders = DatabaseHelper.Read<LatheManufactureOrder>().ToList();
        }

        private void FilterOrders(string filter)
        {
            FilteredOrders.Clear();
            List<LatheManufactureOrder> tmp = new();
            switch (filter)
            {
                case "All Active":
                    tmp = LatheManufactureOrders.Where(n => n.State < OrderState.Complete || n.ModifiedAt.AddDays(1) > DateTime.Now || !n.IsClosed).ToList();
                    break;

                case "Not Ready":
                    tmp = LatheManufactureOrders.Where(n => n.State == OrderState.Problem).ToList();
                    break;

                case "Ready":
                    tmp = LatheManufactureOrders.Where(n => n.State == OrderState.Ready || n.State == OrderState.Prepared).ToList();
                    break;

                case "Complete":
                    tmp = LatheManufactureOrders.Where(n => n.State > OrderState.Running && n.IsClosed).OrderByDescending(n => n.CreatedAt).ToList();
                    break;

                case "All":
                    tmp = LatheManufactureOrders.OrderByDescending(n => n.CreatedAt).ToList();
                    break;

                case "Search":
                    break;
            }


            for (int i = 0; i < tmp.Count; i++)
            {
                FilteredOrders.Add(tmp[i]);
            }

            OnPropertyChanged(nameof(FilteredOrders));

            if (FilteredOrders.Count > 0)
            {
                SelectedLatheManufactureOrder = FilteredOrders.First();
            }
        }

        private void GetLatheManufactureOrderItems()
        {
            LMOItems.Clear();
            LMOItems = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();

            Notes.Clear();
            Notes = DatabaseHelper.Read<Note>().ToList();

        }

        private void LoadLMOItems()
        {
            if (SelectedLatheManufactureOrder == null)
            {
                return;
            }

            string selectedMO = SelectedLatheManufactureOrder.Name;
            FilteredLMOItems.Clear();

            IEnumerable<LatheManufactureOrderItem> associatedItems = LMOItems.Where(i => i.AssignedMO == selectedMO);

            FilteredLMOItems = new(associatedItems.OrderByDescending(n => n.RequiredQuantity).ThenBy(n => n.ProductName));

            List<Note> tmpNotes = new();

            if (App.CurrentUser.UserRole == "admin")
            {
                tmpNotes = Notes.Where(n => n.DocumentReference == selectedMO).OrderBy(x => x.DateSent).ToList();
            }
            else
            {
                tmpNotes = Notes.Where(n => n.DocumentReference == selectedMO && !n.IsDeleted).OrderBy(x => x.DateSent).ToList();
            }

            string name = "";
            DateTime lastTimeStamp = DateTime.MinValue;

            for (int i = 0; i < tmpNotes.Count; i++)
            {
                tmpNotes[i].ShowEdit = false;
                tmpNotes[i].ShowHeader = tmpNotes[i].SentBy != name
                    || DateTime.Parse(tmpNotes[i].DateSent) > lastTimeStamp.AddHours(6);
                if (i < tmpNotes.Count - 1)
                {
                    tmpNotes[i].ShowSpacerUnder = DateTime.Parse(tmpNotes[i + 1].DateSent) > DateTime.Parse(tmpNotes[i].DateSent).AddHours(6);
                }
                lastTimeStamp = DateTime.Parse(tmpNotes[i].DateSent);
                name = tmpNotes[i].SentBy;
            }

            FilteredNotes.Clear();
            for (int i = 0; i < tmpNotes.Count; i++)
            {
                FilteredNotes.Add(tmpNotes[i]);
            }

            OnPropertyChanged("FilteredLMOItems");
            OnPropertyChanged("FilteredNotes");
        }

        #endregion Loading

        public void Search()
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilterOrders(SelectedFilter);
                return;
            }

            List<LatheManufactureOrder> Results = new();
            List<string> FoundOrders = new();

            // Add quick jump buttons

            foreach (LatheManufactureOrder order in LatheManufactureOrders)
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
            foreach (LatheManufactureOrderItem item in LMOItems)
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

            Results.AddRange(LatheManufactureOrders.Where(x => FoundOrdersByItem.Contains(x.Name)));


            for (int i = 0; i < Results.Count; i++)
            {
                FilteredOrders.Add(Results[i]);
            }
            //FilteredOrders = Results;
            FilterOrders("Search");
        }

        private void LoadOrderCard()
        {
            if (SelectedLatheManufactureOrder == null)
            {
                CardVis = Visibility.Hidden;
                return;
            }
            else
            {
                CardVis = Visibility.Visible;
            }
            ProgramIconBrush = SelectedLatheManufactureOrder.HasProgram
                ? (Brush)Application.Current.Resources["materialPrimaryGreen"]
                : (Brush)Application.Current.Resources["materialError"];
            OnPropertyChanged("ProgramIconBrush");

            ToolingIconBrush = SelectedLatheManufactureOrder.IsReady
                ? (Brush)Application.Current.Resources["materialPrimaryGreen"]
                : (Brush)Application.Current.Resources["materialError"];
            OnPropertyChanged("ToolingIconBrush");

            BarVerifiedIconBrush = SelectedLatheManufactureOrder.BarIsVerified
                ? (Brush)Application.Current.Resources["materialPrimaryGreen"]
                : (Brush)Application.Current.Resources["materialError"];
            OnPropertyChanged("BarVerifiedIconBrush");

            BarAllocatedIconBrush = SelectedLatheManufactureOrder.BarIsAllocated
                ? (Brush)Application.Current.Resources["materialPrimaryGreen"]
                : (Brush)Application.Current.Resources["materialError"];
            OnPropertyChanged("BarAllocatedIconBrush");

            LoadLMOItems();

            ModifiedVis = string.IsNullOrEmpty(SelectedLatheManufactureOrder.ModifiedBy)
                ? Visibility.Collapsed
                : Visibility.Visible;
            LiveInfoVis = SelectedLatheManufactureOrder.State == OrderState.Running
                && MachineStatistics.Count != 0
                && !string.IsNullOrEmpty(SelectedLatheManufactureOrder.AllocatedMachine)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            ArchiveVis = SelectedLatheManufactureOrder.IsClosed
                ? Visibility.Visible
                : Visibility.Collapsed;
            CleaningVis = SelectedLatheManufactureOrder.ItemNeedsCleaning
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (LiveInfoVis == Visibility.Visible)
                GetLatestStats();

            Lots = DatabaseHelper.Read<Lot>().ToList();

            RunInfoText = !string.IsNullOrEmpty(SelectedLatheManufactureOrder.AllocatedMachine)
                ? $"Assigned to {selectedLatheManufactureOrder.AllocatedMachine}, starting {SelectedLatheManufactureOrder.StartDate:dddd, MMMM d}{GetDaySuffix(SelectedLatheManufactureOrder.StartDate.Day)}"
                : "Not scheduled";

            //LoadProductInfoCard();
        }

        //private void LoadProductInfoCard()
        //{
        //    SelectedProducts = new();
        //    foreach (LatheManufactureOrderItem item in FilteredLMOItems)
        //    {
        //        List<TurnedProduct> tmp = Products.Where(n => n.ProductName == item.ProductName).ToList();
        //        if (tmp.Count > 0)
        //        {
        //            SelectedProducts.Add(tmp.First());
        //        }
        //    }

        //    if (SelectedProducts.Count != 0)
        //    {
        //        TurnedProduct tmp = SelectedProducts.First();
        //        List<ProductGroup> matches = ProductGroups.Where(x => x.GroupID == tmp.ProductName[..5] && x.MaterialCode == tmp.Material).ToList();
        //        SelectedProductGroup = matches.Count != 0 ? matches.First() : new();
        //    }

        //    OnPropertyChanged("SelectedProductGroup");
        //    OnPropertyChanged("SelectedProduct");
        //}

        public void PrintSelectedOrder()
        {
            ReportPdf reportService = new();
            OrderPrintoutData reportData = new()
            {
                Order = SelectedLatheManufactureOrder,
                Items = FilteredLMOItems.ToArray(),
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
            EditLMOWindow editWindow = new(
                o:SelectedLatheManufactureOrder, 
                i:FilteredLMOItems.ToList(), 
                l:Lots.Where(n => n.Order == SelectedLatheManufactureOrder.Name).ToList(), 
                n:FilteredNotes.ToList()
                );

            editWindow.Owner = Application.Current.MainWindow;
            _ = editWindow.ShowDialog();

            if (editWindow.SaveExit)
            {
                GetLatheManufactureOrders();
                FilterOrders(SelectedFilter); // update list on screen

                SelectedLatheManufactureOrder = LatheManufactureOrders.Where(o => o.Name == editWindow.order.Name).FirstOrDefault();
            }

            editWindow = null;

            GetLatheManufactureOrderItems();
            LoadLMOItems();
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

        public void Dispose()
        {
            Products = null;
            MachineStatistics = null;

            liveTimer.Stop();
            liveTimer.Dispose();

            Debug.WriteLine($"Timer Disposed.");
            Debug.WriteLine("Disposing");
        }

        public void Refresh()
        {
            //GetLatheManufactureOrders();
            //FilterOrders("All Active");
            //GetLatheManufactureOrderItems();
            
            //Random RNG = new();

            //FilteredOrders?.Clear();
            //List<LatheManufactureOrder> tmp = LatheManufactureOrders.OrderBy(x => RNG.Next()).Take(5).ToList();

            //FilteredOrders = new ObservableCollection<LatheManufactureOrder>(tmp);
            //foreach (LatheManufactureOrder item in tmp)
            //{
            //    FilteredOrders.Add(item);
            //}

            OnPropertyChanged("FilteredOrders");
        }

        

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("FilteredOrders");
        }
    }
}