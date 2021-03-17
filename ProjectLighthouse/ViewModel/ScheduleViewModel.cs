using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.ViewModel
{
    public class ScheduleViewModel : BaseViewModel
    {
        public ObservableCollection<LatheManufactureOrder> unallocated { get; set; }

        public ScheduleViewModel()
        {
            unallocated = new ObservableCollection<LatheManufactureOrder>();
            ReadOrders();
        }


        private void ReadOrders()
        {  
            var orders = DatabaseHelper.Read<LatheManufactureOrder>().ToList().Where(n => n.IsComplete != true);
            unallocated.Clear();
            foreach (var order in orders)
            {
                unallocated.Add(order);
            }
        }

    }
}
