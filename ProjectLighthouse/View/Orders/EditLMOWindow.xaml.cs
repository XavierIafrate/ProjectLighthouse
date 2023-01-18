using DocumentFormat.OpenXml.Spreadsheet;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViewModel.Helpers;

namespace ProjectLighthouse.View.Orders
{
    public partial class EditLMOWindow : Window, INotifyPropertyChanged
    {
        #region Variables

        private LatheManufactureOrder _order;
        public LatheManufactureOrder Order
        {
            get { return _order; }
            set
            {
                _order = value;
                SetCheckboxEnabling(CanEdit);
                OnPropertyChanged();
            }
        }

        public LatheManufactureOrder savedOrder;

        public bool CanEdit { get; set; }

        public List<LatheManufactureOrderItem> Items { get; set; }
        public List<Lot> Lots;
        public bool SaveExit { get; set; }
        public List<Note> Notes { get; set; }
        public List<TechnicalDrawing> Drawings { get; set; }
        private List<Lathe> Lathes;

        List<OrderDrawing> DrawingReferences { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<BarStock> BarStock { get; set; }

        public List<User> ProductionStaff { get; set; }

        public List<BarIssue> BarIssues { get; set; }

        #endregion

        public EditLMOWindow(string id, bool canEdit)
        {
            InitializeComponent();

            CanEdit = canEdit;
            LoadData(id);

            DataContext = this;
            UpdateControls(canEdit);
        }

        private void LoadData(string id)
        {
            ProductionStaff = DatabaseHelper.Read<User>()
                .Where(u => u.Role == UserRole.Production)
                .OrderBy(x => x.UserName)
                .Prepend(new() { UserName = null, FirstName = "Unassigned" })
                .ToList();

            Order = DatabaseHelper.Read<LatheManufactureOrder>().Find(x => x.Name == id);
            Lathes = DatabaseHelper.Read<Lathe>();

            if (Order is null) throw new Exception($"Cannot find order {id}");

            if (BarStock is null)
            {
                // TODO handle bad inputs
                BarStock = DatabaseHelper.Read<BarStock>();
                BarStock currentBar = BarStock.Find(x => x.Id == Order.BarID);

                if (currentBar is null) throw new Exception($"Cannot find bar {Order.BarID}");

                BarStock = BarStock.Where(x => x.MaterialId == currentBar.MaterialId).ToList();
                OnPropertyChanged(nameof(BarStock));
            }

            savedOrder = (LatheManufactureOrder)Order.Clone();

            BarIssues = DatabaseHelper.Read<BarIssue>().Where(x => x.OrderId == Order.Name).ToList();
            Order.NumberOfBarsIssued = BarIssues.Sum(x => x.Quantity);
            TotalBarsText.Text = $"{Order.NumberOfBarsIssued}/{Math.Ceiling(Order.NumberOfBars)} Prepared";


            Items = DatabaseHelper.Read<LatheManufactureOrderItem>()
                .Where(x => x.AssignedMO == id)
                .OrderByDescending(n => n.RequiredQuantity)
                .ThenBy(n => n.ProductName)
                .ToList();

            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].ShowEdit = App.CurrentUser.HasPermission(PermissionType.UpdateOrder);
            }
            OnPropertyChanged(nameof(Items));


            Lots = DatabaseHelper.Read<Lot>().Where(x => x.Order == id).ToList();
            Notes = DatabaseHelper.Read<Note>().Where(x => x.DocumentReference == id && !x.IsDeleted).ToList();
            OnPropertyChanged(nameof(Notes));
            FormatNoteDisplay();

            DrawingReferences = DatabaseHelper.Read<OrderDrawing>().Where(x => x.OrderId == Order.Name).ToList();
            List<TechnicalDrawing> drawings = DatabaseHelper.Read<TechnicalDrawing>();
            Drawings = new();
            for (int i = 0; i < DrawingReferences.Count; i++)
            {
                Drawings.Add(drawings.Find(x => x.Id == DrawingReferences[i].DrawingId));
            }

            OnPropertyChanged(nameof(Drawings));
        }

        

