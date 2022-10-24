using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.View.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Printing;
using ProjectLighthouse.ViewModel.Commands.Scheduling;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Orders
{
    public class ScheduleViewModel : BaseViewModel
    {
        public List<ScheduleWarning> Problems { get; set; }
        public List<MachineService> PlannedServices { get; set; }
        public List<ResearchTime> PlannedResearch { get; set; }

        public List<LatheManufactureOrder> OrderHeaders;
        public List<LatheManufactureOrderItem> OrderItems;
        public List<Lathe> Lathes;

        public List<CalendarDay> Agenda { get; set; }

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
                OnPropertyChanged();
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

            AddButtonsVis = App.CurrentUser.Role >= UserRole.Scheduling
                ? Visibility.Visible
                : Visibility.Collapsed;
            OnPropertyChanged(nameof(AddButtonsVis));

            PrintScheduleCommand = new(this);

            LoadData();
            Filter = FilterOptions.Count > 1
                ? FilterOptions[1]
                : FilterOptions[0];
        }

        private void LoadData()
        {
            ActiveOrders = new();
            Problems = new();
            Agenda = new();

            OrderHeaders = DatabaseHelper.Read<LatheManufactureOrder>();
            OrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>();

            PlannedServices = DatabaseHelper.Read<MachineService>().Where(x => x.StartDate.AddSeconds(x.TimeToComplete) > DateTime.Now).ToList();
            PlannedResearch = DatabaseHelper.Read<ResearchTime>().Where(x => x.StartDate.AddSeconds(x.TimeToComplete) > DateTime.Now).ToList();

            foreach (LatheManufactureOrder order in OrderHeaders)
            {
                if (order.State < OrderState.Complete)
                {
                    List<LatheManufactureOrderItem> items = OrderItems.Where(i => i.AssignedMO == order.Name).ToList();
                    order.OrderItems = items;
                    ActiveOrders.Add(order);
                }
            }

            Lathes = DatabaseHelper.Read<Lathe>().Where(x => !x.OutOfService).ToList();
            FilterOptions = new()
            {
                "Unallocated"
            };

            foreach (Lathe lathe in Lathes)
            {
                FilterOptions.Add(lathe.FullName);
            }

            OnPropertyChanged(nameof(FilterOptions)); // TODO Is this right
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

            OnPropertyChanged(nameof(PrintButtonVis));

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

            Problems.Clear();
            if (Filter != "Unallocated")
            {
                GetProblems();
            }

            FilteredItems.AddRange(Problems);
            FilteredItems = FilteredItems.OrderBy(i => i.StartDate).ToList();

            foreach (ScheduleItem order in FilteredItems)
            {
                order.EditMade += SetView;
            }

            OnPropertyChanged(nameof(FilteredItems));

            NoneFoundVis = FilteredItems.Count == 0
                ? Visibility.Visible
                : Visibility.Hidden;
            OnPropertyChanged(nameof(NoneFoundVis));

            InsightsVis = !string.IsNullOrEmpty(searchString)
                ? Visibility.Visible
                : Visibility.Collapsed;
            OnPropertyChanged(nameof(InsightsVis));


            ViewTitle = Filter == "Unallocated" ? "Unscheduled Orders" : $"Schedule for {Filter}";
            OnPropertyChanged(nameof(ViewTitle));

            GetInsights();
            SetAgenda();
        }

        private void SetAgenda()
        {
            int numDays = 30;
            Agenda = new();
            List<ScheduleItem> ordersOnAgenda = new();
            ordersOnAgenda.AddRange(ActiveOrders
                .Where(x => x.StartDate < DateTime.Now.AddDays(numDays))
                .ToList());
            ordersOnAgenda.AddRange(PlannedResearch);
            ordersOnAgenda.AddRange(PlannedServices);

            ordersOnAgenda = ordersOnAgenda.OrderBy(x => x.StartDate).ToList();

            string[] lathes = ordersOnAgenda.Where(x => !string.IsNullOrEmpty(x.AllocatedMachine)).Select(x => x.AllocatedMachine).ToArray();
            for (int l = 0; l < lathes.Length; l++)
            {
                List<ScheduleItem> orders = ordersOnAgenda.Where(x => x is LatheManufactureOrder && x.AllocatedMachine == lathes[l]).ToList();
                if (orders.Count < 2)
                {
                    continue;
                }

                LatheManufactureOrder lastOrder = orders[0] as LatheManufactureOrder;
                for (int i = 1; i < orders.Count; i++)
                {
                    LatheManufactureOrder currentOrder = orders[i] as LatheManufactureOrder;
                    ordersOnAgenda.Find(x => x.Id == currentOrder.Id).IsZeroSet = currentOrder.ToolingGroup == lastOrder.ToolingGroup;
                    lastOrder = currentOrder;
                }
            }

            for (int i = 0; i < numDays; i++)
            {
                DateTime date = DateTime.Today.AddDays(i);
                Agenda.Add(new CalendarDay(date, ordersOnAgenda.Where(x => x.StartDate.Date == date).ToList()));
            }

            OnPropertyChanged(nameof(Agenda));
        }

        private void GetProblems()
        {
            // TODO: 
            // Factor in required product times

            for (int i = 0; i < FilteredItems.Count - 1; i++)
            {
                DateTime starting = FilteredItems[i].StartDate;
                if (FilteredItems[i] is ResearchTime)
                {
                    starting = starting.Date;
                }
                DateTime ending = starting.AddSeconds(FilteredItems[i].TimeToComplete);
                DateTime nextStarting = FilteredItems[i + 1].StartDate.AddHours(-4); //factor in setting

                if (FilteredItems[i + 1] is MachineService)
                {
                    continue;
                }

                if (starting == nextStarting.AddHours(4)) // factor out setting again
                {
                    Problems.Add(new() { WarningText = $"{FilteredItems[i].Name} and {FilteredItems[i + 1].Name} have the same start date", StartDate = nextStarting.AddSeconds(-1), TimeToComplete = 0, Important = true });
                }
                else if (nextStarting < ending)
                {
                    int resolution = (nextStarting - DateTime.Today).TotalDays switch
                    {
                        < 2 => 3,
                        < 7 => 6,
                        < 14 => 12,
                        < 28 => 24,
                        _ => 48,
                    };

                    if (Math.Abs((nextStarting - ending).TotalHours) >= resolution)
                    {
                        Problems.Add(new() { WarningText = $"{FilteredItems[i].Name} will overflow by {(ending - nextStarting).TotalHours:0} hours", StartDate = nextStarting.AddSeconds(-1), TimeToComplete = 0, Important = true });
                    }
                }
                else
                {
                    int resolution = (nextStarting - DateTime.Today).TotalDays switch
                    {
                        < 2 => 3,
                        < 7 => 6,
                        < 14 => 12,
                        < 28 => 24,
                        _ => 36,
                    };

                    if ((nextStarting - ending).TotalHours >= resolution)
                    {
                        Problems.Add(new() { WarningText = $"Downtime of {(nextStarting - ending).TotalHours:0} hours anticipated", StartDate = nextStarting.AddSeconds(-1), TimeToComplete = 0, Important = false });
                    }
                }
            }
        }

        private void GetInsights()
        {
            if (SelectedLathe == null)
            {
                return;
            }

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

            Tuple<TimeSpan, DateTime> workload = WorkloadCalculationHelper.GetMachineWorkload(FilteredItems);

            WorkloadInsights = $"~{Math.Ceiling(2 * ((workload.Item1.TotalDays + 1) / 7)) / 2:N1} weeks [{Math.Ceiling(workload.Item1.TotalDays) + 1} days]";
            BookedUntilInsights = $"Booked until {workload.Item2:dddd, dd MMMM}";

            OnPropertyChanged(nameof(MachineInfoInsights));
            OnPropertyChanged(nameof(MachineCapabilitiesInsights));
            OnPropertyChanged(nameof(NumberOfOrdersInsights));
            OnPropertyChanged(nameof(BookedUntilInsights));
            OnPropertyChanged(nameof(WorkloadInsights));
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
    }
}
