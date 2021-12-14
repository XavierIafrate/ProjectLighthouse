using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View
{
    public partial class EditLMOItemWindow : Window
    {
        public LatheManufactureOrderItem Item;
        public List<Lot> Lots;
        public int intQtyMade;
        public int intQtyReject;
        public int intQtyDelivered;
        private bool LotAdded;
        private string ProducedOnMachine { get; set; }

        public bool SaveExit;

        public EditLMOItemWindow(LatheManufactureOrderItem item, List<Lot> lots, string MachineID)
        {
            InitializeComponent();
            SaveExit = false;
            Item = item;
            Lots = lots;
            foreach (Lot lot in Lots)
            {
                lot.RequestToEdit += EditLot;
            }

            ProducedOnMachine = MachineID;

            LoadUI();
        }

        private void LoadUI()
        {
            intQtyMade = 0;
            intQtyReject = 0;
            intQtyDelivered = 0;

            if (Lots.Count > 0)
            {
                foreach (Lot lot in Lots)
                {
                    if (lot.ProductName != Item.ProductName)
                    {
                        continue;
                    }

                    if (!lot.IsReject)
                    {
                        intQtyMade += lot.Quantity;

                        if (lot.IsDelivered)
                        {
                            intQtyDelivered += lot.Quantity;
                        }
                    }
                    else
                    {
                        intQtyReject += lot.Quantity;
                    }
                }
            }

            Item.QuantityDelivered = intQtyDelivered;
            Item.QuantityMade = intQtyMade;
            Item.QuantityReject = intQtyReject;

            QtyMadeTextBlock.Text = $"{Item.QuantityMade:#,##0} pcs";
            QtyRejectTextBlock.Text = $"{Item.QuantityReject:#,##0} pcs";
            QtyDeliveredTextBlock.Text = $"{Item.QuantityDelivered:#,##0} pcs";
            QuantityRequiredTextbox.Text = Item.RequiredQuantity.ToString();
            QuantityTargetTextbox.Text = Item.TargetQuantity.ToString();
            DateRequiredPicker.SelectedDate = Item.DateRequired;
            DateDisplay.Text = Item.DateRequired.ToString("dd/MM/yy");

            ProductNameTextBlock.Text = Item.ProductName;
            ManufactureOrderTextBlock.Text = Item.AssignedMO;

            SchedulingGrid.Visibility = App.CurrentUser.UserRole is "Scheduling" or "admin"
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
            if (!Int32.TryParse(CycleTime_Min.Text, out int min))
            {
                MessageBox.Show("Invalid cycle time entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Int32.TryParse(CycleTime_Sec.Text, out int sec))
            {
                MessageBox.Show("Invalid cycle time entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Int32.TryParse(QuantityRequiredTextbox.Text, out int reqqty))
            {
                MessageBox.Show("Invalid Quantity Required entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Int32.TryParse(QuantityTargetTextbox.Text, out int tarqty))
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

            //Update master cycle time record
            List<TurnedProduct> product = DatabaseHelper.Read<TurnedProduct>().Where(n => n.ProductName == Item.ProductName).ToList();
            //should return only one!
            if (product != null)
            {
                TurnedProduct thisProduct = product.First();
                thisProduct.CycleTime = cycleTime;
                if (LotAdded)
                {
                    thisProduct.QuantityManufactured = Lots.Where(l => l.ProductName == Item.ProductName).Sum(l => l.Quantity);
                    thisProduct.lastManufactured = DateTime.Now;
                }

                DatabaseHelper.Update(thisProduct);
            }
            else
            {
                MessageBox.Show("Failed to update product record.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void Allow_Nums_Only(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void AddLotButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BatchTextBox.Text))
            {
                MessageBox.Show("Please enter a material batch reference", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (int.TryParse(QuantityNewLotTextBox.Text, out int n))
            {
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
                    DateProduced = DateTime.Now.Hour >= 8 
                        ? DateTime.Now.Date.AddHours(12) 
                        : DateTime.Now.Date.AddHours(-12)
                };

                newLot.DateProduced = newLot.Date;
                newLot.SetExcelDateTime();

                if (!DatabaseHelper.Insert(newLot))
                {
                    MessageBox.Show("Failed to add to database", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (newLot.IsReject)
                {
                    Item.QuantityReject += n;
                }
                else
                {
                    Item.QuantityMade += n;
                }

                LotAdded = true;
                DatabaseHelper.Update<LatheManufactureOrderItem>(Item);

                Lots.Add(newLot);
                SaveExit = true;
                List<Lot> refreshedLots = new(Lots.Where(l => l.ProductName == Item.ProductName));

                foreach (Lot lot in refreshedLots)
                {
                    lot.RequestToEdit += EditLot;
                }

                LotsListBox.ItemsSource = refreshedLots;
                QtyMadeTextBlock.Text = $"{Item.QuantityMade:#,##0} pcs";
                QtyRejectTextBlock.Text = $"{Item.QuantityReject:#,##0} pcs";

                QuantityNewLotTextBox.Text = "";
                RejectCheckBox.IsChecked = false;
            }
            else
            {
                MessageBox.Show("Invalid quantity", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void EditLot(Lot lotToEdit)
        {
            EditLotWindow window = new(lotToEdit);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();

            Lots = null;
            Lots = DatabaseHelper.Read<Lot>().Where(n => n.Order == Item.AssignedMO).ToList();
            foreach (Lot lot in Lots)
            {
                lot.RequestToEdit += EditLot;
            }

            LoadUI();
        }

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

        private void DateRequiredPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime SelectedDate = (DateTime)DateRequiredPicker.SelectedDate;
            DateDisplay.Text = SelectedDate.ToString("dd/MM/yy");
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
    }
}
