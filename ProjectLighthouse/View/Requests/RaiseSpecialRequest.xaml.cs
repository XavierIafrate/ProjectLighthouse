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
            NewProduct = new() { IsSpecialPart = true };
            DataContext = this;
        }


        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
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

            NewProduct.AddedBy = App.CurrentUser.UserName;
            NewProduct.AddedDate = DateTime.Now;

            try
            {
                DatabaseHelper.Insert<TurnedProduct>(NewProduct, throwErrs: true);
                productAdded = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lighthouse encountered an error while inserting to the database.{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
