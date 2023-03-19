using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.Orders
{
    public partial class EditLotWindow : Window
    {
        public bool SaveExit = false;
        public Lot EditLot { get; set; }
        public bool CanEdit { get; set; }
        private Lot originalLot;

        public EditLotWindow(int lotId, bool edit)
        {
            InitializeComponent();

            CanEdit = edit;

            Lot lot = DatabaseHelper.Read<Lot>().Find(x => x.ID == lotId)
                ?? throw new Exception($"Could not find lot with id '{lotId:0}'");

            EditLot = (Lot)lot.Clone();
            originalLot = lot;
            PopulateControls();
        }

        private void PopulateControls()
        {
            QuantityTextBox.Text = EditLot.Quantity.ToString();
            RejectCheckBox.IsChecked = EditLot.IsReject;
            AcceptCheckBox.IsChecked = EditLot.IsAccepted;
            BatchTextBox.Text = EditLot.MaterialBatch;
            RemarksTextBox.Text = EditLot.Remarks;
            ProducedDatePicker.SelectedDate = EditLot.DateProduced.Year < 2000
                ? EditLot.Date
                : EditLot.DateProduced;

            AllowDeliveryCheckbox.IsChecked = EditLot.AllowDelivery;
            AllowDeliveryCheckbox.IsEnabled = App.CurrentUser.Role >= UserRole.Scheduling;

            if (EditLot.IsDelivered)
            {
                RejectCheckBox.IsEnabled = false;
                AcceptCheckBox.IsEnabled = false;
                QuantityTextBox.IsEnabled = false;
                BatchTextBox.IsEnabled = false;
                AllowDeliveryCheckbox.IsEnabled = false;
            }
        }

        public void Save()
        {
            EditLot.IsReject = RejectCheckBox.IsChecked ?? false;
            EditLot.IsAccepted = AcceptCheckBox.IsChecked ?? false;
            EditLot.AllowDelivery = AllowDeliveryCheckbox.IsChecked ?? false;
            EditLot.Remarks = RemarksTextBox.Text.Trim();

            if (originalLot.Quantity == EditLot.Quantity &&
                originalLot.MaterialBatch == EditLot.MaterialBatch &&
                originalLot.IsReject == EditLot.IsReject &&
                originalLot.AllowDelivery == EditLot.AllowDelivery &&
                originalLot.IsAccepted == EditLot.IsAccepted &&
                originalLot.Remarks == EditLot.Remarks &&
                originalLot.DateProduced == EditLot.DateProduced &&
                originalLot.AllowDelivery == EditLot.AllowDelivery)
            {
                Close();
                return;
            }

            EditLot.ModifiedAt = DateTime.Now;
            EditLot.ModifiedBy = App.CurrentUser.GetFullName();

            DatabaseHelper.Update<Lot>(EditLot);

            SaveExit = true;
            Close();
        }

        private void QuantityTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void BatchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditLot.MaterialBatch = BatchTextBox.Text ?? "";
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out int j))
            {
                EditLot.Quantity = j;
            }

            if (string.IsNullOrEmpty(QuantityTextBox.Text))
            {
                EditLot.Quantity = 0;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void RejectCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (AcceptCheckBox.IsChecked ?? false)
            {
                AcceptCheckBox.IsChecked = false;
            }
        }

        private void AcceptCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (RejectCheckBox.IsChecked ?? false)
            {
                RejectCheckBox.IsChecked = false;
            }
        }

        private void ProducedDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not DatePicker picker) return;
            if (picker.SelectedDate is null) return;

            if (EditLot != null)
            {
                EditLot.DateProduced = picker.SelectedDate.Value.AddHours(12);
            }
        }

        private void AllowDeliveryCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            EditLot.AllowDelivery = AllowDeliveryCheckbox.IsChecked ?? false;
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            LabelPrintingHelper.PrintLot(EditLot);
            PrintButton.IsEnabled = false;
        }
    }
}
