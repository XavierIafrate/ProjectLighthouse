using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.View.Orders
{
    public partial class CreateBarIssueWindow : Window
    {
        public bool Confirmed;
        public BarStock Bar;
        public LatheManufactureOrder Order;

        public CreateBarIssueWindow()
        {
            InitializeComponent();
        }

        public void SetupInterface()
        {
            OrderRefText.Text = $"New Bar Issue: {Order.Name}";
            BarIdText.Text = $"Bar ID: {Bar.Id}";
            BarInStockText.Text = $"{Bar.InStock:0} in stock";
            OrderRequirementText.Text = $"Order Requires {Math.Max(Order.NumberOfBars - Order.NumberOfBarsIssued, 0):0}";
            QtyTextBox.Text = Math.Min(Bar.InStock, (Order.NumberOfBars - Order.NumberOfBarsIssued)).ToString("0");
        }

        private void QtyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private bool ValidateInputs()
        {
            if (Bar.MaterialData is null)
            {
                MessageBox.Show("Bar material data is null.", "Internal error");
                return false;
            }

            if (int.TryParse(QtyTextBox.Text, out int quantity))
            {
                if (quantity <= 0 || quantity > Bar.InStock)
                {
                    MessageBox.Show("Invalid Quantity");
                    return false;
                }

                if (quantity > Order.NumberOfBars - Order.NumberOfBarsIssued)
                {
                    if (MessageBox.Show("You are issuing more bars than is required by the order record - is this correct?", "Confirm input", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return false;
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid Quantity");
                return false;
            }

            if (string.IsNullOrEmpty(BatchInfoTextBox.Text.Trim()))
            {
                MessageBox.Show("Input batch number");
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            if (!int.TryParse(QtyTextBox.Text, out int qty))
            {
                return;
            }

            BarIssue barIssue = new()
            {
                Date = DateTime.Now,
                IssuedBy = App.CurrentUser.UserName,
                BarId = Bar.Id,
                MaterialBatch = BatchInfoTextBox.Text.Trim().ToUpperInvariant(),
                OrderId = Order.Name,
                Quantity = qty,
                MaterialInfo = $"{Bar.MaterialData.MaterialText.ToUpperInvariant()}, GRADE {Bar.MaterialData.GradeText.ToUpperInvariant()}"
            };

            //TODO direct sql
            LatheManufactureOrder freshCopyOfOrder = DatabaseHelper.Read<LatheManufactureOrder>().Find(x => x.Name == Order.Name);
            freshCopyOfOrder.ModifiedAt = DateTime.Now;
            freshCopyOfOrder.ModifiedBy = App.CurrentUser.GetFullName();
            freshCopyOfOrder.NumberOfBarsIssued += qty;

            Bar.InStock -= qty;

            Order = freshCopyOfOrder;
            Order.BarIssues.Add(barIssue);

            if (DatabaseHelper.Insert(barIssue) && DatabaseHelper.Update(freshCopyOfOrder) && DatabaseHelper.Update(Bar))
            {
                Confirmed = true;
                LabelPrintingHelper.PrintIssue(barIssue, 2);
                Close();
            }
        }
    }
}
