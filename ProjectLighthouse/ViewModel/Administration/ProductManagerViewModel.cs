using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.View.Administration;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;

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


        public ProductManagerViewModel()
        {
            AddProductGroupCmd = new(this);

            LoadData();
        }

        private void LoadData()
        {
            Products = DatabaseHelper.Read<Product>()
                .OrderBy(x => x.Name)
                .Prepend(new() { Name = "Unassigned", Id=-1 })
                .ToList();
            ProductGroups = DatabaseHelper.Read<ProductGroup>()
                .OrderBy(x => x.Name)
                .Prepend(new() { Name = "Unassigned", ProductId=-1, Id=-1 })
                .ToList();
            TurnedProducts = DatabaseHelper.Read<TurnedProduct>()
                .OrderBy(x => x.MaterialId)
                .ThenBy(x => x.ProductName)
                .ToList();

            SelectedProduct = Products.First();
        }

        private void LoadProduct()
        {
            FilteredProductGroups = ProductGroups
                .Where(x => x.ProductId == SelectedProduct.Id)
                .ToList();

            if (FilteredProductGroups.Count > 0)
            {
                SelectedProductGroup = FilteredProductGroups.First();
            }
            OnPropertyChanged(nameof(FilteredProductGroups));
        }

        private void LoadProductGroup()
        {
            FilteredTurnedProducts = TurnedProducts
                .Where(x => (x.GroupId??-1) == SelectedProductGroup.Id
                    && !x.Retired)
                .ToList();
            OnPropertyChanged(nameof(FilteredTurnedProducts));
        }

        public void AddProductGroup()
        {
            AddProductGroupWindow window = new(SelectedProduct) { Owner = App.MainViewModel.MainWindow};
            window.ShowDialog();


            if (!window.SaveExit)
            {
                return;
            }


            int id = SelectedProduct.Id;

            LoadData();
            SelectedProduct = Products.Find(x => x.Id == id);
        }
    }
}
