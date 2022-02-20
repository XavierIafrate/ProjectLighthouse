using ProjectLighthouse.Model;
using ProjectLighthouse.View.ScheduleViews;
using ProjectLighthouse.ViewModel.Commands.Printing;
using ProjectLighthouse.ViewModel.Commands.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public List<LatheManufactureOrder> ActiveOrders;

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
        public Visibility AddButtonsVis { get; set; }
        public PrintScheduleCommand PrintScheduleCommand { get; set; }
        public AddResearchCommand AddResearchCommand { get; set; }
        public AddServiceCommand AddServiceCommand { get; set; }

        public List<string> FilterOptions { get; set; }

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
        public bool StopRefresh { get; set; }

        public ScheduleViewModel()
        {
            FilteredItems = new();
            FilterOptions = new();

            AddResearchCommand = new(this);
            AddServiceCommand = new(this);
            AddButtonsVis = App.CurrentUser.UserRole is "admin" or "Scheduling"
                ? Visibility.Visible
                : Visibility.Collapsed;
            OnPropertyChanged("AddButtonsVis");

            PrintScheduleCommand = new(this);

            LoadData();
            Filter = FilterOptions.Count > 1
                ? FilterOptions[1]
                : FilterOptions[0];
        }

        private void LoadData()
        {
            ActiveOrders = new();
            OrderHeaders = DatabaseHelper.Read<LatheManufactureOrder>();
            OrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>();

            PlannedServices = DatabaseHelper.Read<MachineService>().Where(x => x.StartDate.AddSeconds(x.TimeToComplete) > System.DateTime.Now).ToList();
            PlannedResearch = DatabaseHelper.Read<ResearchTime>().Where(x => x.StartDate.AddSeconds(x.TimeToComplete) > System.DateTime.Now).ToList();

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
            FilterOptions = new();
            FilterOptions.Add("Unallocated");

            foreach (Lathe lathe in Lathes)
            {
                FilterOptions.Add(lathe.FullName);
            }

            OnPropertyChanged("FilterOptions"); // Is this right
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
                : Visibility.Collapsed;
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

            WorkloadInsights = $"~{Math.Ceiling(2 * ((workload.Item1.TotalDays + 1) / 7)) / 2:N1} weeks [{Math.Ceiling(workload.Item1.TotalDays) + 1} days]";
            BookedUntilInsights = $"Booked until {workload.Item2:dddd, dd MMMM}";

            OnPropertyChanged("MachineInfoInsights");
            OnPropertyChanged("MachineCapabilitiesInsights");
            OnPropertyChanged("NumberOfOrdersInsights");
            OnPropertyChanged("BookedUntilInsights");
            OnPropertyChanged("WorkloadInsights");
        }

        public void AddResearch()
        {
            CreateResearch window = new(new ResearchTime(), Lathes);

            window.ShowDialog();

            if (window.Saved)
            {
                LoadData();
                SetView();
            }
        }

        public void AddMaintenance()
        {
            CreateService window = new(new MachineService(), Lathes);

            window.ShowDialog();

            if (window.Saved)
            {
                LoadData();
                SetView();
            }
        }

        public void PrintSchedule()
        {
            PDFHelper.PrintSchedule(OrderHeaders.ToList(), OrderItems.ToList(), Lathes);
        }

        public void Refresh()
        {
            Debug.WriteLine("Refreshing Schedule");
        }
    }
}
