using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectLighthouse.View.Orders
{
    public partial class EditLMOItemWindow : Window
    {
        public LatheManufactureOrderItem Item { get; set; }
        public List<Lot> Lots { get; set; }
        public bool CanEdit { get; set; }
        public bool AllowDelivery { get; set; }
        private string ProducedOnMachine { get; set; }

        public bool SaveExit;
        private bool LotAdded;

        public EditLMOItemWindow(int ItemId, bool canEdit, bool allowDelivery = true)
        {
            InitializeComponent();
            SaveExit = false;
            AllowDelivery = allowDelivery;
            CanEdit = canEdit;

            LoadData(ItemId);

            //ProducedOnMachine = MachineID;
            DataContext = this;

            LoadUI();
        }

        private void LoadData(int id)
        {
            Item = DatabaseHelper.Read<LatheManufactureOrderItem>().Find(x => x.Id == id);
            Lots = DatabaseHelper.Read<Lot>().Where(x => x.ProductName == Item.ProductName && x.Order == Item.AssignedMO).ToList();

            Item.QuantityMade = Lots.Where(x => !x.IsReject).Sum(x => x.Quantity);
            Item.QuantityReject = Lots.Where(x => x.IsReject).Sum(x => x.Quantity);
            Item.QuantityDelivered = Lots.Where(x => x.IsDelivered).Sum(x => x.Quantity);
            DatabaseHelper.Update(Item);
        }

        private void LoadUI()
        {
            //QuantityRequiredTextbox.Text = Item.RequiredQuantity.ToString();
            //QuantityTargetTextbox.Text = Item.TargetQuantity.ToString();
            //DateRequiredPicker.SelectedDate = Item.DateRequired;

            SchedulingGrid.Visibility = App.CurrentUser.Role >= UserRole.Scheduling
                ? Visibility.Visible
                : Visibility.Collapsed;

            LotsListBox.ItemsSource = null;
            LotsListBox.ItemsSource = Lots.Where(l => l.ProductName == Item.ProductName).ToList();

            if (Lots.Count > 0)
            {
                BatchTextBox.Text = Lots.Last().MaterialBatch;
            }

            PopulateCycleTimes();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            #region Check numerical inputs
            if (!int.TryParse(CycleTime_Min.Text, out int min))
            {
                MessageBox.Show("Invalid cycle time entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(CycleTime_Sec.Text, out int sec))
            {
                MessageBox.Show("Invalid cycle time entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(QuantityRequiredTextbox.Text, out int reqqty))
            {
                MessageBox.Show("Invalid Quantity Required entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(QuantityTargetTextbox.Text, out int tarqty))
            {
                MessageBox.Show("Invalid Quantity Target entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int cycleTime = min * 60 + sec;
            Item.CycleTime = cycleTime;
            #endregion

            Item.RequiredQuantity = reqqty;
            Item.TargetQuantity = tarqty;
            Item.DateRequired = (DateTime)DateRequiredPicker.SelectedDate;
            Item.UpdatedAt = DateTime.Now;
            Item.UpdatedBy = App.CurrentUser.GetFullName();

            DatabaseHelper.Update(Item);

            TurnedProduct product = DatabaseHelper.Read<TurnedProduct>().Find(n => n.ProductName == Item.ProductName);
            if (product == null)
            {
                MessageBox.Show("Failed to update product record.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            product.CycleTime = cycleTime;
            if (LotAdded)
            {
                product.lastManufactured = DateTime.Now;
            }

            DatabaseHelper.Update(product);
            SaveExit = true;
            Close();
        }

        private void PopulateCycleTimes()
        {
            intCycleTimeText.Text = $"({Item.CycleTime}s)";
            int secs = Item.CycleTime % 60;
            int mins = (Item.CycleTime - secs) / 60;

            CycleTime_Min.Text = mins.ToString();
            CycleTime_Sec.Text = secs.ToString();
        }

        private void CalculateCycleTime()
        {
            BrushConverter bc = new();

            if (CycleTime_Min == null || CycleTime_Sec == null)
            {
                return;
            }

            if (int.TryParse(CycleTime_Min.Text, out int min))
            {
                CycleTime_Min.BorderBrush = (Brush)bc.ConvertFrom("#FFABADB3");
            }
            else
            {
                CycleTime_Min.BorderBrush = Brushes.Red;
                intCycleTimeText.Text = "(?s)";
                return;
            }

            if (int.TryParse(CycleTime_Sec.Text, out int sec))
            {
                CycleTime_Sec.BorderBrush = (Brush)bc.ConvertFrom("#FFABADB3");
            }
            else
            {
                CycleTime_Sec.BorderBrush = Brushes.Red;
                intCycleTimeText.Text = "(?s)";
                return;
            }

            int cycleTime = min * 60 + sec;
            intCycleTimeText.Text = $"({cycleTime}s)";

        }

        private void CycleTime_Min_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateCycleTime();
        }

        private void CycleTime_Sec_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateCycleTime();
        }

        private void Allow_Nums_Only(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void Allow_Nums_Only_And_TabAcross(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                CycleTime_Sec.Focus();
                CycleTime_Sec.CaretIndex = CycleTime_Sec.Text.Length;
                e.Handled = true;
                return;
            }

            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void AddLotButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BatchTextBox.Text))
            {
                MessageBox.Show("Please enter a material batch reference", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(QuantityNewLotTextBox.Text, out int n))
            {
                MessageBox.Show("Invalid quantity", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Lot newLot = new()
            {
                ProductName = Item.ProductName,
                Order = Item.AssignedMO,
                AddedBy = App.CurrentUser.UserName,
                Quantity = n,
                Date = DateTime.Now,
                IsReject = (bool)RejectCheckBox.IsChecked,
                IsAccepted = (bool)AcceptedCheckBox.IsChecked,
                IsDelivered = false,
                MaterialBatch = BatchTextBox.Text.Trim(),
                FromMachine = ProducedOnMachine,
                Remarks = RemarksTextBox.Text.Trim(),
                AllowDelivery = AllowDelivery,
                DateProduced = DateTime.Now.Hour >= 9
                    ? DateTime.Now.Date.AddHours(12)
                    : DateTime.Now.Date.AddHours(-12)
            };


            if (!DatabaseHelper.Insert(newLot))
            {
                MessageBox.Show("Failed to add to database", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LabelPrintingHelper.PrintLot(newLot);

            LotAdded = true;
            SaveExit = true;

            LoadData(Item.Id);
            LoadUI();

            QuantityNewLotTextBox.Text = "";
            RejectCheckBox.IsChecked = false;
        }

        //private void EditLot(Lot lotToEdit)
        //{
        //    EditLotWindow window = new(lotToEdit.ID);
        //    window.Owner = Application.Current.MainWindow;
        //    window.ShowDialog();
        //}

        private void BatchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            BatchGhost.Visibility = string.IsNullOrEmpty(BatchTextBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void QuantityNewLotTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            QuantityGhost.Visibility = string.IsNullOrEmpty(QuantityNewLotTextBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void RejectCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)AcceptedCheckBox.IsChecked)
            {
                AcceptedCheckBox.IsChecked = false;
            }
        }

        private void AcceptedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)RejectCheckBox.IsChecked)
            {
                RejectCheckBox.IsChecked = false;
            }
        }

        private void RemarksTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RemarksGhost.Visibility = string.IsNullOrEmpty(RemarksTextBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private ICommand _saveCommand;
        public static int? _toEdit = null;
        public ICommand EditCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(
                        param => SaveObject(),
                        param => CanSave()
                    );
                }
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


            EditLotWindow editWindow = new((int)_toEdit, CanEdit) { Owner = this };
            Hide();
            editWindow.ShowDialog();

            if (editWindow.SaveExit)
            {
                SaveExit = true;
                LoadData(Item.Id);
                LoadUI();
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
                if (execute == null)
                    throw new ArgumentNullException("execute");

                _execute = execute;
                _canExecute = canExecute;
            }

            #endregion

            #region ICommand Members

            [DebuggerStepThrough]
            public bool CanExecute(object parameters)
            {
                return _canExecute == null ? true : _canExecute(parameters);
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

        private void CycleTime_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            if (textBox.Text == "0") textBox.Text = "";
        }

        private void CycleTime_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            if (string.IsNullOrWhiteSpace(textBox.Text)) textBox.Text = "0";
        }
    }
}
