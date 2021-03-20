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

namespace ProjectLighthouse.ViewModel
{
    public class ScheduleViewModel : BaseViewModel
    {
        public ObservableCollection<LatheManufactureOrder> unallocated { get; set; }
        public ObservableCollection<LatheManufactureOrder> C1items { get; set; }
        public ObservableCollection<LatheManufactureOrder> C2items { get; set; }

        public ScheduleViewModel()
        {
            unallocated = new ObservableCollection<LatheManufactureOrder>();
            C1items = new ObservableCollection<LatheManufactureOrder>();
            C2items = new ObservableCollection<LatheManufactureOrder>();
            ReadOrders();
        }


        private void ReadOrders()
        {  
            var orders = DatabaseHelper.Read<LatheManufactureOrder>().ToList().Where(n => n.IsComplete != true);

            unallocated.Clear();
            C1items.Clear();
            C2items.Clear();

            foreach (var order in orders)
            {
                if (order.AllocatedMachine == "C01")
                {
                    C1items.Add(order);
                }
                else if (order.AllocatedMachine == "C02")
                {
                    C2items.Add(order);
                }
                else
                {
                    unallocated.Add(order);
                }
            }

            C1items = new ObservableCollection<LatheManufactureOrder>(C1items.OrderBy(n => n.StartDate));
            C2items = new ObservableCollection<LatheManufactureOrder>(C2items.OrderBy(n => n.StartDate));

            OnPropertyChanged("unallocated");
            OnPropertyChanged("C1items");
            OnPropertyChanged("C2items");
        }

        public void updateItem(LatheManufactureOrder order)
        {
            SetDateWindow editWindow = new SetDateWindow(order);
            editWindow.Owner = Application.Current.MainWindow;
            editWindow.ShowDialog();
            order.StartDate = editWindow.SelectedDate;
            order.AllocatedMachine = editWindow.AllocatedMachine;
            DatabaseHelper.Update(order);

            ReadOrders();
        }

    }
}
