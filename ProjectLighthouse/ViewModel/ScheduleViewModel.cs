using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.ViewModel
{
    public class ScheduleViewModel : BaseViewModel
    {
        public ObservableCollection<LatheManufactureOrder> allOrders { get; set; }
        public ObservableCollection<LatheManufactureOrder> filteredOrders { get; set; }

        public ObservableCollection<LatheManufactureOrderItem> allLMOItems { get; set; }
        public ObservableCollection<LatheManufactureOrderItem> filteredLMOItems { get; set; }

        public ObservableCollection<CompleteOrder> CompleteOrders { get; set; }

        public int totalTime { get; set; }
        public DateTime bookedTo { get; set; }

        private TabItem selectedTab;
        public TabItem SelectedTab
        {
            get { return selectedTab; }
            set
            {
                selectedTab = value;
                if (value.Tag != null)
                {
                    filterOrders(value.Tag.ToString());
                    LoadCompleteOrders();
                }
            }
        }

        public ScheduleViewModel()
        {
            allOrders = new ObservableCollection<LatheManufactureOrder>();
            filteredOrders = new ObservableCollection<LatheManufactureOrder>();

            allLMOItems = new ObservableCollection<LatheManufactureOrderItem>();
            filteredLMOItems = new ObservableCollection<LatheManufactureOrderItem>();

            CompleteOrders = new ObservableCollection<CompleteOrder>();

            totalTime = (int)0;
            bookedTo = DateTime.MinValue;
            
            ReadOrders();
        }

        private void ReadOrders()
        {
            var orders = DatabaseHelper.Read<LatheManufactureOrder>().ToList().Where(n => n.IsComplete != true);
            allOrders.Clear();
            foreach (var order in orders)
            {
                allOrders.Add(order);
            }

            var items = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();
            allLMOItems.Clear();
            foreach (var item in items)
            {
                allLMOItems.Add(item);
            }
            
        }

        private void LoadCompleteOrders()
        {
            CompleteOrders.Clear();
            ObservableCollection<LatheManufactureOrderItem> tmpItems = new ObservableCollection<LatheManufactureOrderItem>();

            totalTime = (int)0;
            bookedTo = DateTime.MinValue;
            DateTime earliestStart = DateTime.MaxValue;
            int orderTime = new int();
            double orderBars = new double();

            foreach(var order in filteredOrders)
            {
                orderTime = (int)0;
                orderBars = (double)0;

                if(order.StartDate < earliestStart)
                {
                    earliestStart = order.StartDate;
                }
                tmpItems.Clear();

                foreach(var item in allLMOItems)
                {
                    if(item.AssignedMO == order.Name)
                    {
                        tmpItems.Add(item);
                        totalTime += item.CycleTime * item.TargetQuantity;
                        orderTime += item.CycleTime * item.TargetQuantity;
                        orderBars += ((item.MajorLength + 2) * item.TargetQuantity) / (double)2700;
                    }
                }

                order.TimeToComplete = orderTime;
                order.NumberOfBars = Math.Ceiling(orderBars);
                DatabaseHelper.Update(order);
                
                CompleteOrder tmpOrder = new CompleteOrder
                {
                    Order = order,
                    OrderItems = new ObservableCollection<LatheManufactureOrderItem>(tmpItems)
                };

                CompleteOrders.Add(tmpOrder);
            }

            bookedTo = earliestStart.AddSeconds(totalTime);
            OnPropertyChanged("bookedTo");
            OnPropertyChanged("totalTime");
        }

        private void filterOrders(string filter)
        {
            filteredOrders.Clear();
            foreach (var order in allOrders)
            {
                if (order.AllocatedMachine == filter)
                {
                    filteredOrders.Add(order);
                }

                if (order.AllocatedMachine == null && filter == "")
                {
                    filteredOrders.Add(order);
                }
            }
            filteredOrders = new ObservableCollection<LatheManufactureOrder>(filteredOrders.OrderBy(n => n.StartDate));
            OnPropertyChanged("filteredOrders");
        }

        public void updateItem(LatheManufactureOrder order)
        {
            SetDateWindow editWindow = new SetDateWindow(order);
            editWindow.Owner = Application.Current.MainWindow;
            editWindow.ShowDialog();

            if (order.Status == "Awaiting scheduling" && order.AllocatedMachine != "")
            {
                if (!order.IsReady)
                {
                    order.Status = "Problem";
                }
                else
                {
                    order.Status = "Ready";
                }
            }

            order.StartDate = editWindow.SelectedDate;
            order.AllocatedMachine = editWindow.AllocatedMachine;
            DatabaseHelper.Update(order);

            ReadOrders();
            filterOrders(SelectedTab.Tag.ToString());
        }
    }
}
