using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.View.Administration;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using ViewModel.Commands.Administration;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class ProductManagerViewModel : BaseViewModel
    {
        public List<Product> Products { get; set; }
        public List<ProductGroup> ProductGroups { get; set; }
        public List<TechnicalDrawing> TechnicalDrawings { get; set; }
        public List<TurnedProduct> TurnedProducts { get; set; }


        public List<ProductGroup> FilteredProductGroups { get; set; }
        public List<TurnedProduct> FilteredTurnedProducts { get; set; }

        private Product selectedProduct;
        public Product SelectedProduct
        {
            get { return selectedProduct; }
            set
            {
                selectedProduct = value;
                LoadProduct();
                OnPropertyChanged();
            }
        }

        private ProductGroup selectedProductGroup;
        public ProductGroup SelectedProductGroup
        {
            get { return selectedProductGroup; }
            set
            {
                selectedProductGroup = value;
                LoadProductGroup();
                OnPropertyChanged();
            }
        }

        public AddProductGroupCommand AddProductGroupCmd { get; set; }
        public EditTurnedProductCommand EditTurnedProductCmd { get; set; }
        public AddTurnedProductCommand AddTurnedProductCmd { get; set; }
        public ViewWebPageCommand ViewWebPageCmd { get; set; }
        public AddProductCommand AddProductCmd { get; set; }



        public ProductManagerViewModel()
        {
            AddProductGroupCmd = new(this);
            AddTurnedProductCmd = new(this);
            EditTurnedProductCmd = new(this);
            ViewWebPageCmd = new(this);
            AddProductCmd = new(this);

            LoadData();
        }

        private void LoadData()
        {
            Products = DatabaseHelper.Read<Product>()
                .OrderBy(x => x.Name)
                .Prepend(new() { Id = -1, Name = "Unassigned", Description = "Incomplete products/groups" })
                .ToList();
            OnPropertyChanged(nameof(Products));

            ProductGroups = DatabaseHelper.Read<ProductGroup>()
                .OrderBy(x => x.Name)
                .Prepend(new() { Id = -1, Name = "Unassigned", ProductId = -1 })
                .ToList();
            OnPropertyChanged(nameof(ProductGroups));

            TurnedProducts = DatabaseHelper.Read<TurnedProduct>()
                .ToList();

            SelectedProduct = Products.First();
        }

        private void LoadProduct()
        {
            if (SelectedProduct is null)
            {
                FilteredProductGroups = new();
                OnPropertyChanged(nameof(FilteredProductGroups));
                SelectedProductGroup = null;
                return;
            }

            FilteredProductGroups = ProductGroups
                .Where(x => (x.ProductId ?? -1) == SelectedProduct.Id)
                .ToList();
            OnPropertyChanged(nameof(FilteredProductGroups));

            if (FilteredProductGroups.Count > 0)
            {
                SelectedProductGroup = FilteredProductGroups.First();
            }
        }

        private void LoadProductGroup()
        {
            if (SelectedProductGroup is null)
            {
                FilteredTurnedProducts = new();
                OnPropertyChanged(nameof(FilteredTurnedProducts));
                return;
            }

            FilteredTurnedProducts = TurnedProducts
                .Where(x => (x.GroupId ?? -1) == SelectedProductGroup.Id)
                .OrderBy(x => x.Retired)
                .ThenBy(x => x.MaterialId)
                .ThenBy(x => x.IsSpecialPart)
                .ThenBy(x => x.ProductName)
                .ToList();

            for (int i = 0; i < FilteredTurnedProducts.Count; i++)
            {
                FilteredTurnedProducts[i].ValidateAll();
            }

            OnPropertyChanged(nameof(FilteredTurnedProducts));
        }

        public void AddProductGroup(ProductGroup? g)
        {
            if (g is not null)
            {
                if (g.Id == -1)
                {
                    MessageBox.Show("The selected group cannot be edited.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }


            AddProductGroupWindow window = new(SelectedProduct, g) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            int id = SelectedProduct.Id;
            LoadData();
            SelectedProduct = Products.Find(x => x.Id == id);
        }

        public void AddTurnedProduct()
        {
            AddTurnedProductWindow window = new(ProductGroups) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            TurnedProducts.Add(window.Product);
            FilteredTurnedProducts.Add(window.Product);
            OnPropertyChanged(nameof(FilteredTurnedProducts));
        }

        public void EditTurnedProduct(int id)
        {
            TurnedProduct? p = FilteredTurnedProducts.Find(x => x.Id == id);

            if (p is null)
            {
                MessageBox.Show($"TurnedProduct with id {id:0} not found in list.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AddTurnedProductWindow window = new(ProductGroups, p) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            int selectedProductId = SelectedProduct.Id;
            int selectedGroupId = SelectedProductGroup.Id;

            LoadData();

            SelectedProduct = Products.Find(x => x.Id == selectedProductId);
            SelectedProductGroup = FilteredProductGroups.Find(x => x.Id == selectedGroupId);
        }

        public void ViewWebPage()
        {
            Process.Start(new ProcessStartInfo(SelectedProduct.WebUrl) { UseShellExecute = true });
        }

        public void CreateProduct(Product? p = null)
        {

            if (p is not null)
            {
                if (p.Id == -1)
                {
                    MessageBox.Show("The selected product cannot be edited.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            AddProductWindow window = new(p) { Owner = App.MainViewModel.MainWindow };

            window.ShowDialog();


            if (!window.SaveExit)
            {
                return;
            }


            int id = window.Product.Id;

            LoadData();

            SelectedProduct = Products.Find(x => x.Id == id);
        }
    }
}