        private void FormatNoteDisplay()
        {
            string name = "";
            DateTime lastTimeStamp = DateTime.MinValue;

            for (int i = 0; i < Notes.Count; i++)
            {
                Notes[i].ShowEdit = Notes[i].SentBy == App.CurrentUser.UserName;
                Notes[i].ShowHeader = Notes[i].SentBy != name
                    || DateTime.Parse(Notes[i].DateSent) > lastTimeStamp.AddHours(6);
                lastTimeStamp = DateTime.Parse(Notes[i].DateSent);
                name = Notes[i].SentBy;
            }
        }

        private void UpdateControls(bool canEdit)
        {
            NotesScroller.ScrollToBottom();

            PurchaseOrderTextBox.IsEnabled = App.CurrentUser.HasPermission(PermissionType.UpdateOrder) && canEdit;
            BarStockComboBox.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && !Order.BarIsVerified && canEdit;
            SpareBarsTextBox.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
            researchCheckBox.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
            AssignedComboBox.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
            composeMessageControls.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;

            AddItemButton.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && Order.State < OrderState.Complete && canEdit;
            GetDrawingUpdatesButton.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit && Order.State < OrderState.Complete;

            SaveButton.IsEnabled = canEdit;

            SetCheckboxEnabling(canEdit);
        }

        private void SetCheckboxEnabling(bool canEdit)
        {
            bool tier1;
            bool tier2;
            bool tier3;
            bool tier4;

            switch (Order.State)
            {
                case >= OrderState.Complete:
                    tier1 = false;
                    tier2 = false;
                    tier3 = false;
                    tier4 = true;
                    break;
                case OrderState.Running:
                    tier1 = false;
                    tier2 = false;
                    tier3 = true;
                    tier4 = true;
                    break;
                case OrderState.Prepared:
                    tier1 = false;
                    tier2 = true;
                    tier3 = Order.StartDate.Date <= DateTime.Today;
                    tier4 = false;
                    break;
                case OrderState.Problem or OrderState.Ready:
                    tier1 = true;
                    tier2 = true;
                    tier3 = false;
                    tier4 = false;
                    break;
                default:
                    tier1 = false;
                    tier2 = false;
                    tier3 = false;
                    tier4 = false;
                    break;
            }

            if (!App.CurrentUser.HasPermission(PermissionType.UpdateOrder) || !canEdit)
            {
                tier1 = false;
                tier2 = false;
                tier3 = false;
                tier4 = false;
            }

            if (Order.AllToolingReady && Order.HasProgram && Order.NumberOfBarsIssued > 0 && Order.State == OrderState.Problem)
            {
                // Not all bar arrived
                tier1 = false;
                tier2 = true;
                tier3 = Order.StartDate.Date <= DateTime.Today; ;
                tier4 = false;
            }

            ToolingOrdered_Checkbox.IsEnabled = tier1;
            BarToolingOrdered_Checkbox.IsEnabled = tier1;
            GaugesOrdered_Checkbox.IsEnabled = tier1;
            BaseProgram_Checkbox.IsEnabled = tier1;
            BarVerified_Checkbox.IsEnabled = tier1;


            ToolingArrived_Checkbox.IsEnabled = tier2;
            BarToolingArrived_Checkbox.IsEnabled = tier2;
            GaugesArrived_Checkbox.IsEnabled = tier2;
            Program_Checkbox.IsEnabled = tier2;

            Running_Checkbox.IsEnabled = tier3;

            Complete_Checkbox.IsEnabled = tier4 && !Order.IsCancelled;
            Cancelled_Checkbox.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DisplayLMOItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DisplayLMOItems control) return;

