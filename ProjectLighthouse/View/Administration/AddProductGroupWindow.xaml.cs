using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;

namespace ProjectLighthouse.View.Administration
{
    public partial class AddProductGroupWindow : Window
    {
        public ProductGroup Group { get; set; }
        public Product Product { get; set; }

        public bool SaveExit = false;
        public AddProductGroupWindow(Product product)
        {
            InitializeComponent();
            Product = product;  
            Group = new() { ProductId = product.Id };
            DataContext = this;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Group.ValidateAll();

            if(Group.HasErrors)
            {
                return;
            }

            bool inserted;
            try
            {
                inserted = DatabaseHelper.Insert(Group, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inserting to database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (inserted)
            {
                SaveExit = true;
                Close();
                return;
            }
        }
    }
}
