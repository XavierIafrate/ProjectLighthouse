using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Win32;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.IO;
using System.Windows;

namespace ProjectLighthouse.View.Administration
{
    public partial class AddProductWindow : Window
    {
        public Product Product { get; set; }
        public Product? originalProduct;

        public bool SaveExit = false;


        public AddProductWindow(Product? product = null)
        {
            InitializeComponent();

            if (product is not null)
            {
                originalProduct = product;
                Product = (Product)product.Clone();
                Product.ValidateAll();


                CreateButton.Visibility = Visibility.Collapsed;
                Title = "Edit Product";
            }
            else
            {
                Product = new();
                UpdateButton.Visibility = Visibility.Collapsed;
            }
            DataContext= this;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Product.ValidateAll();

            if (originalProduct is not null)
            {
                originalProduct.ValidateAll();
                // Prevent new data errors
                if (originalProduct.NoErrors && Product.HasErrors)
                {
                    return;
                }

                try
                {
                    UpdateExistingProduct();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while inserting to the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                if (Product.HasErrors)
                {
                    return;
                }

                try
                {
                    CreateProduct();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            originalProduct = Product;
            SaveExit = true;
            Close();
        }

        void UpdateExistingProduct()
        {
            try
            {
                DatabaseHelper.Update(Product, throwErrs: true);
                originalProduct = Product;
            }
            catch
            {
                throw;
            }
        }

        void CreateProduct()
        {
            try
            {
                DatabaseHelper.Insert(Product, throwErrs: true);
            }
            catch
            {
                throw;
            }
        }

        private void ChangeImageButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();


            OpenFileDialog openFileDialog = new()
            {
                Filter = "Images|*.jpg",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (openFileDialog.ShowDialog() == false) return;
            if (!File.Exists(openFileDialog.FileName))
            {
                return;
            }

            if (Product.LocalRenderPath is not null)
            {
                try
                {
                    File.Delete(App.ROOT_PATH + @"lib\renders\" + Product.ImageUrl);
                    Product.ImageUrl = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            try
            {
                string path = App.ROOT_PATH + @"lib\renders\" + Path.GetFileName(openFileDialog.FileName);
                File.Copy(openFileDialog.FileName, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
