using Microsoft.Win32;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class AddProductWindow : Window, INotifyPropertyChanged
    {
        public Product Product { get; set; }

        private List<string> existingFeatures;
        public List<string> ExistingFeatures
        {
            get { return existingFeatures; }
            set
            {
                existingFeatures = value;
                OnPropertyChanged();
            }
        }

        private List<string> baseExistingFeaturesList;

        public Product? originalProduct;

        public bool SaveExit;


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AddProductWindow(List<string> existingMachineFeatures, Product? product = null)
        {
            InitializeComponent();

            baseExistingFeaturesList = existingMachineFeatures;

            if (product is not null)
            {
                originalProduct = product;
                Product = (Product)product.Clone();
                Product.ValidateAll();

                ExistingFeatures = baseExistingFeaturesList.Where(x => !Product.RequiresFeaturesList.Contains(x)).ToList();

                CreateButton.Visibility = Visibility.Collapsed;
                Title = "Edit Product";
            }
            else
            {
                Product = new();
                ExistingFeatures = baseExistingFeaturesList.ToList();
                UpdateButton.Visibility = Visibility.Collapsed;
            }

            DataContext = this;
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
                string newSource = Path.GetFileName(openFileDialog.FileName);
                string path = App.ROOT_PATH + @"lib\renders\" + newSource;
                if (!File.Exists(path))
                {
                    File.Copy(openFileDialog.FileName, path);
                }

                App.MoveToLocalAppData(path);

                Product.ImageUrl = newSource;
                DatabaseHelper.Update(Product, throwErrs: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void RemoveFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not string feature) return;
            Product.RequiresFeaturesList = Product.RequiresFeaturesList.Where(x => x != feature).ToList();
            ExistingFeatures = baseExistingFeaturesList.Where(x => !Product.RequiresFeaturesList.Contains(x)).ToList();
        }

        private void AddFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not string feature) return;
            Product.RequiresFeaturesList = Product.RequiresFeaturesList.Append(feature).ToList();
            ExistingFeatures = baseExistingFeaturesList.Where(x => !Product.RequiresFeaturesList.Contains(x)).ToList();
        }
    }
}
