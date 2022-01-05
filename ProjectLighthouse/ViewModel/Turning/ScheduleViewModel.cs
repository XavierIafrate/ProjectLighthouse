using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Commands.Printing;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class ScheduleViewModel : BaseViewModel
    {
        public List<MachineService> PlannedServices { get; set; }
        public List<ResearchTime> PlannedResearch { get; set; }

        public List<LatheManufactureOrder> OrderHeaders;
        public List<LatheManufactureOrderItem> OrderItems;
        public List<Lathe> Lathes;

        public string ViewTitle { get; set; }
        public Lathe SelectedLathe { get; set; }

        public List<LatheManufactureOrder> ActiveOrders = new();

        private List<ScheduleItem> _filteredItems;
        public List<ScheduleItem> FilteredItems
        {
            get { return _filteredItems; }
            set
            {
                _filteredItems = value;
                OnPropertyChanged("FilteredItems");
            }
        }

        public Visibility NoneFoundVis { get; set; }
        public Visibility InsightsVis { get; set; }
        public Visibility PrintButtonVis { get; set; }
        public PrintScheduleCommand PrintScheduleCommand { get; set; }

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
            FilteredItems = new();
            filterOptions = new();

            PrintScheduleCommand = new(this);

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

            PlannedServices = DatabaseHelper.Read<MachineService>();
            PlannedResearch = DatabaseHelper.Read<ResearchTime>();

            foreach (LatheManufactureOrder order in OrderHeaders)
            {
                if (order.State < OrderState.Complete)
                {
                    List<LatheManufactureOrderItem> items = OrderItems.Where(i => i.AssignedMO == order.Name).ToList();
                    order.OrderItems = items;
                    ActiveOrders.Add(order);
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
                searchString = null;
                PrintButtonVis = Visibility.Collapsed;
            }
            else
            {
                SelectedLathe = Lathes.Single(l => l.FullName == Filter);
                searchString = SelectedLathe.Id;
                PrintButtonVis = Visibility.Visible;
            }
            OnPropertyChanged("PrintButtonVis");

            FilteredItems = new();
            FilteredItems.Clear();

            FilteredItems.AddRange(
                    ActiveOrders
                        .Where(o => o.AllocatedMachine == searchString)
                        .OrderBy(o => o.StartDate)
                        .ToList());

            FilteredItems.AddRange(PlannedServices.Where(s => s.AllocatedMachine == searchString));
            FilteredItems.AddRange(PlannedResearch.Where(r => r.AllocatedMachine == searchString));

            FilteredItems = FilteredItems.OrderBy(i => i.StartDate).ToList();

            foreach (ScheduleItem order in FilteredItems)
            {
                order.EditMade += SetView;
            }

            OnPropertyChanged("FilteredItems");

            NoneFoundVis = FilteredItems.Count == 0
                ? Visibility.Visible
                : Visibility.Hidden;
            OnPropertyChanged("NoneFoundVis");

            InsightsVis = !string.IsNullOrEmpty(searchString)
                ? Visibility.Visible
                : Visibility.Hidden;
            OnPropertyChanged("InsightsVis");

            ViewTitle = $"Schedule for {Filter}";
            OnPropertyChanged("ViewTitle");

            GetInsights();
        }

        private void GetInsights()
        {
            MachineInfoInsights = $"{SelectedLathe.Make} {SelectedLathe.Model} [ID: {SelectedLathe.Id}]";
            MachineCapabilitiesInsights = $"Max part length: {SelectedLathe.MaxLength}mm\nMax part diameter: {SelectedLathe.MaxDiameter}mm";

            if (FilteredItems.Count == 0)
            {
                NumberOfOrdersInsights = "No orders assigned.";
            }
            else if (FilteredItems.Count == 1)
            {
                NumberOfOrdersInsights = "1 order assigned";
            }
            else
            {
                NumberOfOrdersInsights = $"{FilteredItems.Count} orders assigned";
            }

            System.Tuple<System.TimeSpan, System.DateTime> workload = WorkloadCalculationHelper.GetMachineWorkload(FilteredItems);

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
            PDFHelper.PrintSchedule(OrderHeaders.ToList(), OrderItems.ToList(), Lathes);
        }
    }
}
