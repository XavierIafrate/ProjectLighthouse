using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProjectLighthouse.ViewModel
{
    public class OrderViewModel : BaseViewModel
    {
        #region Variables
        public List<LatheManufactureOrder> LatheManufactureOrders { get; set; }
        public List<LatheManufactureOrder> FilteredOrders { get; set; }
        public List<LatheManufactureOrderItem> LMOItems { get; set; }
        public List<LatheManufactureOrderItem> FilteredLMOItems { get; set; }
        private List<MachineStatistics> _machineStatistics;

        public List<MachineStatistics> machineStatistics
        {
            get { return _machineStatistics; }
            set { _machineStatistics = value; }
        }

        public List<Lot> Lots { get; set; }

        DispatcherTimer dispatcherTimer { get; set; }

        private LatheManufactureOrder selectedLatheManufactureOrder;
        public LatheManufactureOrder SelectedLatheManufactureOrder
        {
            get { return selectedLatheManufactureOrder; }
            set
            {
                selectedLatheManufactureOrder = value;
                
                LoadLMOItems();
                if (selectedLatheManufactureOrder == null)
                {
                    CardVis = Visibility.Hidden;
                    return;
                }
                else
                {
                    CardVis = Visibility.Visible;
                }

                ModifiedVis = String.IsNullOrEmpty(selectedLatheManufactureOrder.ModifiedBy) ? Visibility.Collapsed : Visibility.Visible;
                machineStatistics = (machineStatistics == null) ? new List<MachineStatistics>() : machineStatistics;
                LiveInfoVis = selectedLatheManufactureOrder.Status == "Running" && machineStatistics.Count != 0 ? Visibility.Visible : Visibility.Collapsed;

                Debug.WriteLine(string.Format("# stats: {0}", machineStatistics.Count));
                Debug.WriteLine(string.Format("live vis: {0}", LiveInfoVis));

                Lots = DatabaseHelper.Read<Lot>().ToList();
                RunInfoText = !String.IsNullOrEmpty(selectedLatheManufactureOrder.AllocatedMachine) ? 
                    String.Format("Assigned to {0}, starting {1:dddd, MMMM d}{2}", 
                    selectedLatheManufactureOrder.AllocatedMachine, 
                    selectedLatheManufactureOrder.StartDate, 
                    GetDaySuffix(selectedLatheManufactureOrder.StartDate.Day)) :
                    RunInfoText = "Not scheduled";

                OnPropertyChanged("RunInfoText");
                OnPropertyChanged("SelectedLatheManufactureOrder");
            }
        }

        private string displayStats;
        public string DisplayStats
        {
            get { return displayStats; }
            set 
            { 
                displayStats = value;
                OnPropertyChanged("DisplayStats");
            }
        }

        private string runInfoText;
        public string RunInfoText
        {
            get { return runInfoText; }
            set { runInfoText = value; }
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

        #endregion

        public ICommand PrintOrderCommand { get; set; }
        public ICommand EditCommand { get; set; }

        public event EventHandler SelectedLatheManufactureOrderChanged;
        #endregion

        public OrderViewModel()
        {
            // refresh live stats
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(60);
            dispatcherTimer.Start();

            LatheManufactureOrders = new List<LatheManufactureOrder>();
            FilteredOrders = new List<LatheManufactureOrder>();
            machineStatistics = new List<MachineStatistics>();

            LMOItems = new List<LatheManufactureOrderItem>();
            FilteredLMOItems = new List<LatheManufactureOrderItem>();
            SelectedLatheManufactureOrder = new LatheManufactureOrder();

            PrintOrderCommand = new PrintCommand(this);
            EditCommand = new EditManufactureOrderCommand(this);

            GetLatheManufactureOrders();
            FilterOrders("All Active");
            if (FilteredOrders.Count > 0)
                SelectedLatheManufactureOrder = FilteredOrders.First();

            GetLatheManufactureOrderItems();
            GetLiveStats();
            RefreshLiveInfoText();
        }

        #region MachineStats Display
        private void dispatcherTimer_Tick(object sender, EventArgs e)
       {
            if (machineStatistics == null) // stop the last tick
                return;

            GetLiveStats();
            if (machineStatistics == null)
            {
                dispatcherTimer.Stop();
                return;
            }
                
            RefreshLiveInfoText();
        }

        private async void GetLiveStats()
        {
            machineStatistics = new List<MachineStatistics>();
            machineStatistics = await MachineStatsHelper.GetStats();
            Debug.WriteLine(string.Format("# stats: {0}", machineStatistics.Count));
        }

        private void RefreshLiveInfoText()
        {
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>().ToList();
            string fullname = String.Empty;
            MachineStatistics target_stats = new MachineStatistics();
            foreach(var lathe in lathes)
            {
                if(lathe.Id == SelectedLatheManufactureOrder.AllocatedMachine)
                {
                    fullname = lathe.FullName;
                    break;
                }
            }

            if (!String.IsNullOrWhiteSpace(fullname))
            {
                foreach (var stat in machineStatistics)
                {
                    if(stat.MachineID == fullname)
                    {
                        target_stats = stat;
                    }
                }

                if(target_stats != null)
                {
                    DisplayStats = String.Format("Part Counter: {0:#,##0} of {1:#,##0}, complete {2}", target_stats.PartCountAll, target_stats.PartCountTarget, target_stats.EstimateCompletionDate());
                    if (target_stats.PartCountAll == 0 && target_stats.PartCountTarget == 0 && dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    return;
                }
                else
                {
                    DisplayStats = String.Empty;
                    return;
                }
            }
            else
            {
                DisplayStats = String.Empty;
                return;
            }
        }
        #endregion

        #region Loading
        private void GetLatheManufactureOrders()
        {
            var orders = DatabaseHelper.Read<LatheManufactureOrder>().ToList();
            LatheManufactureOrders.Clear();
            foreach (var order in orders)
                LatheManufactureOrders.Add(order);
        }

        private void FilterOrders(string filter)
        {
            switch (filter)
            {
                case "All Active":
                    FilteredOrders = new List<LatheManufactureOrder>(LatheManufactureOrders.Where(n => !n.IsComplete || n.ModifiedAt.AddDays(1) > DateTime.Now));
                    break;
                case "Not Ready":
                    FilteredOrders = new List<LatheManufactureOrder>(LatheManufactureOrders.Where(n => !n.IsComplete && !n.IsReady));
                    break;
                case "Ready":
                    FilteredOrders = new List<LatheManufactureOrder>(LatheManufactureOrders.Where(n => !n.IsComplete && n.IsReady && n.Status != "Running"));
                    break;
                case "Complete":
                    FilteredOrders = new List<LatheManufactureOrder>(LatheManufactureOrders.Where(n => n.IsComplete).OrderByDescending(n => n.CreatedAt));
                    break;
            }

            if (FilteredOrders.Count > 0)
                SelectedLatheManufactureOrder = FilteredOrders.First();

            OnPropertyChanged("FilteredOrders");
        }

        private void GetLatheManufactureOrderItems()
        {
            var items = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();
            LMOItems.Clear();
            foreach (var item in items)
            {
                LMOItems.Add(item);
            }
        }

        private void LoadLMOItems()
        {
            if (SelectedLatheManufactureOrder == null)
                return;

            string selectedMO = SelectedLatheManufactureOrder.Name;
            FilteredLMOItems.Clear();

            foreach (var item in LMOItems)
                if (item.AssignedMO == selectedMO)
                    FilteredLMOItems.Add(item);

            FilteredLMOItems = new List<LatheManufactureOrderItem>
                (FilteredLMOItems.OrderByDescending(n => n.RequiredQuantity).ThenBy(n => n.ProductName));
            OnPropertyChanged("FilteredLMOItems");
        }

        #endregion

        public void PrintSelectedOrder()
        {
            PDFHelper.PrintOrder(SelectedLatheManufactureOrder, FilteredLMOItems);
        }

        public void EditLMO()
        {
            EditLMOWindow editWindow = new EditLMOWindow((LatheManufactureOrder)SelectedLatheManufactureOrder.Clone(), FilteredLMOItems, Lots.Where(n=>n.Order == SelectedLatheManufactureOrder.Name).ToList());
            editWindow.Owner = Application.Current.MainWindow;
            editWindow.ShowDialog();

            if (editWindow.SaveExit)
            {
                GetLatheManufactureOrders();
                FilterOrders(SelectedFilter); // update list on screen
                foreach (LatheManufactureOrder order in LatheManufactureOrders) // re-select order
                {
                    if (order.Name == editWindow.order.Name)
                    {
                        SelectedLatheManufactureOrder = order;
                        break;
                    }
                }
            }

            editWindow = null;

            GetLatheManufactureOrderItems();
            LoadLMOItems();
        }

        private string GetDaySuffix(int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }
    }
}
