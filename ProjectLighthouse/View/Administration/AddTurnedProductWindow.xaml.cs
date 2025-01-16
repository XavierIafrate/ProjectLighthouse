using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class AddTurnedProductWindow : Window, INotifyPropertyChanged
    {
        public TurnedProduct Product { get; set; }

        public TurnedProduct? originalProduct;

        public List<MaterialInfo> Materials { get; set; }
        public List<ProductGroup> ProductGroups { get; set; }
        public List<ProductGroup> FilteredProductGroups { get; set; } = new();

        public bool SaveExit;

        public event PropertyChangedEventHandler PropertyChanged;

        public AddTurnedProductWindow(List<ProductGroup> Groups, TurnedProduct? product = null, int? groupId = null)
        {
            InitializeComponent();
            Materials = DatabaseHelper.Read<MaterialInfo>();
            ProductGroups = Groups;

            if (product is not null)
            {
                if (product.GroupId is not null)
                {
                    ProductGroup? group = ProductGroups.Find(x => x.Id == product.GroupId);
                    if (group != null)
                    {
                        FilteredProductGroups.Add(group);
                    }
                }

                Product = (TurnedProduct)product.Clone();
                Product.ItemCost = product.ItemCost;
                originalProduct = product;
                ProductNameTextBox.IsEnabled = false;
                Title = "Edit Turned Product";
                TitleText.Text = "Edit Turned Product";
                AddButton.Visibility = Visibility.Collapsed;
                UpdateButton.Visibility = Visibility.Visible;

                OpenAttachmentButton.IsEnabled = !string.IsNullOrWhiteSpace(product.SpecificationDocument);
            }
            else
            {
                Product = new() { AddedBy = App.CurrentUser.UserName, AddedDate = DateTime.Now };

                if (groupId is not null)
                {
                    Product.GroupId = groupId;
                }
                AddButton.Visibility = Visibility.Visible;
                UpdateButton.Visibility = Visibility.Collapsed;

                CycleTimeLabel.Visibility = Visibility.Collapsed;
                CycleTimeTextBox.Visibility = Visibility.Collapsed;
            }


            DataContext = this;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
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
                Product.AddedBy = App.CurrentUser.UserName;
                Product.AddedDate = DateTime.Now;
                DatabaseHelper.Insert(Product, throwErrs: true);
            }
            catch
            {
                throw;
            }
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            string searchText = textBox.Text.Trim().ToUpperInvariant();

            FilteredProductGroups = ProductGroups.Where(x => x.Name.ToUpperInvariant().Contains(searchText)).ToList();
            OnPropertyChanged(nameof(FilteredProductGroups));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenAttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + Path.Join(App.ROOT_PATH,Product.SpecificationDocument) + "\"";

            try
            {
                bool success = fileopener.Start();
                if (!success) 
                {
                    MessageBox.Show("Failed to open document", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("Failed to open document", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
