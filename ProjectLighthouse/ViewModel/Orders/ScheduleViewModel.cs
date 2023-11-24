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
using System.IO;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Orders
{
    public class ScheduleViewModel : BaseViewModel
    {
        public List<ScheduleWarning> Problems { get; set; }
        public List<MachineService> PlannedServices { get; set; }

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

        public Visibility PrintButtonVis { get; set; }
        public Visibility AddButtonsVis { get; set; }

        public PrintScheduleCommand PrintScheduleCommand { get; set; }
        public ManageHolidaysCommand ManageHolidaysCmd { get; set; }
        public AddServiceCommand AddServiceCommand { get; set; }

        private List<DateTime> holidays;
        public List<DateTime> Holidays
        {
            get { return holidays; }
            set { holidays = value; OnPropertyChanged(); }
        }


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

        private List<ScheduleItem> activeItems;
        public List<ScheduleItem> ActiveItems
        {
            get { return activeItems; }
            set { activeItems = value; OnPropertyChanged(); }
        }


        public ScheduleViewModel()
        {
            FilteredItems = new();
            FilterOptions = new();

            ManageHolidaysCmd = new(this);
            AddServiceCommand = new(this);
            PrintScheduleCommand = new(this);

            AddButtonsVis = App.CurrentUser.Role >= UserRole.Scheduling
                ? Visibility.Visible
                : Visibility.Collapsed;
            OnPropertyChanged(nameof(AddButtonsVis));

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
            ActiveItems = new();

            OrderHeaders = DatabaseHelper.Read<LatheManufactureOrder>();
            OrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>();

            PlannedServices = DatabaseHelper.Read<MachineService>().Where(x => x.StartDate.AddSeconds(x.TimeToComplete).AddDays(7) > DateTime.Now).ToList();

            foreach (LatheManufactureOrder order in OrderHeaders)
            {
                if (order.State < OrderState.Complete)
                {
                    List<LatheManufactureOrderItem> items = OrderItems.Where(i => i.AssignedMO == order.Name).ToList();
                    order.OrderItems = items;
                    ActiveOrders.Add(order);
                }

                ActiveItems.Add(order);
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

            try
            {
                Holidays = new();
                Holidays = LoadHolidays();
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
            }

            OnPropertyChanged(nameof(FilterOptions));
            OnPropertyChanged(nameof(ActiveItems));
            OnPropertyChanged(nameof(Holidays));
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

            FilteredItems.AddRange(PlannedServices.Where(s => s.AllocatedMachine == searchString && s.EndsAt() > DateTime.Today));

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

            ViewTitle = Filter == "Unallocated" ? "Unscheduled Orders" : $"Schedule for {Filter}";
            OnPropertyChanged(nameof(ViewTitle));

            SetAgenda();

            List<ScheduleItem> timelineItems = new();
            timelineItems = new();
            timelineItems.AddRange(OrderHeaders);
            timelineItems.AddRange(PlannedServices);
            ActiveItems = timelineItems;
        }

        private void SetAgenda()
        {
            int numDays = App.CurrentUser.Role >= UserRole.Scheduling ? 28 : 14;
            Agenda = new();
            List<ScheduleItem> ordersOnAgenda = new();
            ordersOnAgenda.AddRange(ActiveOrders
                .Where(x => x.StartDate < DateTime.Now.AddDays(numDays))
                .ToList());
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

                    ordersOnAgenda.Find(x => x.Id == currentOrder!.Id)!.IsZeroSet = currentOrder!.GroupId == lastOrder!.GroupId;

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
            // TODO: Factor in required product times

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

        public void AddMaintenance()
        {
            CreateService window = new(new MachineService(), Lathes) { Owner = App.MainViewModel.MainWindow };

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

        #region Holiday Management
        public void EditHolidays()
        {
            ManageHolidaysWindow window = new(Holidays) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (window.SaveExit)
            {
                try
                {
                    SaveHolidays(window.Holidays);
                    Holidays = new(window.Holidays);
                }
                catch(Exception ex)
                {
                    NotificationManager.NotifyHandledException(ex);
                }
            }
        }

        private static List<DateTime> LoadHolidays()
        {
            try
            {
                string holidaysJson = File.ReadAllText(App.ROOT_PATH + @"lib\holidays.json");
                List<DateTime> holidays = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DateTime>>(holidaysJson);
                return holidays.OrderBy(x => x).ToList();
            }
            catch
            {
                throw;
            }
        }

        private static void SaveHolidays(List<DateTime> holidays)
        {
            try
            {
                string data = Newtonsoft.Json.JsonConvert.SerializeObject(holidays);
                File.WriteAllText(App.ROOT_PATH + @"lib\holidays.json", data);
            }
            catch
            {
                throw;
            }
        }
        #endregion
    }
}
