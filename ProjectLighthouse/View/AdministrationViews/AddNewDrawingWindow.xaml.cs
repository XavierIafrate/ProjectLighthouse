using Microsoft.Win32;
using ProjectLighthouse.Model;
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
            string bin = (bool)ArchetypeCheckbox.IsChecked ? @"Drawings\" : @"Drawings\Specials\";
            int newRevision = GetRevision(NewDrawing.Product);
            NewDrawing.Revision = newRevision;
            NewDrawing.URL = bin + $"{NewDrawing.Product.ToUpperInvariant()}_R{newRevision}.pdf";
            destinationFilePath = Path.Combine(App.ROOT_PATH, NewDrawing.URL);
        }

        private bool ImprintData()
        {
            SetNewFileNames();

            try
            {
                File.Copy(targetFilePath, destinationFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                return false;
            }

            NewDrawing.Created = DateTime.Now;
            NewDrawing.CreatedBy = App.CurrentUser.GetFullName();

            return DatabaseHelper.Insert(NewDrawing);
        }

        private void ProductGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            NewDrawing.Product = comboBox.SelectedValue.ToString();
            NewDrawing.IsArchetype = true;
            NewDrawing.Customer = "";
        }

        private void SearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            TurnedProduct selectedProduct = listBox.SelectedValue as TurnedProduct;
            NewDrawing.Product = selectedProduct.ProductName;
            NewDrawing.IsArchetype = false;
            NewDrawing.Customer = selectedProduct.CustomerRef;
        }

        private int GetRevision(string product)
        {
            List<TechnicalDrawing> otherDrawings = Drawings.Where(d => d.Product == product).ToList();
            if (otherDrawings.Count == 0)
            {
                return 1;
            }
            else
            {
                return otherDrawings.OrderByDescending(d => d.Revision).First().Revision + 1;
            }
        }

        private bool DataIsValid()
        {
            return !string.IsNullOrEmpty(targetFilePath) && !string.IsNullOrEmpty(NewDrawing.Product);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataIsValid())
            {
                MessageBox.Show($"Creating {NewDrawing.Product}, revision {GetRevision(NewDrawing.Product)}");
                ImprintData();
                SaveExit = true;
                
                Close();
            }
        }
    }
}
