using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for EditLotWindow.xaml
    /// </summary>
    public partial class EditLotWindow : Window
    {
        public bool SaveExit = false;
        public Lot EditLot { get; set; }
        private Lot originalLot { get; set; }

        public EditLotWindow(Lot lot)
        {
            InitializeComponent();
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

            if (EditLot.IsDelivered)
            {
                RejectCheckBox.IsEnabled = false;
                AcceptCheckBox.IsEnabled = false;
                QuantityTextBox.IsEnabled = false;
                BatchTextBox.IsEnabled = false;
            }
        }

        public void Save()
        {
            EditLot.IsReject = (bool)RejectCheckBox.IsChecked;
            EditLot.IsAccepted = (bool)AcceptCheckBox.IsChecked;
            EditLot.Remarks = RemarksTextBox.Text.Trim();

            if (originalLot.Quantity == EditLot.Quantity &&
                originalLot.MaterialBatch == EditLot.MaterialBatch &&
                originalLot.IsReject == EditLot.IsReject &&
                originalLot.IsAccepted == EditLot.IsAccepted &&
                originalLot.Remarks == EditLot.Remarks)
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

        private void BatchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            EditLot.MaterialBatch = BatchTextBox.Text ?? "";
        }

        private void QuantityTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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
            if ((bool)AcceptCheckBox.IsChecked)
            {
                AcceptCheckBox.IsChecked = false;
            }
        }

        private void AcceptCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)RejectCheckBox.IsChecked)
            {
                RejectCheckBox.IsChecked = false;
            }
        }
    }
}
