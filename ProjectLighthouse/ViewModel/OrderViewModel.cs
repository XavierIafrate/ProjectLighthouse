using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProjectLighthouse.ViewModel
{
    public class OrderViewModel : BaseViewModel
    {
        public ObservableCollection<LatheManufactureOrder> LatheManufactureOrders { get; set; }
        public ObservableCollection<LatheManufactureOrder> FilteredOrders { get; set; }
        public ObservableCollection<LatheManufactureOrderItem> LMOItems { get; set; }
        public ObservableCollection<LatheManufactureOrderItem> FilteredLMOItems { get; set; }
        public List<MachineStatistics> machineStatistics { get; set; }

        private LinearGradientBrush statusBackground;
        public LinearGradientBrush StatusBackground
        {
            get { return statusBackground; }
            set { statusBackground = value; }
        }

        private LatheManufactureOrder selectedLatheManufactureOrder;
        public LatheManufactureOrder SelectedLatheManufactureOrder
        {
            get { return selectedLatheManufactureOrder; }
            set
            {
                selectedLatheManufactureOrder = value;
                OnPropertyChanged("SelectedLatheManufactureOrder");
                SelectedLatheManufactureOrderChanged?.Invoke(this, new EventArgs());
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

                LiveInfoVis = selectedLatheManufactureOrder.Status == "Running" ? Visibility.Visible : Visibility.Collapsed;

                switch (selectedLatheManufactureOrder.Status)
                {
                    case "Ready":
                        StatusBackground = (LinearGradientBrush)Application.Current.Resources["gradGood"];
                        OnPropertyChanged("StatusBackground");
                        break;
                    case "Problem":
                        StatusBackground = (LinearGradientBrush)Application.Current.Resources["gradBad"];
                        OnPropertyChanged("StatusBackground");
                        break;
                    case "Complete":
                        StatusBackground = (LinearGradientBrush)Application.Current.Resources["gradGood"];
                        OnPropertyChanged("StatusBackground");
                        break;
                    case "Running":
                        StatusBackground = (LinearGradientBrush)Application.Current.Resources["gradBlue"];
                        OnPropertyChanged("StatusBackground");
                        RefreshLiveInfoText();
                        break;
                    default:
                        StatusBackground = (LinearGradientBrush)Application.Current.Resources["gradGood"];
                        OnPropertyChanged("StatusBackground");
                        break;
                }

                if (!String.IsNullOrEmpty(selectedLatheManufactureOrder.AllocatedMachine))
                {
                    RunInfoText = String.Format("Assigned to {0}, starting {1:MMMM d}{2}", selectedLatheManufactureOrder.AllocatedMachine, selectedLatheManufactureOrder.StartDate, GetDaySuffix(selectedLatheManufactureOrder.StartDate.Day));
                }
                else
                {
                    RunInfoText = "Not scheduled";
                }
                OnPropertyChanged("RunInfoText");
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

        public ICommand PrintOrderCommand { get; set; }
        public ICommand EditCommand { get; set; }
        #endregion

        public event EventHandler SelectedLatheManufactureOrderChanged;

        public OrderViewModel()
        {
            // refresh live stats
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(60);
            dispatcherTimer.Start();

            LatheManufactureOrders = new ObservableCollection<LatheManufactureOrder>();
            FilteredOrders = new ObservableCollection<LatheManufactureOrder>();
            machineStatistics = new List<MachineStatistics>();

            LMOItems = new ObservableCollection<LatheManufactureOrderItem>();
            FilteredLMOItems = new ObservableCollection<LatheManufactureOrderItem>();
            SelectedLatheManufactureOrder = new LatheManufactureOrder();

            PrintOrderCommand = new PrintCommand(this);
            EditCommand = new EditManufactureOrderCommand(this);

            GetLatheManufactureOrders();
            FilterOrders("All Active");
            if (FilteredOrders.Count > 0)
            {
                SelectedLatheManufactureOrder = FilteredOrders.First();
            }
            
            GetLatheManufactureOrderItems();
            GetLiveStats();
            RefreshLiveInfoText();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            GetLiveStats();
            RefreshLiveInfoText();
        }

        private void GetLiveStats()
        {
            machineStatistics.Clear();
            machineStatistics = MachineStatsHelper.GetStats();
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

        private void GetLatheManufactureOrders()
        {
            var orders = DatabaseHelper.Read<LatheManufactureOrder>().ToList();
            LatheManufactureOrders.Clear();
            foreach (var order in orders)
            {
                LatheManufactureOrders.Add(order);
            }
        }

        private void FilterOrders(string filter)
        {
            switch (filter)
            {
                case "All Active":
                    FilteredOrders = new ObservableCollection<LatheManufactureOrder>(LatheManufactureOrders.Where(n => !n.IsComplete));
                    break;
                case "Not Ready":
                    FilteredOrders = new ObservableCollection<LatheManufactureOrder>(LatheManufactureOrders.Where(n => !n.IsComplete && !n.IsReady));
                    break;
                case "Ready":
                    FilteredOrders = new ObservableCollection<LatheManufactureOrder>(LatheManufactureOrders.Where(n => !n.IsComplete && n.IsReady));
                    break;
                case "Complete":
                    FilteredOrders = new ObservableCollection<LatheManufactureOrder>(LatheManufactureOrders.Where(n => n.IsComplete));
                    break;
            }
            if (FilteredOrders.Count > 0)
            {
                SelectedLatheManufactureOrder = FilteredOrders.First();
            }
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
            {
                return;
            }
            string selectedMO = SelectedLatheManufactureOrder.Name;
            FilteredLMOItems.Clear();
            foreach (var item in LMOItems)
            {
                if (item.AssignedMO == selectedMO)
                {
                    FilteredLMOItems.Add(item);
                }
            }
        }

        public void PrintSelectedOrder()
        {
            PDFHelper.PrintOrder(SelectedLatheManufactureOrder, FilteredLMOItems);
        }

        public void EditLMO()
        {
            EditLMOWindow editWindow = new EditLMOWindow(SelectedLatheManufactureOrder);
            editWindow.Owner = Application.Current.MainWindow;
            editWindow.ShowDialog();

            GetLatheManufactureOrders();
            FilterOrders(SelectedFilter);
            if (FilteredOrders.Count > 0)
            {
                SelectedLatheManufactureOrder = FilteredOrders.First();
            }
            GetLatheManufactureOrderItems();

        }

    }
}