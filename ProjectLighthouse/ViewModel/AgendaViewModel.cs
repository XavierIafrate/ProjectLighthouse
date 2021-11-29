using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Turning;
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
                .Where(o => o.State < OrderState.Complete)
                .OrderBy(o => o.StartDate)
                .ToList();

            DateTime startDate = DateTime.Today.Date; //activeOrders.First().StartDate;
            DateTime endDate = activeOrders.Last().StartDate;

            Debug.WriteLine($"Days = {(endDate - startDate).Days}");

            for (int i = 0; i < (endDate - startDate).Days; i++)
            {
                List<LatheManufactureOrder> ordersOnDay = activeOrders.Where(o => o.StartDate.Date == startDate.AddDays(i)).ToList();
                DateTime day = startDate.AddDays(i);
                if (day.DayOfWeek is not DayOfWeek.Sunday and not DayOfWeek.Saturday)
                {
                    Days.Add(new(
                           startDate.AddDays(i),
                           ordersOnDay));
                }
                else if (ordersOnDay.Count > 0)
                {
                    Days.Add(new(
                           startDate.AddDays(i),
                           ordersOnDay));
                }  
            }
        }
    }
}
