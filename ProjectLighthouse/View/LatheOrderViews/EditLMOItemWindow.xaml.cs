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

        public bool SaveExit;

        public EditLMOItemWindow(LatheManufactureOrderItem item, List<Lot> lots)
        {
            SaveExit = false;
            Item = item;
            Lots = (lots == null) ? new List<Lot>() : lots;
            InitializeComponent();

            intQtyMade = 0;
            intQtyReject = 0;
            intQtyDelivered = 0;

            if(Lots.Count > 0)
            {
                foreach(Lot lot in Lots)
                {
                    if (!lot.IsReject)
                    {
                        intQtyMade += lot.Quantity;
                        if (lot.IsDelivered)
                            intQtyDelivered += lot.Quantity;
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

            QtyMadeTextBlock.Text = string.Format("{0:#,##0} pcs", Item.QuantityMade);
            QtyRejectTextBlock.Text = string.Format("{0:#,##0} pcs", Item.QuantityReject);
            QtyDeliveredTextBlock.Text = string.Format("{0:#,##0} pcs", Item.QuantityDelivered);
            QuantityRequiredTextbox.Text = Item.RequiredQuantity.ToString();
            QuantityTargetTextbox.Text = Item.TargetQuantity.ToString();
            DateRequiredPicker.SelectedDate = Item.DateRequired;
            ProductNameTextBlock.Text = Item.ProductName;
            ManufactureOrderTextBlock.Text = Item.AssignedMO;
            SchedulingGrid.Visibility = App.currentUser.UserRole == "Scheduling" || App.currentUser.UserRole == "admin" ? Visibility.Visible : Visibility.Collapsed;
            LotsListBox.ItemsSource = Lots;

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

            //if (!Int32.TryParse(QuantityMadeTextbox.Text, out int qtyMade))
            //{
            //    MessageBox.Show("Invalid entry to Quantity Made field.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            //if (!Int32.TryParse(QuantityRejectTextbox.Text, out int qtyReject))
            //{
            //    MessageBox.Show("Invalid entry to Quantity Rejected field", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            #endregion

            //Item.QuantityMade = qtyMade;
            //Item.QuantityReject = qtyReject;
            Item.RequiredQuantity = reqqty;
            Item.TargetQuantity = tarqty;
            Item.DateRequired = (DateTime)DateRequiredPicker.SelectedDate;
            Item.UpdatedAt = DateTime.Now;
            Item.UpdatedBy = App.currentUser.GetFullName();

            DatabaseHelper.Update(Item);

            //Update master cycle time record
            var product = DatabaseHelper.Read<TurnedProduct>().Where(n => n.ProductName == Item.ProductName).ToList();
            //should return only one!
            if (product != null)
            {
                TurnedProduct thisProduct = product.First();
                thisProduct.CycleTime = cycleTime;
                DatabaseHelper.Update(thisProduct);
            }
            else
            {
                MessageBox.Show("Failed to update product record.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            SaveExit = true;
            this.Close();
        }

        private void PopulateCycleTimes()
        {
            intCycleTimeText.Text = String.Format("({0}s)", Item.CycleTime);
            int secs = Item.CycleTime % 60;
            int mins = (Item.CycleTime - secs) / 60;

            CycleTime_Min.Text = mins.ToString();
            CycleTime_Sec.Text = secs.ToString();
        }

        private void CalculateCycleTime()
        {
            var bc = new BrushConverter();

            if (Int32.TryParse(CycleTime_Min.Text, out int min))
            {
                CycleTime_Min.BorderBrush = (Brush)bc.ConvertFrom("#FFABADB3");
            }
            else
            {
                CycleTime_Min.BorderBrush = Brushes.Red;
                intCycleTimeText.Text = "(?s)";
                return;
            }

            if (Int32.TryParse(CycleTime_Sec.Text, out int sec))
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
            intCycleTimeText.Text = String.Format("({0}s)", cycleTime);

        }

        private void CycleTime_Min_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateCycleTime();
        }

        private void CycleTime_Sec_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateCycleTime();
        }

        private void ClearDateButton_Click(object sender, RoutedEventArgs e)
        {
            Item.DateRequired = DateTime.MinValue;
        }
    }
}
