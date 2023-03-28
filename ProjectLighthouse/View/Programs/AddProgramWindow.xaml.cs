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

        public List<MaterialInfo> Materials { get; set; }
        public List<string> MachineKeys { get; set; }

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
            FilteredProductGroups = new();
            Materials = materials.OrderBy(x => x.MaterialText).ThenBy(x => x.GradeText).ToList();
            MachineKeys = lathes.Select(x => x.MachineKey).Distinct().OrderBy(x => x).ToList();

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
            Materials = materials.OrderBy(x => x.MaterialText).ThenBy(x => x.GradeText).ToList();
            MachineKeys = lathes.Select(x => x.MachineKey).Distinct().OrderBy(x => x).ToList();

            Program = (NcProgram)existingProgram.Clone();
            originalProgram = existingProgram;

            DataContext = this;
            AddButton.Visibility = Visibility.Collapsed;

            LoadTargeting();
            LoadConstraints();
        }


        void LoadTargeting()
        {
            List<string> productStringIds = Program.ProductStringIds;
            List<string> groupStringIds = Program.GroupStringIds;

            List<int> productIds = new();
            for (int i = 0; i < productStringIds.Count; i++)
            {
                if (!int.TryParse(productStringIds[i], out int product))
                {
                    MessageBox.Show($"Failed to convert '{productStringIds[i] ?? "null"}' to integer while looking for product ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                productIds.Add(product);
            }

            for (int i = 0; i < productIds.Count; i++)
            {
                Product p = Products.Find(x => x.Id == productIds[i]);
                if (p is null) continue;
                ProductsListView.SelectedItems.Add(p);
            }

            List<int> groupIds = new();
            for (int i = 0; i < groupStringIds.Count; i++)
            {
                if (!int.TryParse(groupStringIds[i], out int group))
                {
                    MessageBox.Show($"Failed to convert '{groupStringIds[i] ?? "null"}' to integer while looking for archetype ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                groupIds.Add(group);
            }


            FilterProductGroups(productIds);

            for (int i = 0; i < groupIds.Count; i++)
            {
                ProductGroup g = FilteredProductGroups.Find(x => x.Id == groupIds[i]);
                if (g is null) continue;
                FilteredArchetypesListBox.SelectedItems.Add(g);
            }
        }

        void LoadConstraints()
        {
            if (!string.IsNullOrEmpty(Program.Materials))
            {
                LoadMaterialSelections();
            }

            if (!string.IsNullOrEmpty(Program.Machines))
            {
                LoadMachineSelections();
            }
        }

        void LoadMachineSelections()
        {
            List<string> machineKeys = Program.MachinesList;

            for (int i = 0; i < machineKeys.Count; i++)
            {
                string m = MachineKeys.Find(x => x == machineKeys[i]);
                if (m is null) continue;
                MachinesListView.SelectedItems.Add(m);
            }
        }

        void LoadMaterialSelections()
        {
            List<string> materialStringIds = Program.MaterialsList;

            List<int> materialIds = new();
            for (int i = 0; i < materialStringIds.Count; i++)
            {
                if (!int.TryParse(materialStringIds[i], out int material))
                {
                    MessageBox.Show($"Failed to convert '{materialStringIds[i] ?? "null"}' to integer while looking for material ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                materialIds.Add(material);
            }

            for (int i = 0; i < materialIds.Count; i++)
            {
                MaterialInfo m = Materials.Find(x => x.Id == materialIds[i]);
                if (m is null) continue;
                MaterialsListView.SelectedItems.Add(m);
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
            List<int> productIds = new();
            foreach (Product product in ProductsListView.SelectedItems)
            {
                productIds.Add(product.Id);
            }
            Program.Products = productIds.Count == 0
                ? null
                : string.Join(";", productIds);

            List<int> groupIds = new();
            foreach (ProductGroup group in FilteredArchetypesListBox.SelectedItems)
            {
                groupIds.Add(group.Id);
            }
            Program.Groups = groupIds.Count == 0
                ? null
                : string.Join(";", groupIds);

            List<int> materialIds = new();
            foreach (MaterialInfo material in MaterialsListView.SelectedItems)
            {
                materialIds.Add(material.Id);
            }
            Program.Materials = materialIds.Count == 0 
                ? null 
                : string.Join(";", materialIds);

            List<string> machineKeys = new();
            foreach (string machineKey in MachinesListView.SelectedItems)
            {
                machineKeys.Add(machineKey);
            }
            Program.Machines = machineKeys.Count == 0
                ? null
                : string.Join(";", machineKeys);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyTargeting();

            Program.Notepad = string.IsNullOrWhiteSpace(Program.Notepad)
                ? null
                : Program.Notepad.Trim();

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
            FilteredProductGroups = ProductGroups
                .Where(x => x.ProductId is not null)
                .Where(x => productIds.Contains((int)x.ProductId!))
                .ToList();
        }

        private void Notepad_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextLengthCounter.Text = $"{Program.Notepad?.Length}/1024";
        }
    }
}
