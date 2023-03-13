using DocumentFormat.OpenXml.Office2010.ExcelAc;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.View.Programs;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ProjectLighthouse.View.Administration
{
    public partial class AddProductGroupWindow : Window
    {
        public ProductGroup Group { get; set; }
        public ProductGroup? originalGroup;
        public Product Product { get; set; }
        public List<Product> Products { get; set; }
        public List<Model.Programs.NcProgram> ProgramList { get; set; }

        public bool SaveExit = false;
        public AddProductGroupWindow(Product product, List<Product> products)
        {
            InitializeComponent();
            Product = product;  
            Products = products;
            Group = new() { ProductId = product.Id };

            this.Title = "New Archetype";
            UpdateButton.Visibility = Visibility.Collapsed;

            DataContext = this;
        }

        public AddProductGroupWindow(Product product, ProductGroup group, List<Model.Programs.NcProgram> programs, List<Product> products)
        {
            InitializeComponent();
            Product = product;
            Products = products;
            ProgramList = programs;

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
    }
}
