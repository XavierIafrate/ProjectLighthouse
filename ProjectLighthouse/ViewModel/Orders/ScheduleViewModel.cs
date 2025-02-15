﻿using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Scheduling;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using static ProjectLighthouse.Model.Scheduling.ProductionSchedule;

namespace ProjectLighthouse.ViewModel.Orders
{
    public class ScheduleViewModel : BaseViewModel
    {

        private string searchString;
        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value;
                OnPropertyChanged();
                Search();
            }
        }

        public bool NoResults { get; set; }

        private DateTime minDate = DateTime.Today.AddDays(-3);
        public DateTime MinDate
        {
            get { return minDate; }
            set
            {
                SearchString = string.Empty;
                int existingSpan = (MaxDate - MinDate).Days;
                if (value.Date >= maxDate.Date)
                {
                    MaxDate = value.Date.AddDays(existingSpan);
                }
                minDate = value.Date;
                OnPropertyChanged();
            }
        }

        private DateTime maxDate = DateTime.Today.AddDays(8);
        public DateTime MaxDate
        {
            get { return maxDate; }
            set
            {
                SearchString = string.Empty;
                int existingSpan = (MaxDate - MinDate).Days;
                if (value.Date <= minDate.Date)
                {
                    MinDate = value.Date.AddDays(existingSpan * -1);
                }
                maxDate = value.Date;
                OnPropertyChanged();
            }
        }

        private List<DateTime> holidays;
        public List<DateTime> Holidays
        {
            get { return holidays; }
            set { holidays = value; OnPropertyChanged(); }
        }

        private int columnWidth = 75;
        public int ColumnWidth
        {
            get { return columnWidth; }
            set
            {
                columnWidth = value;
                OnPropertyChanged();
            }
        }

        private int rowHeight = 75;
        public int RowHeight
        {
            get { return rowHeight; }
            set
            {
                rowHeight = value;
                OnPropertyChanged();
            }
        }


        private List<ScheduleItem> scheduleItems;
        public List<ScheduleItem> ScheduleItems
        {
            get { return scheduleItems; }
            set
            {
                scheduleItems = value;
                OnPropertyChanged();
            }
        }


        private ScheduleItem? selectedItem;
        public ScheduleItem? SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                OnPropertyChanged();
            }
        }


        private List<Machine> machines;
        public List<Machine> Machines
        {
            get { return machines; }
            set
            {
                machines = value;
                OnPropertyChanged();
            }
        }

        private ProductionSchedule schedule;
        public ProductionSchedule Schedule
        {
            get { return schedule; }
            set
            {
                schedule = value;
                OnPropertyChanged();
            }
        }

        private SelectScheduleItemCommand selectItemCmd;
        public SelectScheduleItemCommand SelectItemCmd
        {
            get { return selectItemCmd; }
            set
            {
                selectItemCmd = value;
                OnPropertyChanged();
            }
        }

        private RescheduleItemCommand rescheduleCmd;
        public RescheduleItemCommand RescheduleCmd
        {
            get { return rescheduleCmd; }
            set
            {
                rescheduleCmd = value;
                OnPropertyChanged();
            }
        }

        private DeleteMaintenanceEventCommand deleteMaintenanceEventCmd;

        public DeleteMaintenanceEventCommand DeleteMaintenanceEventCmd
        {
            get { return deleteMaintenanceEventCmd; }
            set { deleteMaintenanceEventCmd = value; OnPropertyChanged(); }
        }


        public ResetDatesCommand ResetDatesCmd { get; set; }


        public ScheduleViewModel()
        {
            SelectItemCmd = new(this);
            RescheduleCmd = new(this);
            ResetDatesCmd = new(this);
            DeleteMaintenanceEventCmd = new(this);

            LoadData();
            ResetDates();
        }

        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchString))
            {
                SelectedItem = null;
                NoResults = false;
                OnPropertyChanged(nameof(NoResults));
                return;
            }

            ScheduleItem? foundItem = Schedule.Find(SearchString);

            if (foundItem is not null) // must be before assigning SelectedItem
            {
                EnsureItemIsInDateWindow(foundItem);
            }

            SelectedItem = foundItem;
            NoResults = foundItem == null;
            OnPropertyChanged(nameof(NoResults));
        }

        private void EnsureItemIsInDateWindow(ScheduleItem foundItem)
        {
            DateTime startDate = foundItem.StartDate.Date;
            DateTime endDate = foundItem.EndsAt().Date;

            if (foundItem is LatheManufactureOrder order)
            {
                startDate = order.GetSettingStartDateTime().Date;
                endDate = order.EndsAt().Date;
            }

            if (MinDate > startDate || MaxDate < endDate)
            {
                int existingSpan = ((DateTime)MaxDate! - (DateTime)MinDate!).Days;
                int itemSpan = (endDate.Date - startDate.Date).Days + 1;
                int requiredSpan = Math.Max(existingSpan, itemSpan);

                minDate = startDate;
                maxDate = startDate.AddDays(requiredSpan);

                OnPropertyChanged(nameof(MinDate));
                OnPropertyChanged(nameof(MaxDate));
            }
        }


        private void LoadData()
        {
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>(throwErrs: true);
            List<Machine> machines = DatabaseHelper.Read<Machine>(throwErrs: true);
            machines.AddRange(lathes);

            this.Machines = machines;

            ScheduleItems = GetScheduleItems();

            try
            {
                Holidays = new();
                Holidays = LoadHolidays();
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
            }

            Schedule = new(Machines, ScheduleItems, Holidays);
        }

        private static List<ScheduleItem> GetScheduleItems()
        {
            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>(throwErrs: true).Where(x => x.State != OrderState.Cancelled).ToList();
            List<LatheManufactureOrderItem> orderItems = DatabaseHelper.Read<LatheManufactureOrderItem>(throwErrs: true);
            List<GeneralManufactureOrder> generalOrders = DatabaseHelper.Read<GeneralManufactureOrder>(throwErrs: true);
            List<NonTurnedItem> generalOrderItems = DatabaseHelper.Read<NonTurnedItem>(throwErrs: true);
            List<MachineService> machineServices = DatabaseHelper.Read<MachineService>(throwErrs: true);
            List<BarStock> bars = DatabaseHelper.Read<BarStock>(throwErrs: true);
            List<MaterialInfo> materials = DatabaseHelper.Read<MaterialInfo>(throwErrs: true);

            bars.ForEach(b => b.MaterialData = materials.Find(m => m.Id == b.MaterialId));

            List<ScheduleItem> allItems = new();
            orders.ForEach(x =>
            {
                x.OrderItems = orderItems.Where(i => i.AssignedMO == x.Name).ToList();
                x.Bar = bars.Find(b => b.Id == x.BarID);
                allItems.Add(x);
            }
            );

            generalOrders.ForEach(x => x.Item = generalOrderItems.Find(i => i.Id == x.NonTurnedItemId));

            machineServices.ForEach(x => allItems.Add(x));
            allItems.AddRange(generalOrders);

            return allItems;
        }

        internal void SelectItem(ScheduleItem? item)
        {
            SelectedItem = item;
        }

        internal void Reschedule(RescheduleInformation info)
        {
            if (info.createRecord)
            {
                try
                {
                    AddNewItem(info.item);
                }
                catch (Exception ex)
                {
                    NotificationManager.NotifyHandledException(ex);
                    return;
                }

                Schedule.UnallocatedItems.Add(info.item);
            }

            try
            {
                Schedule.RescheduleItem(info);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            SelectedItem = null;
            SelectedItem = info.item;
        }

        private static void AddNewItem(ScheduleItem item)
        {
            if (item is MachineService service)
            {
                try
                {
                    DatabaseHelper.Insert<MachineService>(service, throwErrs: true);
                }
                catch
                {
                    throw;
                }
                return;
            }

            throw new NotImplementedException();
        }

        internal void ResetDates()
        {
            this.minDate = DateTime.Today.AddDays(-3);
            this.maxDate = ScheduleItems.Max(x => x.EndsAt()).Date.AddDays(3);
            this.maxDate = this.maxDate > DateTime.Today.AddDays(42) ? DateTime.Today.AddDays(42) : this.maxDate;

            this.searchString = string.Empty;
            OnPropertyChanged(nameof(SearchString));

            OnPropertyChanged(nameof(MinDate));
            OnPropertyChanged(nameof(MaxDate));
        }

        #region Holiday Management
        private static List<DateTime> LoadHolidays()
        {
            try
            {
                string holidaysJson = File.ReadAllText(App.ROOT_PATH + @"lib\holidays.json");
                List<DateTime> holidays = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DateTime>>(holidaysJson);

                if (holidays == null)
                {
                    return new();
                }

                return holidays.OrderBy(x => x).ToList();
            }
            catch
            {
                throw;
            }
        }

        #endregion

        internal void CancelService(MachineService service)
        {
            
            bool success;

            try
            {
                success = DatabaseHelper.Delete(service);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete service: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!success)
            {
                MessageBox.Show($"Failed to delete service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Schedule.RemoveService(service);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            SelectedItem = null;
        }
    }
}
