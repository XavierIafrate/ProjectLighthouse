using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
{
    public class OrderViewModel : BaseViewModel
    {
        #region Variables
        public List<LatheManufactureOrder> LatheManufactureOrders { get; set; }
        public List<LatheManufactureOrder> FilteredOrders { get; set; }
        public List<LatheManufactureOrderItem> LMOItems { get; set; }
        public List<LatheManufactureOrderItem> FilteredLMOItems { get; set; }
        public List<MachineStatistics> machineStatistics { get; set; }
        public List<Lot> Lots { get; set; }

        private LatheManufactureOrder selectedLatheManufactureOrder;
        public LatheManufactureOrder SelectedLatheManufactureOrder
        {
            get { return selectedLatheManufactureOrder; }
            set
            {
                selectedLatheManufactureOrder = value;

                if (value == null)
                {
                    CardVis = Visibility.Hidden;
                    return;
                }
                else
                {
                    CardVis = Visibility.Visible;
                }

                LoadLMOItems();
                ModifiedVis = string.IsNullOrEmpty(value.ModifiedBy) ? Visibility.Collapsed : Visibility.Visible;
                LiveInfoVis = value.Status == "Running" && machineStatistics.Count != 0 ? Visibility.Visible : Visibility.Collapsed;

                if (LiveInfoVis == Visibility.Visible)
                    GetLatestStats();

                Lots = DatabaseHelper.Read<Lot>().ToList();

                RunInfoText = !string.IsNullOrEmpty(value.AllocatedMachine) ?
                    string.Format("Assigned to {0}, starting {1:dddd, MMMM d}{2}",
                    selectedLatheManufactureOrder.AllocatedMachine,
                    selectedLatheManufactureOrder.StartDate,
                    GetDaySuffix(value.StartDate.Day)) :
                    RunInfoText = "Not scheduled";

                OnPropertyChanged("RunInfoText");
                OnPropertyChanged("SelectedLatheManufactureOrder");
            }
        }

        private MachineStatistics displayStats; // Stats for the machine listed on the order currently selected
        public MachineStatistics DisplayStats
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
            LatheManufactureOrders = new List<LatheManufactureOrder>();
            FilteredOrders = new List<LatheManufactureOrder>();
            machineStatistics = new List<MachineStatistics>();
            LMOItems = new List<LatheManufactureOrderItem>();
            FilteredLMOItems = new List<LatheManufactureOrderItem>();
            SelectedLatheManufactureOrder = new LatheManufactureOrder();
            DisplayStats = new MachineStatistics();

            PrintOrderCommand = new PrintCommand(this);
            EditCommand = new EditManufactureOrderCommand(this);

            GetLatheManufactureOrders();
            FilterOrders("All Active");
            GetLatheManufactureOrderItems();
            GetLatestStats();
        }

        #region MachineStats Display

        private async void GetLatestStats()
        {
            machineStatistics = null;
            machineStatistics = await MachineStatsHelper.GetStats();
            machineStatistics ??= new List<MachineStatistics>();
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>().ToList();
            if (machineStatistics.Count == 0)
                return;

            string latheName = lathes.Where(n => n.Id == SelectedLatheManufactureOrder.AllocatedMachine).FirstOrDefault().FullName;

            DisplayStats = machineStatistics.Where(n => n.MachineID == latheName).FirstOrDefault();
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
            EditLMOWindow editWindow = new EditLMOWindow((LatheManufactureOrder)SelectedLatheManufactureOrder.Clone(), FilteredLMOItems, Lots.Where(n => n.Order == SelectedLatheManufactureOrder.Name).ToList());
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
