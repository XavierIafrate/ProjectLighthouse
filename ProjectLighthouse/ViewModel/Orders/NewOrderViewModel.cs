using DocumentFormat.OpenXml.ExtendedProperties;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Orders;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Orders
{
    public class NewOrderViewModel : BaseViewModel
    {
        private bool editMode;
        public bool EditMode
        {
            get { return editMode; }
            set { editMode = value; OnPropertyChanged(); }
        }

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
            set 
            { 
                selectedItem = value; 
                OnPropertyChanged();
                SetSelectedItem();
            }
        }

        private void SetSelectedItem()
        {
            if(SelectedItem == null)
            {
                EditMode = false;
                return;
            }

            EditMode = editCopies.ContainsKey(SelectedItem.Name);
        }

        private List<ScheduleItem> items;


        private Dictionary<User, int> assignmentCounts;
        public Dictionary<User, int> AssignmentCounts
        {
            get { return assignmentCounts; }
            set { assignmentCounts = value; OnPropertyChanged(); }
        }

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
        private List<User> users;
        private List<Lot> lots;
        private List<Note> notes;

        Dictionary<string, ScheduleItem> editCopies = new();

        public OrderViewModelRelayCommand RelayCmd { get; set; }


        public NewOrderViewModel()
        {
            LoadData();

            SelectedFilter = "All Active";
            SelectedItem = FilteredItems.First();

            SetAssignmentData();

            LoadCommands();
        }

        private void LoadCommands()
        {
            RelayCmd = new(this);
        }

        private void LoadData()
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
            this.users = DatabaseHelper.Read<User>();
            this.lots = DatabaseHelper.Read<Lot>();
            this.notes = DatabaseHelper.Read<Note>();

            this.nonTurnedItems = DatabaseHelper.Read<NonTurnedItem>();
        }

        private void SetAssignmentData()
        {
            Dictionary<User, int> counts = new();
            List<ScheduleItem> notComplete = items.Where(x => x.State < OrderState.Complete).ToList();
            string[] uniqueUsers = notComplete.Where(x => !string.IsNullOrEmpty(x.AssignedTo)).Select(x => x.AssignedTo).Distinct().ToArray();
            foreach (string uniqueUser in uniqueUsers)
            {
                User? user = users.Find(x => x.UserName == uniqueUser);
                user ??= new()
                    {
                        FirstName = "Unknown",
                        LastName = "User",
                        UserName = uniqueUser,
                };

                counts.Add(user, notComplete.Where(x  => x.AssignedTo == uniqueUser).Count());
            }


            User? unassigned = new()
            {
                FirstName = "Unassigned",
                UserName = "unassigned",
                Emoji = "👻",
            };

            int unassignedCount = notComplete.Where(x => string.IsNullOrWhiteSpace(x.AssignedTo)).Count();
            if (unassignedCount > 0)
            {
                counts.Add(unassigned, unassignedCount);
            }

            AssignmentCounts = counts;
        }

        void Filter()
        {
            List<ScheduleItem> results = new();


            foreach (ScheduleItem item in items)
            {
                switch (SelectedFilter)
                {
                    case "All Active":
                        if(item.State < OrderState.Complete || (item.ModifiedAt ?? DateTime.MinValue).AddDays(1) > DateTime.Now || !item.IsClosed )
                        {
                            results.Add(item);
                        }
                        break;

                    case "Assigned To Me":
                        if (item.AssignedTo == App.CurrentUser.UserName)
                        {
                            results.Add(item);
                        }
                        break;

                    case "Not Ready":
                        if (item.State == OrderState.Problem)
                        {
                            results.Add(item);
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
                            results.Add(item);
                        }
                        break;

                    case "Complete":
                        if (item.State > OrderState.Running)
                        {
                            results.Add(item);
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
                        results.Add(item);
                        break;
                }

                results = results.TakeLast(100).ToList();
                results = results.OrderBy(x => x.State == OrderState.Running? 0 : 1).ThenBy(x =>  x.State).ToList();

                if (!string.IsNullOrWhiteSpace(SearchString))
                {
                    List<ScheduleItem> filteredResults = new();
                    string sanitised = SearchString.Trim().ToUpperInvariant();
                    foreach (ScheduleItem result in results)
                    {
                        if (result.Name.Contains(sanitised) || (result.POReference ?? string.Empty).ToUpperInvariant().Contains(sanitised) || (result.AssignedTo?? string.Empty).ToUpperInvariant() == sanitised)
                        {
                            filteredResults.Add(result);
                            continue;
                        }

                    }

                    results = filteredResults;
                }
            }


            foreach(ScheduleItem item in results)
            {
                item.Lots??=lots.Where(x => x.Order == item.Name).ToList();
                item.Notes ??= notes.Where(x => x.DocumentReference == item.Name).ToList();

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

            FilteredItems = results;
        }

        void EditItem()
        {
            if (SelectedItem == null) return;

            ScheduleItem copy = (ScheduleItem)SelectedItem.Clone();

            editCopies.TryAdd(SelectedItem.Name, copy);
        }

        internal void EnterEditMode()
        {
            EditMode = true;
            EditItem();
        }

        internal void ExitEditMode(bool save)
        {
            EditMode = false;

            ScheduleItem copy = editCopies[SelectedItem.Name];
            bool updated = SelectedItem.IsUpdated(copy);

            if (updated && save)
            {
                DatabaseHelper.Update(SelectedItem);

                ScheduleItem masterVersion = items.Find(x => x.Name == copy.Name);
                masterVersion = SelectedItem;

            }
            else if (updated)
            {
                ScheduleItem masterVersion = items.Find(x => x.Name == copy.Name);
                masterVersion = copy;
                SelectedItem = copy;
            }

            editCopies.Remove(copy.Name);


            //MessageBox.Show(updated ? "updated" : "not updated");
        }
    }
}