            if (App.CurrentUser.HasPermission(PermissionType.UpdateOrder))
            {
                SaveCommand.Execute(control.LatheManufactureOrderItem.Id);
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Message.Text.Trim()))
            {
                AddNewNote(Message.Text.Trim());
            }
        }

        private void Message_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComposeGhost.Visibility = string.IsNullOrEmpty(Message.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        public void AddNewNote(string note)
        {
            Note newNote = new()
            {
                Message = note,
                OriginalMessage = note,
                IsEdited = false,
                IsDeleted = false,
                SentBy = App.CurrentUser.UserName,
                DateSent = DateTime.Now.ToString("s"),
                DateEdited = DateTime.MinValue.ToString("s"),
                DocumentReference = Order.Name
            };

            // people who have already commented
            List<string> toUpdate = Notes.Select(x => x.SentBy).Distinct().ToList();

            List<string> otherUsers;
            if (App.CurrentUser.Role >= UserRole.Scheduling)
            {
                otherUsers = App.NotificationsManager.users.Where(x => x.Role == UserRole.Production && x.ReceivesNotifications).Select(x => x.UserName).ToList();
            }
            else
            {
                otherUsers = App.NotificationsManager.users.Where(x => x.Role >= UserRole.Scheduling && x.ReceivesNotifications).Select(x => x.UserName).ToList();
            }

            toUpdate.AddRange(otherUsers);
            toUpdate = toUpdate.Distinct().Where(x => x != App.CurrentUser.UserName).ToList();

            for (int i = 0; i < toUpdate.Count; i++)
            {
                DatabaseHelper.Insert<Notification>(new(to: toUpdate[i], from: App.CurrentUser.UserName, header: $"Comment - {Order.Name}", body: "New comment left on this order", toastAction: $"viewManufactureOrder:{Order.Name}"));
            }

            DatabaseHelper.Insert(newNote);
            Notes.Add(newNote);
            NotesDisplay.Notes = new List<Note>();
            NotesDisplay.Notes = Notes;
            Message.Text = "";
            SaveExit = true;

            FormatNoteDisplay();
        }

        private void Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Message.Text) && e.Key == Key.Enter)
            {
                AddNewNote(Message.Text.Trim());
            }
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            SetCheckboxEnabling(CanEdit);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!CanEdit)
            {
                return;
            }
            
            CalculateTimeAndBar();

            if (savedOrder.IsUpdated(Order))
            {
                SaveExit = true;
            }

            if (SaveExit && CanEdit)
            {
                _ = SaveOrder();
            }
        }

        private bool SaveOrder()
        {
            Order.ModifiedBy = App.CurrentUser.GetFullName();
            Order.ModifiedAt = DateTime.Now;

            Order.Status = Order.State.ToString();

            if (savedOrder.State < OrderState.Complete && Order.State >= OrderState.Complete)
            {
                Order.CompletedAt = Order.ModifiedAt;
            }

            if (!DatabaseHelper.Update(Order))
            {
                MessageBox.Show("Failed to update the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void SpareBarsTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListView list)
            {
                return;
            }

            if (list.SelectedValue is LatheManufactureOrderItem selectedItem)
            {
                RemoveItemButton.IsEnabled = App.CurrentUser.Role >= UserRole.Scheduling
                    && CanEdit
                    && Order.State < OrderState.Complete
                    && selectedItem.QuantityMade == 0;
            }
            else
            {
                RemoveItemButton.IsEnabled = false;
            }
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            LatheManufactureOrderItem item = (LatheManufactureOrderItem)ItemsListBox.SelectedValue;
            MessageBoxResult userChoice = MessageBox.Show($"Are you sure you want to delete {item.ProductName} from {Order.Name}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (userChoice == MessageBoxResult.Yes)
            {
                DatabaseHelper.Delete(item);
                Order.NumberOfBars = Items.CalculateNumberOfBars((BarStock)BarStockComboBox.SelectedValue, Order.SpareBars);
                (Order.TimeToComplete, _, _) = Items.CalculateOrderRuntime();
                SaveExit = true;
                SaveOrder();
                LoadData(Order.Name);
            }
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            AddItemToOrderWindow window = new(Order.Id);
            if (window.PossibleItems.Count == 0)
            {
                MessageBox.Show("No further items are available to run on this order.", "Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Hide();
            window.ShowDialog();

            if (window.ItemsWereAdded)
            {
                SaveExit = true;
                CalculateTimeAndBar();
                SaveOrder();
                LoadData(Order.Name);
            }

            Show();
        }

        private void CalculateTimeAndBar()
        {
            (Order.TimeToComplete, _, _) = Items.CalculateOrderRuntime();

            double? partOff = null;
            if (!string.IsNullOrEmpty(Order.AllocatedMachine))
            {
                Lathe? runningOnLathe = Lathes.Find(l => l.Id == Order.AllocatedMachine);
                if (runningOnLathe is not null)
                {
                    partOff = runningOnLathe.PartOff;
                }
            }

            if (partOff is not null)
            {
                Order.NumberOfBars = Items.CalculateNumberOfBars((BarStock)BarStockComboBox.SelectedValue, Order.SpareBars, (double)partOff);
            }
            else
            {
                Order.NumberOfBars = Items.CalculateNumberOfBars((BarStock)BarStockComboBox.SelectedValue, Order.SpareBars);
            }
        }


        private ICommand _saveCommand;
        public static int? _toEdit = null;
        public ICommand SaveCommand
        {
            get
            {
                _saveCommand ??= new RelayCommand(
                        param => this.SaveObject(),
                        param => this.CanSave()
                    );
                return _saveCommand;
            }
        }

        private bool CanSave()
        {
            return true;// Verify command can be executed here
        }

        private void SaveObject()
        {
            if (_toEdit is not int) return;

            EditLMOItemWindow editWindow = new((int)_toEdit, CanEdit, allowDelivery: !Order.IsResearch);
            Hide();
            editWindow.ShowDialog();

            if (editWindow.SaveExit)
            {
                SaveExit = true;
                LoadData(Order.Name);
            }
            ShowDialog();
        }

        public class RelayCommand : ICommand
        {
            #region Fields

            readonly Action<object> _execute;
            readonly Predicate<object> _canExecute;

            #endregion

            #region Constructors

            public RelayCommand(Action<object> execute)
                : this(execute, null)
            {
            }

            public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            {
                _execute = execute ?? throw new ArgumentNullException("execute");
                _canExecute = canExecute;
            }

            #endregion

            #region ICommand Members

            [DebuggerStepThrough]
            public bool CanExecute(object parameters)
            {
                return _canExecute == null || _canExecute(parameters);
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameters)
            {
                if (parameters is not int id)
                {
                    return;
                }
                _toEdit = id;
                _execute(parameters);
            }

            #endregion
        }

        private void GetDrawingUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            List<TechnicalDrawing> allDrawings = DatabaseHelper.Read<TechnicalDrawing>().Where(x => x.DrawingType == (Order.IsResearch ? TechnicalDrawing.Type.Research : TechnicalDrawing.Type.Production)).ToList();
            List<TechnicalDrawing> drawings = TechnicalDrawing.FindDrawings(allDrawings, Items, Order.GroupId, Order.MaterialId);

            int[] currentDrawingIds = DrawingReferences.Select(x => x.DrawingId).ToArray();
            int[] upToDateDrawingIds = drawings.Select(x => x.Id).ToArray();

            if (currentDrawingIds.OrderBy(x => x) == upToDateDrawingIds.OrderBy(x => x))
            {
                MessageBox.Show("No drawing updates have been found.", "Up to date", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (currentDrawingIds.Length == 0 && upToDateDrawingIds.Length == 0)
            {
                MessageBox.Show("No drawing updates have been found.", "Up to date", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            for (int i = 0; i < currentDrawingIds.Length; i++)
            {
                if (!upToDateDrawingIds.Contains(currentDrawingIds[i]))
                {
                    DatabaseHelper.Delete(DrawingReferences.Find(x => x.DrawingId == currentDrawingIds[i]));
                    Drawings.Remove(Drawings.Find(x => x.Id == currentDrawingIds[i]));
                    DrawingReferences.Remove(DrawingReferences.Find(x => x.DrawingId == currentDrawingIds[i]));
                }
            }


            for (int i = 0; i < upToDateDrawingIds.Length; i++)
            {
                if (!currentDrawingIds.Contains(upToDateDrawingIds[i]))
                {
                    OrderDrawing newRecord = new() { DrawingId = upToDateDrawingIds[i], OrderId = Order.Name };
                    DatabaseHelper.Insert<OrderDrawing>(newRecord);
                    DrawingReferences.Add(newRecord);
                    Drawings.Add(drawings.Find(x => x.Id == upToDateDrawingIds[i]));
                }
            }

            MessageBox.Show("Updated drawings were found and the order records amended.", "Now up to date", MessageBoxButton.OK, MessageBoxImage.Information);
            Drawings = new(Drawings);
            OnPropertyChanged(nameof(Drawings));
        }

        private void PurchaseOrderTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateAlphanumeric(e);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox) return;
            if (listBox.SelectedValue is not BarIssue)
            {
                PrintIssueLabelButton.IsEnabled = false;
                return;
            }
            PrintIssueLabelButton.IsEnabled = true;

        }

        private void PrintIssueLabelButton_Click(object sender, RoutedEventArgs e)
        {
            if (BarIssuesListBox.SelectedValue == null) return;

            LabelPrintingHelper.PrintIssue((BarIssue)BarIssuesListBox.SelectedValue, copies:2);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
