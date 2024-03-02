using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel.Commands.Orders;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private bool saveExit;
        public bool SaveExit
        {
            get { return saveExit; }
            set { saveExit = value; OnPropertyChanged(); }
        }

        public List<Note> Notes { get; set; }
        public List<TechnicalDrawing> Drawings { get; set; }
        private List<Lathe> Lathes;

        List<OrderDrawing> DrawingReferences { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public List<BarStock> BarStock { get; set; }

        public List<User> ProductionStaff { get; set; }

        public List<BarIssue> BarIssues { get; set; }

        public OrderSaveEditNoteCommand SaveEditNoteCmd { get; set; }
        public OrderDeleteNoteCommand DeleteNoteCmd { get; set; }

        private ProductGroup archetype;
        private Product? product;
        public BarStock OrderBar { get; set; }

        public List<BreakdownCode> BreakdownCodes { get; set; }
        public List<MachineBreakdown> Breakdowns { get; set; }

        private MachineBreakdown newBreakdown;
        public MachineBreakdown NewBreakdown
        {
            get { return newBreakdown; }
            set 
            { 
                newBreakdown = value; 
                OnPropertyChanged(); 
            }
        }


        private TimeModel? tmpTimeModel;
        public TimeModel? TmpTimeModel
        {
            get { return tmpTimeModel; }
            set { tmpTimeModel = value; }
        }


        public ISeries[] TimeModelSeries { get; set; }
        public Axis[] YAxes { get; set; } =
        {
            new Axis
            {
                MinLimit = 0,
                Name="Time",
                NameTextSize=14,
                TextSize=12
            }
        };

        public RectangularSection[] Sections { get; set; }

        public Axis[] XAxes { get; set; } =
        {
            new Axis
            {
                MinLimit = 0,
                Name="Major Length",
                NameTextSize=14,
                TextSize=12
            }
        };

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

            if (!string.IsNullOrEmpty(Order.TimeCodePlanned))
            {
                TmpTimeModel = new(Order.TimeCodePlanned);
            }

            if (BarStock is null)
            {
                // TODO handle bad inputs
                BarStock = DatabaseHelper.Read<BarStock>().Where(x => !x.IsDormant).ToList();
            }

            BarStock currentBar = BarStock.Find(x => x.Id == Order.BarID)
                ?? throw new Exception($"Cannot find bar {Order.BarID}");

            MaterialInfo material = DatabaseHelper.Read<MaterialInfo>().Find(m => m.Id == currentBar.MaterialId);
            currentBar.MaterialData = material;
            BarStock = BarStock.Where(x => x.MaterialId == currentBar.MaterialId).ToList();
            BarStock.ForEach(b => b.MaterialData = material);

            Order.Bar = currentBar;
            OnPropertyChanged(nameof(BarStock));

            savedOrder = (LatheManufactureOrder)Order.Clone();

            BarIssues = DatabaseHelper.Read<BarIssue>().Where(x => x.OrderId == Order.Name).ToList();
            Order.NumberOfBarsIssued = BarIssues.Sum(x => x.Quantity);
            TotalBarsText.Text = $"{Order.NumberOfBarsIssued}/{Math.Ceiling(Order.NumberOfBars)} Prepared";

            if (Order.StartDate.Date > DateTime.MinValue)
            {
                EndDatePicker.DisplayDateStart = Order.StartDate.Date.AddDays(1);
                EndDatePicker.DisplayDateEnd = Order.StartDate.Date.AddYears(1);
            }


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
            Notes = DatabaseHelper.Read<Note>().Where(x => x.DocumentReference == id).ToList();
            OnPropertyChanged(nameof(Notes));

            DrawingReferences = DatabaseHelper.Read<OrderDrawing>().Where(x => x.OrderId == Order.Name).ToList();
            List<TechnicalDrawing> drawings = DatabaseHelper.Read<TechnicalDrawing>();

            Drawings = new();
            for (int i = 0; i < DrawingReferences.Count; i++)
            {
                Drawings.Add(drawings.Find(x => x.Id == DrawingReferences[i].DrawingId));
            }

            OnPropertyChanged(nameof(Drawings));

            Task.Run(() => ChartTimeModel());

            BreakdownCodes = DatabaseHelper.Read<BreakdownCode>();
            Breakdowns = DatabaseHelper.Read<MachineBreakdown>()
                .Where(x => x.OrderName == Order.Name)
                .OrderBy(x => x.BreakdownStarted)
                .ToList();

            NewBreakdown = new()
            {
                BreakdownStarted = DateTime.Now.AddHours(-1),
                BreakdownEnded = DateTime.Now,
                OrderName = Order.Name,
            };

            archetype = DatabaseHelper.Read<ProductGroup>().Find(x => x.Id == Order.GroupId)
                ?? throw new Exception($"Cannot find archetype {Order.GroupId}");

            product = DatabaseHelper.Read<Product>().Find(x => x.Id == archetype.ProductId);
        }

        private void ChartTimeModel()
        {
            if (TmpTimeModel is null) return;

            TimeModelSeries = null;
            OnPropertyChanged(nameof(TimeModelSeries));

            Sections = null;
            OnPropertyChanged(nameof(Sections));

            ISeries[] series = Array.Empty<ISeries>();
            List<RectangularSection> sections = new();

            ObservableCollection<ObservablePoint> historicalPoints = new();
            ObservableCollection<ObservablePoint> modelledPoints = new();
            ObservableCollection<ObservablePoint> newReadings = new();

            double maxLength = Items.Max(x => x.MajorLength) * 1.1;


            Items.ForEach(x =>
            {
                if (x.CycleTime > 0)
                {
                    newReadings.Add(new(x.MajorLength, x.CycleTime));
                }

                if (x.PreviousCycleTime is not null)
                {
                    historicalPoints.Add(new(x.MajorLength, x.PreviousCycleTime));

                    if (x.CycleTime > 0 && x.CycleTime != x.PreviousCycleTime)
                    {
                        sections.Add(new RectangularSection
                        {
                            Yi = x.PreviousCycleTime,
                            Yj = x.CycleTime,
                            Xi = x.MajorLength,
                            Xj = x.MajorLength,
                            Stroke = new SolidColorPaint
                            {
                                Color = x.CycleTime > x.PreviousCycleTime ? SKColors.DarkRed.WithAlpha(50) : SKColors.DarkGreen.WithAlpha(50),
                                StrokeThickness = 3
                            }
                        });
                    }
                }
                else if (x.ModelledCycleTime is not null)
                {
                    modelledPoints.Add(new(x.MajorLength, x.ModelledCycleTime));
                }
            });

            LineSeries<ObservablePoint> modelSeries = TimeModel.GetSeries(TmpTimeModel, maxLength, "Model");
            modelSeries.Stroke = new SolidColorPaint(SKColors.Gray);
            series = series.Append(modelSeries).ToArray();

            if (newReadings.Count > 0)
            {
                series = series.Append(new ScatterSeries<ObservablePoint>
                {
                    Values = newReadings,
                    Name = "New Readings",
                    GeometrySize = 8,
                    Fill = new SolidColorPaint(SKColors.DarkRed)
                }).ToArray();
            }

            if (modelledPoints.Count > 0)
            {
                series = series.Append(new ScatterSeries<ObservablePoint>
                {
                    Values = modelledPoints,
                    Name = "Modelled",
                    GeometrySize = 8,
                    Fill = new SolidColorPaint(SKColors.MediumPurple)
                }).ToArray();
            }

            if (historicalPoints.Count > 0)
            {
                series = series.Append(new ScatterSeries<ObservablePoint>
                {
                    Values = historicalPoints,
                    GeometrySize = 8,
                    Name = "Historical",
                    Fill = new SolidColorPaint(SKColors.Black)
                }).ToArray();
            }

            Sections = sections.ToArray();
            OnPropertyChanged(nameof(Sections));

            TimeModelSeries = series;
            OnPropertyChanged(nameof(TimeModelSeries));
        }

        public void SaveNoteEdit(Note note)
        {
            try
            {
                note.IsEdited = true;
                note.DateEdited = DateTime.Now.ToString("s");
                DatabaseHelper.Update(note, throwErrs: true);
                SaveExit = true;
            }
            catch
            {
                MessageBox.Show("Failed to update database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Notes = DatabaseHelper.Read<Note>().Where(x => x.DocumentReference == Order.Name).ToList();
            OnPropertyChanged(nameof(Notes));
        }

        public void DeleteNote(Note note)
        {
            try
            {
                note.IsDeleted = true;
                DatabaseHelper.Update(note, throwErrs: true);
                SaveExit = true;
            }
            catch
            {
                MessageBox.Show("Failed to update database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Notes = DatabaseHelper.Read<Note>().Where(x => x.DocumentReference == Order.Name).ToList();
            OnPropertyChanged(nameof(Notes));
        }

        private void UpdateControls(bool canEdit)
        {
            NotesScroller.ScrollToBottom();

            PurchaseOrderTextBox.IsEnabled = App.CurrentUser.HasPermission(PermissionType.UpdateOrder) && canEdit;
            BarStockComboBox.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && !Order.BarIsVerified && canEdit;
            SpareBarsTextBox.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
            researchCheckBox.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
            platingCheckBox.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
            AssignedComboBox.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
            incrementButton.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
            decrementButton.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
            EndDatePicker.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
            clearButton.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
            SettingTimeHrs.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit;
            composeMessageControls.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;

            AddItemButton.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && Order.State < OrderState.Complete && canEdit;
            GetDrawingUpdatesButton.IsEnabled = App.CurrentUser.HasPermission(PermissionType.EditOrder) && canEdit && Order.State < OrderState.Complete;

            SaveButton.IsEnabled = canEdit;

            SetCheckboxEnabling(canEdit);

            TimeTab.IsEnabled = App.CurrentUser.Role >= UserRole.Scheduling && canEdit && TmpTimeModel is not null;

            if (canEdit)
            {
                SaveEditNoteCmd = new(this);
                DeleteNoteCmd = new(this);
            }
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

            List<string> otherUsers = App.NotificationsManager.users
                .Where(x => x.Role >= UserRole.Production && x.ReceivesNotifications)
                .Select(x => x.UserName)
                .ToList();

            toUpdate.AddRange(otherUsers);
            toUpdate = toUpdate.Distinct().Where(x => x != App.CurrentUser.UserName).ToList();

            for (int i = 0; i < toUpdate.Count; i++)
            {
                DatabaseHelper.Insert<Notification>(new(to: toUpdate[i], from: App.CurrentUser.UserName, header: $"Comment - {Order.Name}", body: note[..Math.Min(note.Length, 150)], toastAction: $"viewManufactureOrder:{Order.Name}"));
            }

            DatabaseHelper.Insert(newNote);
            Notes.Add(newNote);
            NotesDisplay.Notes = new List<Note>();
            NotesDisplay.Notes = Notes;
            Message.Text = "";
            SaveExit = true;
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
            if (sender is not CheckBox checkBox) return;
            if (checkBox.Name == "Complete_Checkbox" && (checkBox.IsChecked ?? false))
            {
                if (Items.Any(x => x.QuantityDelivered < x.RequiredQuantity))
                {
                    MessageBox.Show(
                        "The customer requirement has not been delivered - please ensure we have enough to cover.",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Hand);
                }
            }

            SetCheckboxEnabling(CanEdit);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!CanEdit)
            {
                return;
            }

            if (TmpTimeModel is not null)
            {
                if (Order.TimeCodePlanned != TmpTimeModel.ToString())
                {
                    // reset if not saved
                    //Order.TimeModelPlanned = new(originalTimeCode);
                }
            }

            CalculateTimeAndBar();

            List<string> features = Order.Bar.MaterialData.RequiresFeaturesList;
            features.AddRange(Order.Bar.RequiresFeaturesList);
            features.AddRange(Order.Bar.MaterialData.RequiresFeaturesList);
            features.AddRange(archetype.RequiresFeaturesList);
            if (product is not null)
            {
                features.AddRange(product.RequiresFeaturesList);
            }

            features = features.OrderBy(x => x).Distinct().ToList();

            Order.RequiredFeaturesList = features;

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
            if (savedOrder.AssignedTo != Order.AssignedTo)
            {
                NotifyAssignmentChanged(savedOrder.AssignedTo, Order.AssignedTo);
            }

            Order.ModifiedBy = App.CurrentUser.GetFullName();

            DateTime modificationDate = DateTime.Now;
            Order.ModifiedAt = modificationDate;

            if (savedOrder.State < OrderState.Complete && Order.State >= OrderState.Complete)
            {
                Order.CompletedAt = modificationDate;
            }

            if (!DatabaseHelper.Update(Order))
            {
                MessageBox.Show("Failed to update the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void NotifyAssignmentChanged(string? from, string? to)
        {
            if (from is not null)
            {
                App.NotificationsManager.NotifyOrderAssignment(Order, from, unassigned: true);
            }

            if (to is not null)
            {
                App.NotificationsManager.NotifyOrderAssignment(Order, to, unassigned: false);
            }
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

            if (item.RequiredQuantity > 0)
            {
                MessageBox.Show("Item has a customer requirement and cannot be deleted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Items.Count == 1)
            {
                MessageBox.Show("Order requires at least one item listed. Alternatively you can cancel the order.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult userChoice = MessageBox.Show($"Are you sure you want to delete {item.ProductName} from {Order.Name}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (userChoice != MessageBoxResult.Yes)
            {
                return;
            }

            bool deleted = DatabaseHelper.Delete(item);

            if (!deleted)
            {
                MessageBox.Show("Failed to delete record from database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CalculateTimeAndBar();
            SaveExit = true;
            SaveOrder();
            LoadData(Order.Name);
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
            Order.TimeToComplete = OrderResourceHelper.CalculateOrderRuntime(Order, Items, Breakdowns);

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
            EditLMOItemWindow editWindow;

            try
            {
                editWindow = new((int)_toEdit, CanEdit, allowDelivery: !Order.IsResearch);
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
                return;
            }

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
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
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

            int[] currentDrawingIds = DrawingReferences.Select(x => x.DrawingId).OrderBy(x => x).ToArray();
            int[] upToDateDrawingIds = drawings.Select(x => x.Id).OrderBy(x => x).ToArray();

            if (currentDrawingIds.Length == upToDateDrawingIds.Length)
            {
                bool diff = false;
                for (int i = 0; i < currentDrawingIds.Length; i++)
                {
                    if (currentDrawingIds[i] != upToDateDrawingIds[i])
                    {
                        diff = true;
                        break;
                    }
                }

                if (!diff)
                {
                    MessageBox.Show("No drawing updates have been found.", "Up to date", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
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
            SaveExit = true;
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

            LabelPrintingHelper.PrintIssue((BarIssue)BarIssuesListBox.SelectedValue, copies: 2);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;

            gradTop.Visibility = scrollViewer.VerticalOffset == 0
                ? Visibility.Hidden
                : Visibility.Visible;

            gradBottom.Visibility = scrollViewer.VerticalOffset + 1 > scrollViewer.ScrollableHeight
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void platingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Drawings.ForEach(x => x.PlatingStatement = true);
        }

        private void platingCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Drawings.ForEach(x => x.PlatingStatement = false);
        }

        private void TimeModel_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChartTimeModel();
        }

        private void CancelModelChanges_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Order.TimeCodePlanned))
            {
                TmpTimeModel = new(Order.TimeCodePlanned);
                ChartTimeModel();
            }
        }

        private void UpdateModelChanges_Click(object sender, RoutedEventArgs e)
        {
            if (TmpTimeModel is null) return;
            Order.TimeModelPlanned = TmpTimeModel;
            foreach (LatheManufactureOrderItem item in Items)
            {
                if (item.ModelledCycleTime is null)
                {
                    continue;
                }

                item.ModelledCycleTime = Order.TimeModelPlanned.At(item.MajorLength);
                DatabaseHelper.Update(item);
            }
            Items = new(Items);
            OnPropertyChanged(nameof(Items));

            CalculateTimeAndBar();

            ChartTimeModel();
        }

        private void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Order is null) return;

            if (EndDatePicker.SelectedDate is not null)
            {
                DateTime end = (DateTime)EndDatePicker.SelectedDate;
                end = end.ChangeTime(6, 0, 0, 0);

                Order.ScheduledEnd = end;
            }
            else
            {
                Order.ScheduledEnd = null;
            }
        }

        private void IncrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (Order.ScheduledEnd is null) return;
            DateTime dt = (DateTime)Order.ScheduledEnd;
            int hourCurrent = dt.Hour;
            hourCurrent++;
            hourCurrent %= 24;
            Order.ScheduledEnd = dt.ChangeTime(hourCurrent, 0, 0, 0);
        }

        private void DecrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (Order.ScheduledEnd is null) return;
            DateTime dt = (DateTime)Order.ScheduledEnd;
            int hourCurrent = dt.Hour;
            hourCurrent--;
            if (hourCurrent < 0)
            {
                hourCurrent = 23;
            }

            Order.ScheduledEnd = dt.ChangeTime(hourCurrent, 0, 0, 0);
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            Order.ScheduledEnd = null;
        }

        private void AddBreakdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (!App.CurrentUser.HasPermission(PermissionType.EditOrder))
            {
                MessageBox.Show("You do not have permissions to Edit Order", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewBreakdown.ValidateAll();
            if (NewBreakdown.HasErrors)
            {
                MessageBox.Show("New record has errors", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (NewBreakdown.BreakdownEnded > (Order.ScheduledEnd ?? Order.EndsAt()))
            {
                MessageBox.Show("Breakdown cannot end after order has finished", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (NewBreakdown.BreakdownStarted < Order.StartDate)
            {
                MessageBox.Show("Breakdown cannot begin before order has started", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



            NewBreakdown.CreatedAt = DateTime.Now;
            NewBreakdown.CreatedBy = App.CurrentUser.UserName;

            try
            {
                DatabaseHelper.Insert(NewBreakdown, throwErrs: true);
            }
            catch
            {
                MessageBox.Show("Failed to insert to database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewBreakdown.BreakdownMeta = BreakdownCodes.Find(x => x.Id == NewBreakdown.BreakdownCode);
            Breakdowns = null;
            Breakdowns = DatabaseHelper.Read<MachineBreakdown>()
                .Where(x => x.OrderName == Order.Name)
                .OrderBy(x => x.BreakdownStarted)
                .ToList();
            OnPropertyChanged(nameof(Breakdowns));
            NewBreakdown = new()
            {
                BreakdownStarted = DateTime.Now.AddHours(-1),
                BreakdownEnded = DateTime.Now,
                OrderName = Order.Name
            };

            SaveExit = true;
        }

        private void removeBreakdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not int breakdownId) return;

            if (!App.CurrentUser.HasPermission(PermissionType.EditOrder))
            {
                MessageBox.Show("You do not have permissions to Edit Order", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MachineBreakdown? breakdown = Breakdowns.Find(x => x.Id == breakdownId);
            if (breakdown == null)
            {
                MessageBox.Show($"Cannot find breakdown with ID {breakdownId}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this breakdown record?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (!DatabaseHelper.Delete(breakdown))
            {
                MessageBox.Show($"Failed to delete breakdown record", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Breakdowns = Breakdowns.Where(x => x.Id != breakdown.Id).ToList();
            OnPropertyChanged(nameof(Breakdowns));
            MessageBox.Show($"Deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
