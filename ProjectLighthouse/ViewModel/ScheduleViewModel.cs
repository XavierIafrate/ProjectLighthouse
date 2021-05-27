using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Commands.Printing;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
{
    public class ScheduleViewModel : BaseViewModel
    {
        #region Variables
        public ObservableCollection<LatheManufactureOrder> Orders { get; set; }
        public ObservableCollection<LatheManufactureOrderItem> OrderItems { get; set; }
        public List<CompleteOrder> CompleteOrders { get; set; }
        public List<TabInfo> WindowTabs { get; set; }

        private Visibility autoScheduleVis;
        public Visibility AutoScheduleVis
        {
            get { return autoScheduleVis; }
            set
            {
                autoScheduleVis = value;
                OnPropertyChanged("AutoScheduleVis");
            }
        }

        private Visibility printButtonVis;
        public Visibility PrintButtonVis
        {
            get { return printButtonVis; }
            set
            {
                printButtonVis = value;
                OnPropertyChanged("PrintButtonVis");
            }
        }

        public PrintScheduleCommand printScheduleCommand { get; set; }
        public AutoScheduleCommand autoScheduleCommand { get; set; }

        public event EventHandler SelectedTabChanged;

        private TabInfo selectedTab;
        public TabInfo SelectedTab
        {
            get { return selectedTab; }
            set
            {
                selectedTab = value;
                selectedTab.Orders = CompleteOrders.Where(n => (n.Order.AllocatedMachine ?? "") == value.LatheID).OrderBy(n=>n.Order.StartDate).ToList();
                SelectedTab.CalculateTimings();
                PrintButtonVis = SelectedTab.LatheID == "" ? Visibility.Collapsed : Visibility.Visible;
                AutoScheduleVis = (App.currentUser.UserRole == "admin" || App.currentUser.UserRole == "Scheduling") && SelectedTab.LatheID != "" 
                    ? Visibility.Visible : Visibility.Collapsed;
                SelectedTabChanged?.Invoke(this, new EventArgs());
                OnPropertyChanged("SelectedTab");
            }
        }
        #endregion

        public ScheduleViewModel()
        {
            Orders = new ObservableCollection<LatheManufactureOrder>();
            OrderItems = new ObservableCollection<LatheManufactureOrderItem>();
            CompleteOrders = new List<CompleteOrder>();
            WindowTabs = new List<TabInfo>();

            printScheduleCommand = new PrintScheduleCommand(this);
            autoScheduleCommand = new AutoScheduleCommand(this);

            ReadOrders();
            LoadCompleteOrders();
            CreateTabs();
            
            if (WindowTabs.Count > 1)
                SelectedTab = WindowTabs[1];
        }

        private void ReadOrders()
        {
            // Get Orders
            var orders = DatabaseHelper.Read<LatheManufactureOrder>().ToList().Where(n => n.IsComplete != true);
            Orders.Clear();
            foreach (var order in orders)
                Orders.Add(order);

            // Get Order Items
            var items = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();
            OrderItems.Clear();
            foreach (var item in items)
                OrderItems.Add(item);
        }

        private void LoadCompleteOrders()
        {
            CompleteOrders.Clear();

            foreach (LatheManufactureOrder order in Orders)
            {
                if (order.IsComplete)
                    continue;

                CompleteOrder tmpOrder = new CompleteOrder() { Order = order, OrderItems = new List<LatheManufactureOrderItem>() };

                foreach (LatheManufactureOrderItem item in OrderItems)
                    if (item.AssignedMO == order.Name)
                        tmpOrder.OrderItems.Add(item);

                CompleteOrders.Add(tmpOrder);
            }
        }

        private void CreateTabs()
        {
            WindowTabs.Clear();
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>().ToList();

            WindowTabs.Add(new TabInfo()
            {
                LatheID = "",
                LatheName = "Unallocated",
                ViewTitle = "Awaiting Scheduling"
            });

            foreach (Lathe lathe in lathes)
            {
                WindowTabs.Add(new TabInfo()
                {
                    LatheID = lathe.Id,
                    LatheName = lathe.FullName,
                    Orders = CompleteOrders.Where(n => n.Order.AllocatedMachine == lathe.Id).ToList(),
                    ViewTitle = string.Format("{0} Schedule", lathe.FullName)
                });
            }

            lathes = null;
        }

        public class TabInfo
        {
            public string LatheID { get; set; }
            public string LatheName { get; set; }
            public List<CompleteOrder> Orders { get; set; }
            public string ViewTitle { get; set; }
            public int TotalTime { get; set; }
            public DateTime BookedTo { get; set; }

            public void CalculateTimings()
            {
                TotalTime = (int)0;
                BookedTo = DateTime.MinValue;
                DateTime firstItemStarts = DateTime.MaxValue;

                if(Orders.Count == 0)
                {
                    TotalTime = 0;
                    BookedTo = DateTime.Now;
                    return;
                }

                foreach(CompleteOrder order in Orders)
                {
                    if (order.Order.StartDate < firstItemStarts)
                        firstItemStarts = order.Order.StartDate;
                    TotalTime += 60 * 60 * 3; // Setting time
                    foreach (LatheManufactureOrderItem item in order.OrderItems)
                        TotalTime += item.CycleTime * item.TargetQuantity;
                }

                BookedTo = firstItemStarts.AddDays(1) + TimeSpan.FromSeconds(TotalTime);
            }
        }

        public void updateItem(LatheManufactureOrder order)
        {
            SetDateWindow editWindow = new SetDateWindow(order);
            editWindow.Owner = Application.Current.MainWindow;
            editWindow.ShowDialog();

            if (order.Status == "Awaiting scheduling" && order.AllocatedMachine != "")
            {
                order.Status = !order.IsReady ? "Problem" : "Ready";
                order.ModifiedAt = DateTime.Now;
                order.ModifiedBy = App.currentUser.GetFullName();
            }
                
            order.StartDate = editWindow.SelectedDate;
            order.AllocatedMachine = editWindow.AllocatedMachine;

            DatabaseHelper.Update(order);

            ReadOrders();
            LoadCompleteOrders();
        }

        public void AutoSchedule()
        {
            MessageBox.Show("Not implemented yet", "AutoSchedule");
        }


        public void PrintSchedule()
        {
            MessageBox.Show("Not implemented yet", "Printing");
        }
    }
}
