using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Commands.Printing;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class
        ScheduleViewModel : BaseViewModel
    {
        #region Variables
        public List<LatheManufactureOrder> Orders { get; set; }
        public List<LatheManufactureOrderItem> OrderItems { get; set; }
        public List<Lathe> Lathes { get; set; }
        public List<CompleteOrder> CompleteOrders { get; set; }
        public List<TabInfo> WindowTabs { get; set; }
        public DisplayLMOScheduling SelectedOrder { get; set; }

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

        public PrintScheduleCommand PrintScheduleCommand { get; set; }
        public AutoScheduleCommand AutoScheduleCommand { get; set; }
        public UpdateItemOnScheduleCommand UpdateItemCommand { get; set; }
        public event EventHandler SelectedTabChanged;

        private TabInfo selectedTab;
        public TabInfo SelectedTab
        {
            get { return selectedTab; }
            set
            {
                selectedTab = value;
                LoadTabOrders();
                OnPropertyChanged("SelectedTab");
            }
        }


        #endregion

        public ScheduleViewModel()
        {
            Debug.WriteLine("Init: ScheduleViewModel");
            Orders = new();
            OrderItems = new();
            CompleteOrders = new();
            Lathes = new();
            WindowTabs = new();
            SelectedOrder = new();

            PrintScheduleCommand = new(this);
            AutoScheduleCommand = new(this);
            UpdateItemCommand = new(this);

            ReadOrders();
            LoadCompleteOrders();
            CreateTabs();

            if (WindowTabs.Count > 1)
            {
                SelectedTab = WindowTabs[1];
            }
        }

        private void ReadOrders()
        {
            Orders.Clear();
            Orders = DatabaseHelper.Read<LatheManufactureOrder>().Where(o => !o.IsComplete).ToList();
            OnPropertyChanged("Orders");

            OrderItems.Clear();
            OrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();
            OnPropertyChanged("OrderItems");
        }

        private void LoadTabOrders()
        {
            SelectedTab.Orders = null;
            SelectedTab.Orders = CompleteOrders.Where(n => (n.Order.AllocatedMachine ?? "") == SelectedTab.LatheID).OrderBy(n => n.Order.StartDate).ToList();
            SelectedTab.CalculateTimings();
            PrintButtonVis = SelectedTab.LatheID == "" ? Visibility.Collapsed : Visibility.Visible;
            AutoScheduleVis = (App.CurrentUser.UserRole == "admin" || App.CurrentUser.UserRole == "Scheduling") && SelectedTab.LatheID != ""
                ? Visibility.Visible
                : Visibility.Collapsed;
            OnPropertyChanged("SelectedTab.Orders");
        }

        private void LoadCompleteOrders()
        {
            CompleteOrders.Clear();

            foreach (LatheManufactureOrder order in Orders)
            {
                CompleteOrder tmpOrder = new()
                {
                    Order = order,
                    OrderItems = new(),
                    UpdateCommand = UpdateItemCommand
                };

                tmpOrder.OrderItems.AddRange(OrderItems.Where(i => i.AssignedMO == order.Name).ToList());
                CompleteOrders.Add(tmpOrder);
            }
            OnPropertyChanged("CompleteOrders");
        }

        private void CreateTabs()
        {
            WindowTabs.Clear();
            Lathes = DatabaseHelper.Read<Lathe>().ToList();

            WindowTabs.Add(new TabInfo()
            {
                LatheID = "",
                LatheName = "Unallocated",
                ViewTitle = "Awaiting Scheduling"
            });

            foreach (Lathe lathe in Lathes)
            {
                WindowTabs.Add(new TabInfo()
                {
                    LatheID = lathe.Id,
                    LatheName = lathe.FullName,
                    Orders = CompleteOrders.Where(n => n.Order.AllocatedMachine == lathe.Id).ToList(),
                    ViewTitle = $"{lathe.FullName} Schedule"
                });
            }
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
                TotalTime = 0;
                BookedTo = DateTime.MinValue;
                DateTime firstItemStarts = DateTime.MaxValue;

                if (Orders.Count == 0)
                {
                    TotalTime = 0;
                    BookedTo = DateTime.Now;
                    return;
                }

                foreach (CompleteOrder order in Orders)
                {
                    if (order.Order.StartDate < firstItemStarts)
                    {
                        firstItemStarts = order.Order.StartDate;
                    }

                    TotalTime += 60 * 60 * 3; // Setting time
                    foreach (LatheManufactureOrderItem item in order.OrderItems)
                    {
                        TotalTime += item.CycleTime * item.TargetQuantity;
                    }
                }

                BookedTo = firstItemStarts.AddDays(1) + TimeSpan.FromSeconds(TotalTime);
            }
        }

        public void UpdateOrder(LatheManufactureOrder order)
        {
            SetDateWindow editWindow = new(order);
            editWindow.Owner = Application.Current.MainWindow;
            editWindow.ShowDialog();

            if (!editWindow.SaveExit)
                return;

            if (order.Status == "Awaiting scheduling" && order.AllocatedMachine != "")
            {
                order.Status = !order.IsReady ? "Problem" : "Ready";
                order.ModifiedAt = DateTime.Now;
                order.ModifiedBy = App.CurrentUser.GetFullName();
            }

            order.StartDate = editWindow.SelectedDate;
            order.AllocatedMachine = editWindow.AllocatedMachine;

            DatabaseHelper.Update(order);

            ReadOrders();
            LoadCompleteOrders();
            LoadTabOrders();
        }

        //public void AutoSchedule()
        //{
        //    MessageBox.Show("Not implemented yet", "AutoSchedule");
        //}


        public void PrintSchedule()
        {
            PDFHelper.PrintSchedule(Orders.ToList(), OrderItems.ToList(), Lathes);
        }
    }
}
