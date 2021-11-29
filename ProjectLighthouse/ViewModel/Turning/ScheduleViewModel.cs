using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
{
    public class ScheduleViewModel : BaseViewModel
    {
        public List<LatheManufactureOrder> OrderHeaders;
        public List<LatheManufactureOrderItem> OrderItems;
        public List<Lathe> Lathes;

        public string ViewTitle { get; set; }
        public Lathe SelectedLathe { get; set; }

        
        public List<CompleteOrder> ActiveOrders = new();
        private List<CompleteOrder> filteredOrders;

        public List<CompleteOrder> FilteredOrders
        {
            get { return filteredOrders; }
            set
            {
                filteredOrders = value;
                OnPropertyChanged("FilteredOrders");
            }
        }

        public Visibility NoneFoundVis { get; set; }
        public Visibility InsightsVis { get; set; }

        public List<string> filterOptions { get; set; }

        private string filter;
        public string Filter
        {
            get { return filter; }
            set 
            { 
                filter = value;
                SetView();
            }
        }


        public string MachineInfoInsights { get; set; }
        public string MachineCapabilitiesInsights { get; set; }
        public string NumberOfOrdersInsights { get; set; }
        public string BookedUntilInsights { get; set; }
        public string WorkloadInsights { get; set; }


        public ScheduleViewModel()
        {
            FilteredOrders = new();
            filterOptions = new();

            LoadData();
            Filter = filterOptions.Count > 1 
                ? filterOptions[1] 
                : filterOptions[0];
            //SetView();
        }


        private void LoadData()
        {
            OrderHeaders = DatabaseHelper.Read<LatheManufactureOrder>();
            OrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>();

            foreach (LatheManufactureOrder order in OrderHeaders)
            {
                if (order.State < OrderState.Complete)
                {
                    List<LatheManufactureOrderItem> items = OrderItems.Where(i => i.AssignedMO == order.Name).ToList();
                    ActiveOrders.Add(new(
                        order: order,
                        items: items));
                }
            }

            Lathes = DatabaseHelper.Read<Lathe>();
            filterOptions = new();
            filterOptions.Add("Unallocated");

            foreach (Lathe lathe in Lathes)
            {
                filterOptions.Add(lathe.FullName);
            }
            OnPropertyChanged("filterOptions");
        }

        private void SetView()
        {
            string searchString;
            if (Filter == "Unallocated")
            {
                searchString = "";
            }
            else
            {
                SelectedLathe = Lathes.Single(l => l.FullName == Filter);
                searchString = SelectedLathe.Id;

            }

            FilteredOrders = ActiveOrders
                .Where(o => o.Order.AllocatedMachine == searchString)
                .OrderBy(o => o.Order.StartDate)
                .ToList();

            foreach (CompleteOrder order in FilteredOrders)
            {
                order.EditMade += SetView;
            }

            NoneFoundVis = FilteredOrders.Count == 0
                ? Visibility.Visible
                : Visibility.Hidden;
            OnPropertyChanged("NoneFoundVis");

            InsightsVis = !string.IsNullOrEmpty(searchString)
                ? Visibility.Visible
                : Visibility.Hidden;
            OnPropertyChanged("InsightsVis");

            ViewTitle = $"Schedule for {Filter}";

            GetInsights();
        }

        private void GetInsights()
        {
            MachineInfoInsights = $"{SelectedLathe.Make} {SelectedLathe.Model} [ID: {SelectedLathe.Id}]";
            MachineCapabilitiesInsights = $"Max part length: {SelectedLathe.MaxLength}mm\nMax part diameter: {SelectedLathe.MaxDiameter}mm";

            if (FilteredOrders.Count == 0)
            {
                NumberOfOrdersInsights = "No orders assigned.";
            }
            else if (FilteredOrders.Count == 1)
            {
                NumberOfOrdersInsights = "1 order assigned";
            }
            else
            {
                NumberOfOrdersInsights = $"{FilteredOrders.Count} orders assigned";
            }

            var workload = WorkloadCalculationHelper.GetMachineWorkload(FilteredOrders);

            WorkloadInsights = $"{workload.Item1.TotalDays:N0} days work";
            BookedUntilInsights = $"Booked until {workload.Item2:dddd, dd MMMM}";

            OnPropertyChanged("MachineInfoInsights");
            OnPropertyChanged("MachineCapabilitiesInsights");
            OnPropertyChanged("NumberOfOrdersInsights");
            OnPropertyChanged("BookedUntilInsights");
            OnPropertyChanged("WorkloadInsights");
        }

        public void PrintSchedule()
        {
            //PDFHelper.PrintSchedule(Orders.ToList(), OrderItems.ToList(), Lathes);
        }
    }
}
