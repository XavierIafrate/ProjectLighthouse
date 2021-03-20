using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectLighthouse.ViewModel
{
    public class OrderViewModel : BaseViewModel
    {
        public ObservableCollection<LatheManufactureOrder> LatheManufactureOrders { get; set; }
        public ObservableCollection<LatheManufactureOrder> FilteredOrders { get; set; }
        public ObservableCollection<LatheManufactureOrderItem> LMOItems { get; set; }
        public ObservableCollection<LatheManufactureOrderItem> FilteredLMOItems { get; set; }

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
                if(selectedLatheManufactureOrder == null)
                {
                    CardVis = Visibility.Hidden;
                    return;
                }
                else
                {
                    CardVis = Visibility.Visible;
                }

                if (String.IsNullOrEmpty(selectedLatheManufactureOrder.ModifiedBy))
                {
                    ModifiedVis = Visibility.Collapsed;
                }
                else
                {
                    ModifiedVis = Visibility.Visible;
                }

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

        private Visibility cardVis;
        public Visibility CardVis
        {
            get { return cardVis; }
            set 
            { 
                cardVis = value;
                OnPropertyChanged("CardVis");
                if(value == Visibility.Visible)
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

        public event EventHandler SelectedLatheManufactureOrderChanged;

        public OrderViewModel()
        {
            LatheManufactureOrders = new ObservableCollection<LatheManufactureOrder>();
            FilteredOrders = new ObservableCollection<LatheManufactureOrder>();


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
            foreach(var item in LMOItems)
            {
                if(item.AssignedMO == selectedMO)
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