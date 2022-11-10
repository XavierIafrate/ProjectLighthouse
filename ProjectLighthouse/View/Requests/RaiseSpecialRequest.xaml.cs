using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;

namespace ProjectLighthouse.View.Requests
{
    public partial class RaiseSpecialRequest : Window
    {
        public TurnedProduct NewProduct { get; set; }
        public bool productAdded = false;
        public RaiseSpecialRequest()
        {
            InitializeComponent();
            NewProduct = new() { isSpecialPart = true };
        }


        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(productName.Text))
            {
                MessageBox.Show("Product needs a name.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            else
            {
                NewProduct.ProductName = productName.Text.Trim().ToUpperInvariant();
            }

            if (string.IsNullOrWhiteSpace(specDetails.Text) && string.IsNullOrEmpty(specDocument.FilePath))
            {
                MessageBox.Show("You need to provide a specification by one of the given methods.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }


            if (!string.IsNullOrEmpty(specDocument.FilePath))
            {
                string newPath = $@"{App.ROOT_PATH}lib\{System.IO.Path.GetFileName(specDocument.FilePath)}";
                try
                {
                    System.IO.File.Copy(specDocument.FilePath, newPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
                NewProduct.SpecificationDocument = $@"lib\{System.IO.Path.GetFileName(specDocument.FilePath)}";
            }

            if (!string.IsNullOrWhiteSpace(specDetails.Text))
            {
                NewProduct.SpecificationDetails = specDetails.Text.Trim();
            }

            if (DatabaseHelper.Insert<TurnedProduct>(NewProduct))
            {
                MessageBox.Show($"Successfully added {NewProduct.ProductName} to database, you can now raise a request.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                productAdded = true;
                Close();
            }
            else
            {
                MessageBox.Show($"Something bad happened, please notify an administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void productName_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressForProductName(e);
        }
    }
}
