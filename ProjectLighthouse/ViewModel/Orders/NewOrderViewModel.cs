using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Commands.Orders;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
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
            if (SelectedItem == null)
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
        private List<OrderDrawing> drawingReferences;
        private List<TechnicalDrawing> drawings;
        private List<BreakdownCode> breakdownCodes;
        private List<MachineBreakdown> machineBreakdowns;
        private List<Product> products;
        private List<ProductGroup> productGroups;

        Dictionary<string, ScheduleItem> editCopies = new();

        public OrderViewModelRelayCommand RelayCmd { get; set; }
        public SendMessageCommand SendMessageCmd { get; set; }
        public DeleteNoteCommand DeleteMessageCmd { get; set; }
        public SaveNoteCommand SaveMessageCmd { get; set; }


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
            SendMessageCmd = new(this);
            DeleteMessageCmd = new(this);
            SaveMessageCmd = new(this);
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
            this.drawingReferences = DatabaseHelper.Read<OrderDrawing>();
            this.drawings = DatabaseHelper.Read<TechnicalDrawing>();
            this.breakdownCodes = DatabaseHelper.Read<BreakdownCode>();
            this.machineBreakdowns = DatabaseHelper.Read<MachineBreakdown>();
            this.products = DatabaseHelper.Read<Product>();
            this.productGroups = DatabaseHelper.Read<ProductGroup>();

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

                counts.Add(user, notComplete.Where(x => x.AssignedTo == uniqueUser).Count());
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
                        if (item.State < OrderState.Complete || (item.ModifiedAt ?? DateTime.MinValue).AddDays(1) > DateTime.Now || !item.IsClosed)
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
                results = results.OrderBy(x => x.State == OrderState.Running ? 0 : 1).ThenBy(x => x.State).ToList();

                if (!string.IsNullOrWhiteSpace(SearchString))
                {
                    List<ScheduleItem> filteredResults = new();
                    string sanitised = SearchString.Trim().ToUpperInvariant();
                    foreach (ScheduleItem result in results)
                    {
                        if (result.Name.Contains(sanitised) || (result.POReference ?? string.Empty).Contains(sanitised, StringComparison.InvariantCultureIgnoreCase) || (result.AssignedTo ?? string.Empty).Equals(sanitised, StringComparison.InvariantCultureIgnoreCase))
                        {
                            filteredResults.Add(result);
                            continue;
                        }

                    }

                    results = filteredResults;
                }
            }


            foreach (ScheduleItem item in results)
            {
                item.Lots ??= lots.Where(x => x.Order == item.Name).ToList();
                item.Notes ??= notes.Where(x => x.DocumentReference == item.Name).ToList();

                if (item is LatheManufactureOrder latheOrder)
                {
                    latheOrder.OrderItems ??= orderItems.Where(x => x.AssignedMO == latheOrder.Name).ToList();

                    latheOrder.Bar ??= barStock.Find(x => x.Id == latheOrder.BarID);

                    latheOrder.DrawingsReferences ??= drawingReferences.Where(x => x.OrderId == latheOrder.Name).ToList();
                    latheOrder.Drawings ??= drawings.Where(x => latheOrder.DrawingsReferences.Any(r => r.DrawingId == x.Id)).ToList();

                    latheOrder.Breakdowns ??= machineBreakdowns.Where(x => x.OrderName == latheOrder.Name).ToList();
                    latheOrder.Breakdowns.ForEach(b => b.BreakdownMeta = breakdownCodes.Find(c => c.Id == b.BreakdownCode));

                    latheOrder.ProductGroup ??= productGroups.Find(x => x.Id == latheOrder.GroupId);
                    if (latheOrder.ProductGroup != null && latheOrder.Product == null)
                    {
                        latheOrder.Product = products.Find(x => x.Id == latheOrder.ProductGroup.ProductId);
                    }
                }
                else if (item is GeneralManufactureOrder generalOrder)
                {
                    generalOrder.Item = nonTurnedItems.Find(x => x.Id == generalOrder.NonTurnedItemId);
                }
            }

            FilteredItems = results;
            if(SelectedItem == null && FilteredItems.Count > 0)
            {
                SelectedItem = FilteredItems[0];
            }
        }

        void EditItem()
        {
            if (SelectedItem == null) return;

            ScheduleItem copy = (ScheduleItem)SelectedItem.Clone();

            editCopies.TryAdd(SelectedItem.Name, copy);

            SelectedItem.Editing = true;
        }

        internal void EnterEditMode()
        {
            EditMode = true;
            EditItem();
        }

        internal void ExitEditMode(bool save)
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Nothing selected to save", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedItem.Editing = false;

            ScheduleItem copy;
            try
            {
                copy = editCopies[SelectedItem.Name];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool updated = SelectedItem.IsUpdated(copy);

            if (!updated)
            {
                updated = LotsChanged(newCopy: SelectedItem.Lots, originalCopy: copy.Lots);
            }

            if (updated && save)
            {
                if (!SaveChanges(SelectedItem))
                {
                    MessageBox.Show("Lighthouse encountered an error while saving order data, the operation has been aborted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                ScheduleItem masterVersion = items.Find(x => x.Name == copy.Name);
                masterVersion = SelectedItem;

                SetAssignmentData();

            }
            else if (updated)
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (items[i].Name == copy.Name)
                    {
                        items[i] = copy;
                        break;
                    }
                }

                Filter();

                SelectedItem = FilteredItems.Find(x => x.Name == copy.Name);
            }

            editCopies.Remove(copy.Name);
            EditMode = false;
        }

        private static bool SaveChanges(ScheduleItem selectedItem)
        {
            using SQLiteConnection conn = DatabaseHelper.GetConnection();
            conn.BeginTransaction();

            for (int i = 0; i < selectedItem.Lots.Count; i++)
            {
                Lot lot = selectedItem.Lots[i];

                if (lot.ID == 0)
                {
                    if (conn.Insert(lot) != 1)
                    {
                        conn.Rollback();
                        conn.Close();
                        return false;
                    }
                }
                else
                {
                    if (conn.Update(lot) != 1)
                    {
                        conn.Rollback();
                        conn.Close();
                        return false;
                    }
                }
            }



            selectedItem.ModifiedAt = DateTime.Now;
            selectedItem.ModifiedBy = App.CurrentUser.UserName;
            int rows = conn.Update(selectedItem);

            if (rows != 1)
            {
                conn.Rollback();
                conn.Close();
                return false;
            }

            conn.Commit();
            conn.Close();
            return true;
        }

        private static bool LotsChanged(List<Lot> newCopy, List<Lot> originalCopy)
        {
            if (newCopy.Count != originalCopy.Count) return true;
            List<Lot> addedLots = newCopy.Where(x => !originalCopy.Any(l => l.ID == x.ID)).ToList();

            if (addedLots.Count > 0)
            {
                return true;
            }

            for (int i = 0; i < originalCopy.Count; i++)
            {
                Lot lot = originalCopy[i];

                Lot? newLotCopy = newCopy.Find(x => x.ID == lot.ID)
                    ?? throw new KeyNotFoundException("Could not find lot (logic error)");

                if (lot.IsUpdated(newLotCopy))
                {
                    return true;
                }
            }

            return false;
        }

        internal void SendMessage(Note note)
        {
            note.DocumentReference = SelectedItem.Name;
            note.Message = note.Message.Trim();
            note.OriginalMessage = note.Message;

            try
            {
                DatabaseHelper.Insert<Note>(note, throwErrs: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add to database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            notes = DatabaseHelper.Read<Note>();
            SelectedItem.Notes = SelectedItem.Notes.Append(note).ToList();
        }

        public override void DeleteNote(Note note)
        {
            if (SelectedItem is null) return;

            note.IsDeleted = true;

            try
            {
                DatabaseHelper.Update(note, throwErrs: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add to database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            notes = DatabaseHelper.Read<Note>();
            SelectedItem.Notes = notes.Where(x => x.DocumentReference == SelectedItem.Name).ToList();
        }

        public override void UpdateNote(Note note)
        {
            if (SelectedItem is null) return;

            note.IsEdited = true;
            note.DateEdited = DateTime.Now.ToString("s");

            try
            {
                DatabaseHelper.Update(note, throwErrs: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add to database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            notes = DatabaseHelper.Read<Note>();
            SelectedItem.Notes = notes.Where(x => x.DocumentReference == SelectedItem.Name).ToList();
        }

        public override bool CanClose()
        {
            if (FilteredItems.Any(x => x.Editing))
            {
                MessageBox.Show("Editing stuff");
                return false;
            }

            return true;
        }
    }
}
