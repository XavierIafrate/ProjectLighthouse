using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.View.Orders;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Commands.Orders;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Orders
{
    public class NewOrderViewModel : BaseViewModel
    {
        #region Observables
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
                if (selectedItem is not null)
                {
                    selectedItem.PropertyChanged -= ItemUpdated;
                }

                selectedItem = value;

                if (selectedItem is not null)
                {
                    selectedItem.PropertyChanged += ItemUpdated;
                }

                OnPropertyChanged();
                SetSelectedItem();
            }
        }

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
        #endregion

        #region Core Data
        private List<ScheduleItem> items;
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
        #endregion

        Dictionary<string, ScheduleItem> editCopies = new();

        public OrderViewModelRelayCommand RelayCmd { get; set; }
        public SendMessageCommand SendMessageCmd { get; set; }
        public DeleteNoteCommand DeleteMessageCmd { get; set; }
        public SaveNoteCommand SaveMessageCmd { get; set; }
        public CreateNewOrderCommand CreateNewOrderCmd { get; set; }


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
            CreateNewOrderCmd = new(this);
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

        public void CreateNewOrder()
        {
            string targetName;
            if (MessageBox.Show("Would you like to raise a Lathe order?", "Choose option", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                OrderConstructorWindow window = new()
                {
                    Owner = Application.Current.MainWindow
                };

                window.ShowDialog();

                if (!window.SaveExit)
                {
                    return;
                }
                targetName = window.NewOrder.Name;
            }
            else
            {
                GeneralOrderConstructorWindow window = new() { Owner = Application.Current.MainWindow };

                window.ShowDialog();

                if (!window.SaveExit)
                {
                    return;
                }

                targetName = window.NewOrder.Name;
            }


            LoadData();
            Filter();

            SelectedItem = FilteredItems.Find(x => x.Name == targetName);

            if (SelectedItem is null && FilteredItems.Count > 0)
            {
                SelectedItem = FilteredItems[0];
            }
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

            EnrichData(results);

            FilteredItems = results;
            if (SelectedItem == null && FilteredItems.Count > 0)
            {
                SelectedItem = FilteredItems[0];
            }
        }

        private void EnrichData(List<ScheduleItem> results)
        {
            foreach (ScheduleItem item in results)
            {
                item.Lots ??= lots.Where(x => x.Order == item.Name).OrderBy(x => x.DateProduced).ToList();
                item.Notes ??= notes.Where(x => x.DocumentReference == item.Name).ToList();

                if (item is LatheManufactureOrder latheOrder)
                {
                    latheOrder.OrderItems ??= orderItems.Where(x => x.AssignedMO == latheOrder.Name).OrderByDescending(x => x.RequiredQuantity).ThenBy(x => x.ProductName).ToList();

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

                    latheOrder.Briefing = GetBriefing(latheOrder);
                }
                else if (item is GeneralManufactureOrder generalOrder)
                {
                    generalOrder.Item = nonTurnedItems.Find(x => x.Id == generalOrder.NonTurnedItemId);
                }
            }
        }

        private Briefing GetBriefing(LatheManufactureOrder latheOrder)
        {
            List<LatheManufactureOrder> ordersOfArchetype = new();
            for (int i = 0; i < items.Count; i++)
            {
                ScheduleItem item = items[i];
                if (item is not LatheManufactureOrder order) continue;
                if (order.GroupId == latheOrder.GroupId && order.StartDate > DateTime.MinValue && order.IsComplete && order.Name != latheOrder.Name && order.StartDate < latheOrder.CreatedAt)
                {
                    ordersOfArchetype.Add(order);
                }
            }

            Briefing result = new()
            {
                OrderName = latheOrder.Name,
                NumberOfTimesRun = ordersOfArchetype.Count,
                RunInMaterialBefore = ordersOfArchetype.Any(x => x.MaterialId == latheOrder.MaterialId)
            };
            result.ArchetypeRunBefore = result.NumberOfTimesRun > 0;
            if (result.ArchetypeRunBefore)
            {
                result.LastRun = ordersOfArchetype.Max(x => x.StartDate);
            }

            return result;
        }


        internal void EnterEditMode()
        {
            if (SelectedItem == null) return;
            EditMode = true;

            ScheduleItem copy = (ScheduleItem)SelectedItem.Clone();
            editCopies.TryAdd(SelectedItem.Name, copy);

            SelectedItem.Editing = true;
        }

        internal void ExitEditMode(bool save)
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Nothing selected to save", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


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

            if (SelectedItem is LatheManufactureOrder latheOrder && copy is LatheManufactureOrder latheOrderCopy)
            {
                bool updated = DetectLatheOrderChanges(latheOrder, latheOrderCopy);

                if (updated && save)
                {
                    bool saved = SaveLatheOrder(latheOrder, latheOrderCopy);

                    if (!saved)
                    {
                        MessageBox.Show("failed to update database");
                        return;
                    }
                    latheOrder.Editing = false;

                    for (int i = items.Count - 1; i >= 0; i--)
                    {
                        if (items[i].Name == latheOrder.Name)
                        {
                            items[i] = (LatheManufactureOrder)latheOrder.Clone();
                            break;
                        }
                    }

                    Filter();
                    SelectedItem = FilteredItems.Find(x => x.Name == copy.Name);
                    SetAssignmentData();
                }
                else if (updated)
                {
                    RestoreCopy(copy);
                    SetAssignmentData();
                }
                else
                {
                    SelectedItem.Editing = false;
                }
            }
            else if (SelectedItem is GeneralManufactureOrder generalOrder && copy is GeneralManufactureOrder generalOrderCopy)
            {
                bool updated = DetectGeneralOrderChanges(generalOrder, generalOrderCopy);

                if (updated && save)
                {
                    bool saved = SaveGeneralOrder(generalOrder);

                    if (!saved)
                    {
                        MessageBox.Show("failed to update database");
                        return;
                    }
                    generalOrder.Editing = false;

                    for (int i = items.Count - 1; i >= 0; i--)
                    {
                        if (items[i].Name == generalOrder.Name)
                        {
                            items[i] = (GeneralManufactureOrder)generalOrder.Clone();
                            break;
                        }
                    }
                    Filter();
                    SelectedItem = FilteredItems.Find(x => x.Name == copy.Name);
                    SetAssignmentData();
                }
                else if (updated)
                {
                    RestoreCopy(copy);
                    SetAssignmentData();
                }
                else
                {
                    SelectedItem.Editing = false;
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            editCopies.Remove(copy.Name);
            EditMode = false;
        }

        private void RestoreCopy(ScheduleItem copy)
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

        private static bool SaveLatheOrder(LatheManufactureOrder latheOrder, LatheManufactureOrder oldCopy)
        {
            using SQLiteConnection conn = DatabaseHelper.GetConnection();
            conn.BeginTransaction();

            if (!UpdateLots(latheOrder.Lots, oldCopy.Lots, conn))
            {
                conn.Rollback();
                conn.Close();
                return false;
            }

            if (!UpdateItems(latheOrder.OrderItems, oldCopy.OrderItems, conn))
            {
                conn.Rollback();
                conn.Close();
                return false;
            }

            // Breakdowns

            // Update master cycle time...

            // Drawings

            latheOrder.ModifiedAt = DateTime.Now;
            latheOrder.ModifiedBy = App.CurrentUser.UserName;

            if (conn.Update(latheOrder) != 1)
            {
                conn.Rollback();
                conn.Close();
                return false;
            }

            conn.Commit();
            conn.Close();
            return true;
        }

        private static bool UpdateItems(List<LatheManufactureOrderItem> to, List<LatheManufactureOrderItem> from, SQLiteConnection conn)
        {
            List<LatheManufactureOrderItem> added = to.Where(x => x.Id == 0).ToList();
            List<LatheManufactureOrderItem> removed = from.Where(x => !to.Any(i => i.Id == x.Id)).ToList();

            List<LatheManufactureOrderItem> updated = new();
            for (int i = 0; i < to.Count; i++)
            {
                LatheManufactureOrderItem i1 = to[i];
                if (i1.Id == 0) continue;
                LatheManufactureOrderItem? i2 = from.Find(x => x.Id == i1.Id) 
                    ?? throw new KeyNotFoundException($"could not find Lathe Order Item with ID '{i1.Id}' in copy");
                if (i2.IsUpdated(i1))
                {
                    updated.Add(i1);
                }
            }


            foreach (LatheManufactureOrderItem item in updated)
            {
                if (conn.Update(item) != 1)
                {
                    return false;
                }

                if (item.CycleTime > 0)
                {
                    if (conn.Execute($"UPDATE {nameof(TurnedProduct)} SET CycleTime = {item.CycleTime} WHERE ProductName = '{item.ProductName}'") != 1)
                    {
                        return false;
                    }

                }
            }
            

            if (conn.InsertAll(added) != added.Count)
            {
                return false;
            }

            foreach (LatheManufactureOrderItem item in removed)
            {
                if (conn.Delete(item) != 1)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool UpdateLots(List<Lot> to, List<Lot> from, SQLiteConnection conn)
        {
            List<Lot> added = to.Where(x => x.ID == 0).ToList();
            List<Lot> updated = new();
            for (int i = 0; i < to.Count; i++)
            {
                Lot l1 = to[i];
                if (l1.ID == 0) continue;
                Lot? l2 = from.Find(x => x.ID == l1.ID) 
                    ?? throw new KeyNotFoundException($"could not find lot with ID '{l1.ID}' in copy");
                if (l2.IsUpdated(l1))
                {
                    updated.Add(l1);
                }
            }

            if (conn.UpdateAll(updated) != updated.Count)
            {
                return false;
            }

            if (conn.InsertAll(added) != added.Count)
            {
                return false;
            }

            return true;
        }

        private static bool SaveGeneralOrder(GeneralManufactureOrder generalOrder)
        {
            using SQLiteConnection conn = DatabaseHelper.GetConnection();
            conn.BeginTransaction();

            for (int i = 0; i < generalOrder.Lots.Count; i++)
            {
                Lot lot = generalOrder.Lots[i];
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

            generalOrder.ModifiedAt = DateTime.Now;
            generalOrder.ModifiedBy = App.CurrentUser.UserName;
            int rows = conn.Update(generalOrder);

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


        #region Lathe Order Change Detection
        private static bool DetectLatheOrderChanges(LatheManufactureOrder latheOrder, LatheManufactureOrder latheOrderCopy)
        {
            bool updated = latheOrderCopy.IsUpdated(latheOrder);
            if (updated) return updated;

            updated = LotsChanged(newCopy: latheOrder.Lots, originalCopy: latheOrderCopy.Lots);
            if (updated) return updated;

            updated = OrderItemsChanged(latheOrderCopy.OrderItems, latheOrder.OrderItems);
            if (updated) return updated;

            updated = DrawingReferencesChanged(latheOrderCopy.DrawingsReferences, latheOrder.DrawingsReferences);
            if (updated) return updated;

            updated = BreakdownsChanged(latheOrderCopy.Breakdowns, latheOrder.Breakdowns);
            if (updated) return updated;

            return updated;
        }


        private static bool BreakdownsChanged(List<MachineBreakdown> breakdowns1, List<MachineBreakdown> breakdowns2)
        {
            if (breakdowns1.Count != breakdowns2.Count) return true;
            if (breakdowns1.Count == 0 && breakdowns2.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < breakdowns1.Count; i++)
            {
                MachineBreakdown item = breakdowns1[i];
                MachineBreakdown? itemInOtherList = breakdowns2.Find(x => x.Id == item.Id);
                if (itemInOtherList == null) return true;
                //if (item.IsUpdated(itemInOtherList)) return true;
            }

            return false;
        }

        private static bool DrawingReferencesChanged(List<OrderDrawing> drawingsReferences1, List<OrderDrawing> drawingsReferences2)
        {
            if (drawingsReferences1.Count != drawingsReferences2.Count) return true;
            if (drawingsReferences1.Count == 0 && drawingsReferences2.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < drawingsReferences1.Count; i++)
            {
                OrderDrawing orderDrawing = drawingsReferences1[i];
                OrderDrawing? orderDrawingInOtherList = drawingsReferences1.Find(x => x.Id == orderDrawing.Id);
                if (orderDrawingInOtherList == null) return true;
                // Drawing refs do not get updated so just need to check they if exist in list or not
            }

            return false;
        }

        private static bool OrderItemsChanged(List<LatheManufactureOrderItem> orderItems1, List<LatheManufactureOrderItem> orderItems2)
        {
            if (orderItems1.Count != orderItems2.Count) return true;
            if (orderItems1.Count == 0 && orderItems2.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < orderItems1.Count; i++)
            {
                LatheManufactureOrderItem item = orderItems1[i];
                LatheManufactureOrderItem? itemInOtherList = orderItems2.Find(x => x.Id == item.Id);
                if (itemInOtherList == null) return true;
                if (item.IsUpdated(itemInOtherList)) return true;
            }

            return false;
        }

        #endregion

        private static bool DetectGeneralOrderChanges(GeneralManufactureOrder generalOrder, GeneralManufactureOrder generalOrderCopy)
        {
            bool updated = generalOrderCopy.IsUpdated(generalOrder);

            if (!updated)
            {
                updated = LotsChanged(newCopy: generalOrder.Lots, originalCopy: generalOrderCopy.Lots);
            }

            return updated;
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
            if (SelectedItem is null) return;
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

            note.Message = note.Message.Trim();
            if(note.Message == note.OriginalMessage)
            {
                return;
            }

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

        private void ItemUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ScheduleItem.AssignedTo))
            {
                SetAssignmentData();
            }
            else if (e.PropertyName == nameof(LatheManufactureOrder.BarID))
            {
                if (SelectedItem is not LatheManufactureOrder order) return;
                order.Bar = barStock.Find(b => b.Id == order.BarID);
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

        public struct Briefing
        {
            public string OrderName { get; set; }
            public bool ArchetypeRunBefore { get; set; }
            public int NumberOfTimesRun { get; set; }
            public bool RunInMaterialBefore { get; set; }
            public DateTime LastRun { get; set; }
        }
    }
}
