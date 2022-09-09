using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.View.LatheOrderViews;
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

namespace ProjectLighthouse.View
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

        List<OrderDrawing> DrawingReferences { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<BarStock> BarStock { get; set; }

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

            Order = DatabaseHelper.Read<LatheManufactureOrder>().Find(x => x.Name == id);

            if (BarStock == null)
            {
                BarStock = DatabaseHelper.Read<BarStock>();
                BarStock currentBar = BarStock.Find(x => x.Id == Order.BarID);
                BarStock = BarStock.Where(x => x.Material == currentBar.Material).ToList();
                OnPropertyChanged(nameof(BarStock));
            }
            savedOrder = (LatheManufactureOrder)Order.Clone();

            Items = DatabaseHelper.Read<LatheManufactureOrderItem>()
                .Where(x => x.AssignedMO == id)
                .OrderByDescending(n => n.RequiredQuantity)
                .ThenBy(n => n.ProductName)
                .ToList();

            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].ShowEdit = App.CurrentUser.CanUpdateLMOs && CanEdit;
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

            PurchaseOrderTextBox.IsEnabled = App.CurrentUser.CanUpdateLMOs && canEdit;
            BarStockComboBox.IsEnabled = App.CurrentUser.Role >= UserRole.Scheduling && !Order.BarIsVerified && canEdit;
            SpareBarsTextBox.IsEnabled = App.CurrentUser.Role >= UserRole.Scheduling && canEdit;
            researchCheckBox.IsEnabled = App.CurrentUser.Role >= UserRole.Scheduling && canEdit;
            composeMessageControls.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;

            AddItemButton.IsEnabled = App.CurrentUser.Role >= UserRole.Scheduling && Order.State < OrderState.Complete && canEdit && !Order.BarIsAllocated;
            GetDrawingUpdatesButton.IsEnabled = App.CurrentUser.Role >= UserRole.Scheduling && canEdit && Order.State < OrderState.Complete;

            SaveButton.IsEnabled = canEdit;

            SetCheckboxEnabling(canEdit);
        }

        private void SetCheckboxEnabling(bool canEdit)
        {

            //TODO
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
                    tier2 = Order.BarIsVerified;
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

            if (!App.CurrentUser.CanUpdateLMOs || !canEdit)
            {
                tier1 = false;
                tier2 = false;
                tier3 = false;
                tier4 = false;
            }

            Tooling_Checkbox.IsEnabled = tier1;
            BarVerified_Checkbox.IsEnabled = tier1;


            Program_Checkbox.IsEnabled = tier2;

            Running_Checkbox.IsEnabled = tier3;

            Complete_Checkbox.IsEnabled = tier4 && !Order.IsCancelled;
            Cancelled_Checkbox.IsEnabled = App.CurrentUser.Role >= UserRole.Scheduling && canEdit;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CheckIfOrderIsClosed()
        {
            if (Order.State > OrderState.Running)
            {
                List<LatheManufactureOrderItem> itemsWithBadCycleTimes = Items.Where(i => i.CycleTime == 0 && i.QuantityMade > 0).ToList();
                List<Lot> unresolvedLots = Lots.Where(l => l.Quantity != 0 && !l.IsDelivered && !l.IsReject && l.AllowDelivery).ToList();

                Order.IsClosed = itemsWithBadCycleTimes.Count == 0 // ensure cycle time is updated
                    && unresolvedLots.Count == 0; // ensure lots are fully processed
            }
            else
            {
                Order.IsClosed = false;
            }
        }

        private void DisplayLMOItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DisplayLMOItems control = sender as DisplayLMOItems;
            if (App.CurrentUser.CanUpdateLMOs && CanEdit)
            {
                SaveCommand.Execute(control.LatheManufactureOrderItem.Id);
            }
        }


        #region Helpers
        private void CalculateTime()
        {
            int estimatedTimeSeconds = 0;
            int cycleTime = 0;
            foreach (LatheManufactureOrderItem item in Items.OrderByDescending(x => x.CycleTime))
            {
                if (item.CycleTime == 0 && cycleTime > 0) // uses better cycle time if available
                {
                    estimatedTimeSeconds += item.GetCycleTime() * item.TargetQuantity;
                }
                else
                {
                    estimatedTimeSeconds += item.GetCycleTime() * item.TargetQuantity;
                }
            }

            Order.TimeToComplete = estimatedTimeSeconds;
        }

        private void CalculateBarRequirements()
        {
            double totalLengthRequired = 0;
            double partOff = 2;
            BarStock bar = (BarStock)BarStockComboBox.SelectedValue;

            if (!string.IsNullOrEmpty(Order.AllocatedMachine))
            {
                List<Lathe> lathes = DatabaseHelper.Read<Lathe>();
                Lathe runningOnLathe = lathes.First(l => l.Id == Order.AllocatedMachine);
                partOff = runningOnLathe.PartOff;
            }

            foreach (LatheManufactureOrderItem item in Items)
            {
                totalLengthRequired += (item.MajorLength + item.PartOffLength + partOff) * item.TargetQuantity * 1.02;
            }

            Order.NumberOfBars = Math.Ceiling(totalLengthRequired / (bar.Length - 300)) + Order.SpareBars;
        }

        #endregion

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

        public void AddNewNote(string note) // Factor in edits
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
                otherUsers = DatabaseHelper.Read<User>().Where(x => x.Role == UserRole.Production && x.ReceivesNotifications).Select(x => x.UserName).ToList();
            }
            else
            {
                otherUsers = DatabaseHelper.Read<User>().Where(x => x.Role >= UserRole.Scheduling && x.ReceivesNotifications).Select(x => x.UserName).ToList();
            }

            toUpdate.AddRange(otherUsers);
            toUpdate = toUpdate.Distinct().Where(x => x != App.CurrentUser.UserName).ToList();

            for (int i = 0; i < toUpdate.Count; i++)
            {
                DatabaseHelper.Insert<Notification>(new(to: toUpdate[i], from: App.CurrentUser.UserName, header: $"Comment - {Order.Name}", body: "New comment left on this order", toastAction: $"viewManufactureOrder:{Order.Name}"));
            }

            DatabaseHelper.Insert(newNote);
            Notes.Add(newNote);
            NotesDisplay.ItemsSource = new List<Note>();
            NotesDisplay.ItemsSource = Notes;
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
            CalculateTime();
            CalculateBarRequirements();
            CheckIfOrderIsClosed(); // running to closed is broken

            if (savedOrder.IsUpdated(Order))
            {
                SaveExit = true;
            }

            if (SaveExit)
            {
                SaveOrder();
            }
        }

        void SaveOrder()
        {
            Order.ModifiedBy = App.CurrentUser.GetFullName();
            Order.ModifiedAt = DateTime.Now;

            Order.Status = Order.State.ToString();

            if (savedOrder.State < OrderState.Complete && Order.State >= OrderState.Complete)
            {
                Order.CompletedAt = Order.ModifiedAt;
            }

            _ = DatabaseHelper.Update(Order);
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
                CalculateBarRequirements();
                CalculateTime();
                SaveExit = true;
                SaveOrder();
                LoadData(Order.Name);
            }
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            AddItemToOrderWindow window = new(Order.Name);
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
                CalculateBarRequirements();
                CalculateTime();
                SaveOrder();
                LoadData(Order.Name);
            }
            Show();
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
            if (_toEdit == null)
            {
                MessageBox.Show("no edit");// Save command execution logic
            }
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
            List<TechnicalDrawing> drawings = TechnicalDrawing.FindDrawings(allDrawings, Items, Order.ToolingGroup);

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
            OnPropertyChanged(nameof(Drawings));
        }

        private void PurchaseOrderTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateAlphanumeric(e);
        }
    }
}
