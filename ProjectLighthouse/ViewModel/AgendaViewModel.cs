using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;

namespace ProjectLighthouse.ViewModel
{
    public class AgendaViewModel : BaseViewModel
    {
        private List<CalendarDay> days;
        public List<CalendarDay> Days
        {
            get => days;
            set
            {
                days = value;
                OnPropertyChanged("Days");
            }
        }

        private List<LatheManufactureOrder> orders;

        public List<LatheManufactureOrder> Orders
        {
            get => orders;
            set
            {
                orders = value;
                OnPropertyChanged("Orders");
            }
        }


        public AgendaViewModel()
        {
            LoadData();
        }

        public void LoadData()
        {
            Orders = DatabaseHelper.Read<LatheManufactureOrder>();
            Days = new();

            List<LatheManufactureOrder> activeOrders = Orders
                .Where(o => o.Status != "Complete")
                .OrderBy(o => o.StartDate)
                .ToList();

            DateTime startDate = activeOrders.First().StartDate;
            DateTime endDate = activeOrders.Last().StartDate;

            Debug.WriteLine($"Days = {(endDate - startDate).Days}");

            for (int i = 0; i < (endDate - startDate).Days; i++)
            {
                Days.Add(new(
                    startDate.AddDays(i),
                    activeOrders.Where(o => o.StartDate.Date == startDate.AddDays(i)).ToList()
                    ));
            }




        }
    }
}
