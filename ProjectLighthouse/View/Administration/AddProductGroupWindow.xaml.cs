using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using static ProjectLighthouse.Model.Products.ProductGroup;

namespace ProjectLighthouse.View.Administration
{
    public partial class AddProductGroupWindow : Window, INotifyPropertyChanged
    {
        public ProductGroup Group { get; set; }
        public ProductGroup? originalGroup;
        public Product Product { get; set; }
        public List<Product> Products { get; set; }
        public Array Statuses { get; set; }

        public bool SaveExit;

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AddProductGroupWindow(Product product, List<Product> products, List<string> existingMachineFeatures)
        {
            InitializeComponent();
            baseExistingFeaturesList = existingMachineFeatures;
            ExistingFeatures = baseExistingFeaturesList.ToList();

            Statuses = Enum.GetValues(typeof(GroupStatus));

            Product = product;
            Products = products;
            Group = new() { ProductId = product.Id };

            this.Title = "New Archetype";
            UpdateButton.Visibility = Visibility.Collapsed;

            DataContext = this;
        }

        public AddProductGroupWindow(Product product, ProductGroup group, List<Product> products, List<string> existingMachineFeatures)
        {
            InitializeComponent();
            baseExistingFeaturesList = existingMachineFeatures;
            ExistingFeatures = baseExistingFeaturesList.Where(x => !group.RequiresFeaturesList.Contains(x)).ToList();

            Statuses = Enum.GetValues(typeof(GroupStatus));

            Product = product;
            Products = products;

            originalGroup = group;
            Group = (ProductGroup)group.Clone();

            this.Title = "Edit Archetype";
            CreateButton.Visibility = Visibility.Collapsed;

            DataContext = this;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Group.ValidateAll();

            if (originalGroup is not null)
            {
                originalGroup.ValidateAll();
                // Prevent new data errors
                if (originalGroup.NoErrors && Group.HasErrors)
                {
                    return;
                }

                try
                {
                    UpdateExistingGroup();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while inserting to the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                if (Group.HasErrors)
                {
                    return;
                }

                try
                {
                    CreateGroup();
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

        void UpdateExistingGroup()
        {
            try
            {
                DatabaseHelper.Update(Group, throwErrs: true);
                originalGroup = Group;
            }
            catch
            {
                throw;
            }
        }

        void CreateGroup()
        {
            try
            {
                DatabaseHelper.Insert(Group, throwErrs: true);
            }
            catch
            {
                throw;
            }
        }

        private void RemoveFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not string feature) return;
            Group.RequiresFeaturesList = Group.RequiresFeaturesList.Where(x => x != feature).ToList();
            ExistingFeatures = baseExistingFeaturesList.Where(x => !Group.RequiresFeaturesList.Contains(x)).ToList();
        }

        private void AddFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not string feature) return;
            Group.RequiresFeaturesList = Group.RequiresFeaturesList.Append(feature).ToList();
            ExistingFeatures = baseExistingFeaturesList.Where(x => !Group.RequiresFeaturesList.Contains(x)).ToList();
        }
    }
}
