using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Orders
{
    public class NewOrderViewModel : BaseViewModel
    {
        private string selectedFilter;
        public string SelectedFilter
        {
            get { return selectedFilter; }
            set { selectedFilter = value; Filter(); OnPropertyChanged(); }
        }


        private string searchString;

        public string SearchString
        {
            get { return searchString; }
            set { searchString = value; Filter(); OnPropertyChanged(); }
        }


        private ScheduleItem? selectedItem;
        public ScheduleItem? SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; OnPropertyChanged(); }
        }

        private List<ScheduleItem> items;


        private List<ScheduleItem> filteredItems;

        public List<ScheduleItem> FilteredItems
        {
            get { return filteredItems; }
            set { filteredItems = value; OnPropertyChanged(); }
        }

        private List<Machine> machines;
        private List<LatheManufactureOrderItem> orderItems;
        private List<BarStock> barStock;
        private List<MaterialInfo> materials;
        private List<NonTurnedItem> nonTurnedItems;

        Dictionary<string, ScheduleItem> editCopies = new();


        public NewOrderViewModel()
        {
            machines = DatabaseHelper.Read<Machine>();
            machines.AddRange(DatabaseHelper.Read<Lathe>());

            List<ScheduleItem> items = new();

            items.AddRange(DatabaseHelper.Read<LatheManufactureOrder>());
            items.AddRange(DatabaseHelper.Read<GeneralManufactureOrder>());

            this.items = items;

            this.orderItems = DatabaseHelper.Read<LatheManufactureOrderItem>();
            this.materials = DatabaseHelper.Read<MaterialInfo>();
            this.barStock = DatabaseHelper.Read<BarStock>();
            this.barStock.ForEach(x => x.MaterialData = materials.Find(m => m.Id == x.MaterialId));

            this.nonTurnedItems = DatabaseHelper.Read<NonTurnedItem>();

            SelectedFilter = "All Active";

            SelectedItem = FilteredItems.First();
            //EditItem();
        }


        void Filter()
        {
            List<ScheduleItem> result = new();


            foreach (ScheduleItem item in items)
            {
                switch (SelectedFilter)
                {
                    case "All Active":
                        if(item.State < OrderState.Complete || (item.ModifiedAt ?? DateTime.MinValue).AddDays(1) > DateTime.Now || !item.IsClosed )
                        {
                            result.Add(item);
                        }
                        break;

                    case "Assigned To Me":
                        if (item.AssignedTo == App.CurrentUser.UserName)
                        {
                            result.Add(item);
                        }
                        break;

                    case "Not Ready":
                        if (item.State == OrderState.Problem)
                        {
                            result.Add(item);
                        }
                        break;

                    case "No Program":
                        //FilteredOrders = Orders.Where(n => !n.HasProgram && n.State < OrderState.Complete && n.StartDate > DateTime.MinValue)
                        //    .OrderBy(x => x.StartDate)
                        //    .ToList();
                        break;

                    case "Ready":
                        if (item.State == OrderState.Ready || item.State == OrderState.Prepared)
                        {
                            result.Add(item);
                        }
                        break;

                    case "Complete":
                        if (item.State > OrderState.Running)
                        {
                            result.Add(item);
                        }
                        break;

                    case "Development":
                        //FilteredOrders = Orders
                        //    .Where(n => n.IsResearch && n.State < OrderState.Complete)
                        //    .OrderByDescending(n => n.CreatedAt)
                        //    .Take(200)
                        //    .ToList();
                        break;

                    case "All":
                        result.Add(item);
                        break;
                }

                result = result.TakeLast(100).ToList();

                if (item is LatheManufactureOrder latheOrder)
                {
                    if (latheOrder.OrderItems == null)
                    {
                        latheOrder.OrderItems = orderItems.Where(x => x.AssignedMO == latheOrder.Name).ToList();
                    }

                    if (latheOrder.Bar == null)
                    {
                        latheOrder.Bar = barStock.Find(x => x.Id == latheOrder.BarID);
                    }

                }
                else if (item is GeneralManufactureOrder generalOrder)
                {
                    generalOrder.Item = nonTurnedItems.Find(x => x.Id == generalOrder.NonTurnedItemId);
                }
            }


            foreach(ScheduleItem item in result)
            {
                if (item is LatheManufactureOrder latheOrder)
                {
                    if (latheOrder.OrderItems == null)
                    {
                        latheOrder.OrderItems = orderItems.Where(x => x.AssignedMO == latheOrder.Name).ToList();
                    }

                    if (latheOrder.Bar == null)
                    {
                        latheOrder.Bar = barStock.Find(x => x.Id == latheOrder.BarID);
                    }

                }
                else if (item is GeneralManufactureOrder generalOrder)
                {
                    generalOrder.Item = nonTurnedItems.Find(x => x.Id == generalOrder.NonTurnedItemId);
                }
            }

            FilteredItems = result;
        }

        void EditItem()
        {
            if (SelectedItem == null) return;

            ScheduleItem copy = (ScheduleItem)SelectedItem.Clone();

            editCopies.TryAdd(SelectedItem.Name, copy);
        }
    }
}
