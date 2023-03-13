using DocumentFormat.OpenXml.Office2010.Excel;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace ProjectLighthouse.View.Programs
{
    public partial class AddProgramWindow : Window, INotifyPropertyChanged
    {
        public List<Product> Products { get; set; }
        public List<ProductGroup> ProductGroups;

        private List<ProductGroup> filteredProductGroups;
        public List<ProductGroup> FilteredProductGroups
        {
            get { return filteredProductGroups; }
            set 
            { 
                filteredProductGroups = value;
                OnPropertyChanged();
            }
        }



        public NcProgram? originalProgram { get; set; }
        public NcProgram Program { get; set; }

        private string newTag = "";
        public string NewTag
        {
            get { return newTag; }
            set 
            { 
                newTag = value;
                OnPropertyChanged();
            }
        }

        public bool SaveExit;

        public AddProgramWindow(List<Product> products, List<ProductGroup> groups, List<MaterialInfo> materials, List<Lathe> lathes)
        {
            InitializeComponent();
            Title = "New Program";

            Products = products;
            ProductGroups = groups;
            FilteredProductGroups = ProductGroups;

            Program = new();
            DataContext = this;

            UpdateButton.Visibility = Visibility.Collapsed;
        }


        public AddProgramWindow(List<Product> products, List<ProductGroup> groups, List<MaterialInfo> materials, List<Lathe> lathes, NcProgram existingProgram)
        {
            InitializeComponent();
            Title = "Edit Program";

            Products = new(products);
            ProductGroups = new(groups);

            Program = (NcProgram)existingProgram.Clone();
            originalProgram = existingProgram;

            DataContext = this;
            AddButton.Visibility = Visibility.Collapsed;

            LoadTargeting();
        }


        void LoadTargeting()
        {
            if (Program.Groups is null) return;

            List<string> groupStringIds = Program.Groups.Split(";").ToList();
            List<int> groupIds = new();

            for (int i = 0; i < groupStringIds.Count; i++)
            {
                if (!int.TryParse(groupStringIds[i], out int group)) 
                {
                    MessageBox.Show($"Failed to convert {groupStringIds[i]} to integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                groupIds.Add(group);
            }

            List<ProductGroup> targetedGroups = new();

            for(int i = 0; i < groupIds.Count; i++)
            {
                int id = groupIds[i];
                ProductGroup? g = ProductGroups.Find(x => x.Id == id);

                if (g is null)
                {
                    MessageBox.Show($"Failed to find group with ID {id}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                targetedGroups.Add(g);
            }

            List<int> productIds = new();
            for(int i = 0; i < Products.Count; i++)
            {
                Product p = Products[i];

                if (targetedGroups.Any(x => x.ProductId == p.Id))
                {
                    ProductsListView.SelectedItems.Add(p);
                    productIds.Add(p.Id);
                }
            }

            FilterProductGroups(productIds);

            for (int i = 0; i < targetedGroups.Count; i++)
            {
                ProductGroup g = FilteredProductGroups.Find(x => x.Id == targetedGroups[i].Id);
                if (g is null) continue;
                FilteredArchetypesListBox.SelectedItems.Add(g);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void AddTagButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> tags = Program.TagsList;
            tags.Add(NewTag);
            Program.TagsList = tags;

            Program.ValidateTags();

            NewTag = "";
            AddTagButton.IsEnabled = Program.TagsList.Count < 10;
        }

        private void DeleteTagButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.CommandParameter is not string tagToRemove) return;

            List<string> tags = Program.TagsList;
            tags.Remove(tagToRemove);
            Program.TagsList = tags;

            Program.ValidateTags();

            NewTag = "";
            AddTagButton.IsEnabled = Program.TagsList.Count < 10;
        }

        void ApplyTargeting()
        {
            List<int> ids = new();

            foreach (ProductGroup group in FilteredArchetypesListBox.SelectedItems)
            {
                ids.Add(group.Id);
            }

            Program.Groups = string.Join(";", ids);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyTargeting();
            Program.Notepad = Program.Notepad.Trim();
            Program.ValidateAll();

            if (originalProgram is not null)
            {
                originalProgram.ValidateAll();

                // Prevent new data errors
                if (originalProgram.NoErrors && Program.HasErrors)
                {
                    return;
                }

                try
                {
                    UpdateExistingProgram();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while inserting to the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                if (Program.HasErrors)
                {
                    return;
                }

                try
                {
                    CreateProgram();
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

        void UpdateExistingProgram()
        {
            try
            {
                DatabaseHelper.Update(Program, throwErrs: true);
                originalProgram = Program;
            }
            catch
            {
                throw;
            }
        }

        void CreateProgram()
        {
            try
            {
                DatabaseHelper.Insert(Program, throwErrs: true);
            }
            catch
            {
                throw;
            }
        }

        private void ProductsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListView listView) return;

            List<int> ids = new();

            foreach (Product product in listView.SelectedItems)
            {
                ids.Add(product.Id);
            }

            FilterProductGroups(ids);
        }

        void FilterProductGroups(List<int> productIds)
        {
            FilteredProductGroups = ProductGroups.Where(x => x.ProductId is not null).Where(x => productIds.Contains((int)x.ProductId!)).ToList();
        }

        private void Notepad_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextLengthCounter.Text = $"{Program.Notepad.Length}/1024";
        }
    }
}
