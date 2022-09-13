using Microsoft.Win32;
using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    public partial class AddNewDrawingWindow : Window
    {
        public TechnicalDrawing NewDrawing = new();

        private List<TechnicalDrawing> Drawings;

        string targetFilePath;
        string destinationFilePath;

        private List<TurnedProduct> Products;
        private List<string> ProductFamilies;

        public bool SaveExit;

        public AddNewDrawingWindow(List<TechnicalDrawing> drawings)
        {
            InitializeComponent();
            Products = DatabaseHelper.Read<TurnedProduct>();
            Drawings = drawings;

            ProductFamilies = Products.Where(p => !p.isSpecialPart).Select(p => p.ProductName[..5]).Distinct().OrderBy(f => f).ToList();
            ProductGroupComboBox.ItemsSource = ProductFamilies;

            MaterialConstraintComboBox.ItemsSource = new List<string>().Prepend("Any Material");
            MaterialConstraintComboBox.SelectedItem = MaterialConstraintComboBox.Items[0];
            MaterialConstraintComboBox.IsEnabled = false;

            ToolingGroupConstraintComboBox.ItemsSource = new List<string>().Prepend("All Tooling Groups");
            ToolingGroupConstraintComboBox.SelectedItem = ToolingGroupConstraintComboBox.Items[0];

            ChooseArchetypeControls.Visibility = Visibility.Collapsed;
        }

        private void ArchetypeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            ChooseArchetypeControls.Visibility = Visibility.Visible;
            FindProductControls.Visibility = Visibility.Collapsed;
        }

        private void ArchetypeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            ChooseArchetypeControls.Visibility = Visibility.Collapsed;
            FindProductControls.Visibility = Visibility.Visible;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchString = SearchBox.Text.Trim().ToLowerInvariant();
            SearchResults.ItemsSource = Products
                .Where(p => p.ProductName.ToLowerInvariant().Contains(searchString))
                .ToList();
        }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            string openDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.InitialDirectory = openDir;

            if ((bool)openFileDialog.ShowDialog())
            {
                targetFilePath = openFileDialog.FileName;
                FilePathDisplayText.Text = Path.GetFileName(openFileDialog.FileName);
            }
        }

        private void SetNewFileNames()
        {
            string bin = @"lib\";
            NewDrawing.Revision = 0;
            NewDrawing.DrawingStore = bin + Path.GetRandomFileName();
            destinationFilePath = Path.Combine(App.ROOT_PATH, NewDrawing.DrawingStore);
        }

        private bool ImprintData()
        {
            SetNewFileNames();

            if (!NewDrawing.IsArchetype)
            {
                NewDrawing.MaterialConstraint = "";
            }

            try
            {
                File.Copy(targetFilePath, destinationFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                return false;
            }

            NewDrawing.DrawingType = (bool)researchCheckBox.IsChecked
                ? TechnicalDrawing.Type.Research
                : TechnicalDrawing.Type.Production;

            NewDrawing.IssueDetails = revisionInfoTextBox.Text.Trim();

            NewDrawing.Created = DateTime.Now;
            NewDrawing.CreatedBy = App.CurrentUser.GetFullName();

            List<User> ToNotify = DatabaseHelper.Read<User>().Where(x => x.CanApproveDrawings && x.UserName != App.CurrentUser.UserName).ToList();

            for (int i = 0; i < ToNotify.Count; i++)
            {
                DatabaseHelper.Insert<Notification>(new(to: ToNotify[i].UserName, from: App.CurrentUser.UserName, header: $"Drawing proposal - {NewDrawing.DrawingName}", body: $"{App.CurrentUser.FirstName} has submitted a proposal for {NewDrawing.DrawingName}, please approve or reject.", toastAction: $"viewDrawing:{NewDrawing.DrawingName}"));
            }

            return DatabaseHelper.Insert(NewDrawing);
        }

        private void ProductGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NewDrawing.IsArchetype = true;
            NewDrawing.Customer = "";

            MaterialConstraintComboBox.SelectedItem = MaterialConstraintComboBox.Items[0];
            ToolingGroupConstraintComboBox.SelectedItem = ToolingGroupConstraintComboBox.Items[0];

            SetConstraints();
        }

        private void SetConstraints()
        {
            if (ProductGroupComboBox.SelectedValue == null) return;

            List<TurnedProduct> possibilities = Products.Where(x => x.ProductName[..5] == ProductGroupComboBox.SelectedValue.ToString() && !x.isSpecialPart).ToList();

            if (ToolingGroupConstraintComboBox.SelectedValue.ToString() != "All Tooling Groups")
            {
                NewDrawing.ToolingGroup = ToolingGroupConstraintComboBox.SelectedValue.ToString();
                possibilities = possibilities.Where(x => x.ProductGroup == NewDrawing.ToolingGroup).ToList();
            }
            else
            {
                NewDrawing.ToolingGroup = "";
            }

            if (MaterialConstraintComboBox.SelectedValue.ToString() != "Any Material")
            {
                NewDrawing.MaterialConstraint = MaterialConstraintComboBox.SelectedValue.ToString();
                possibilities = possibilities.Where(x => x.Material == NewDrawing.MaterialConstraint).ToList();
            }
            else
            {
                NewDrawing.MaterialConstraint = "";
            }

            PreviewBox.ItemsSource = possibilities.ToList();

            NewDrawing.ProductGroup = ProductGroupComboBox.SelectedValue.ToString();

            if (ToolingGroupConstraintComboBox.SelectedValue.ToString() == "All Tooling Groups")
            {
                ToolingGroupConstraintComboBox.ItemsSource = possibilities.Select(x => x.ProductGroup).OrderBy(x => x).Distinct().ToList().Prepend("All Tooling Groups");
                NewDrawing.DrawingName = ProductGroupComboBox.SelectedValue.ToString();
                MaterialConstraintComboBox.SelectedItem = MaterialConstraintComboBox.Items[0];
                MaterialConstraintComboBox.IsEnabled = false;
            }
            else
            {
                NewDrawing.DrawingName = ToolingGroupConstraintComboBox.SelectedValue.ToString();
                MaterialConstraintComboBox.IsEnabled = true;
            }

            if (MaterialConstraintComboBox.SelectedValue.ToString() == "Any Material")
            {
                MaterialConstraintComboBox.ItemsSource = possibilities.Select(x=>x.Material).OrderBy(x => x).Distinct().ToList().Prepend("Any Material");
            }
            else
            {
                NewDrawing.DrawingName += $"-{MaterialConstraintComboBox.SelectedValue}";
            }
        }

        private void SearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            TurnedProduct selectedProduct = listBox.SelectedValue as TurnedProduct;
            NewDrawing.DrawingName = selectedProduct.ProductName;
            NewDrawing.IsArchetype = false;
            NewDrawing.Customer = selectedProduct.CustomerRef;
        }

        private bool DataIsValid()
        {
            return !string.IsNullOrEmpty(targetFilePath) && !string.IsNullOrEmpty(NewDrawing.DrawingName) && !string.IsNullOrEmpty(revisionInfoTextBox.Text.Trim());
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataIsValid())
            {
                ImprintData();
                SaveExit = true;
                Close();
            }
        }

        private void MaterialConstraintComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetConstraints();
        }

        private void ToolingGroupConstraintComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetConstraints();
        }

    }
}
