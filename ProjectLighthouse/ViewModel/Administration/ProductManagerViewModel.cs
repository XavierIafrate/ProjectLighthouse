using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using PdfSharp.Pdf;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.View.Administration;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public List<NcProgram> Programs { get; set; }
        public List<MaterialInfo> Materials { get; set; }

        public List<Product> FilteredProducts { get; set; }
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

        private string searchText;

        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                Search();
                OnPropertyChanged();
            }
        }

        private ISeries[] cycleResponseSeries;

        public ISeries[] CycleResponseSeries
        {
            get { return cycleResponseSeries; }
            set { cycleResponseSeries = value; OnPropertyChanged(); }
        }

        public Axis[] XStartAtZero { get; set; } =
        {
            new Axis
            {
                MinLimit=0
            }
        };

        public Axis[] YStartAtZero { get; set; } =
        {
            new Axis
            {
                MinLimit=0
            }
        };



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
            Search();
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

            Materials = DatabaseHelper.Read<MaterialInfo>();

            TurnedProducts = DatabaseHelper.Read<TurnedProduct>()
                .ToList();

            Programs = DatabaseHelper.Read<NcProgram>();

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

            List<TurnedProduct> withCycleTimes = FilteredTurnedProducts.Where(x => x.CycleTime > 0 && !x.HasErrors && x.MaterialId is not null).ToList();

            int[] materials = withCycleTimes.Select(x => (int)x.MaterialId!).Distinct().ToArray();    

            ISeries[] series = Array.Empty<ISeries>();



            // overall length or just major?
            for(int i = 0; i < materials.Length; i++)
            {
                int material = materials[i];
                List<TurnedProduct> withCycleTimesInMaterial = withCycleTimes.Where(x => x.MaterialId ==  material).ToList();   
                ObservableCollection<ObservablePoint> cyclePoints = new();
                withCycleTimesInMaterial.ForEach(x => cyclePoints.Add(new(x.MajorLength, x.CycleTime)));

                MaterialInfo? m = Materials.Find(x => x.Id == material);

                series = series.Append(new ScatterSeries<ObservablePoint> { Values=cyclePoints,
                    Name = "Cycle Response",
                    TooltipLabelFormatter = (chartPoint) =>
                    $"{(m == null ? "N/A" : m.MaterialCode)} {chartPoint.SecondaryValue}: {chartPoint.PrimaryValue}",
                }).ToArray();

                (TimeModel timeModel, double r2) = OrderResourceHelper.GetCycleResponse(withCycleTimesInMaterial);
                ObservableCollection<ObservablePoint> cyclePointsBF = new();

                double minLength = FilteredTurnedProducts.Min(x => x.MajorLength);
                double maxLength = FilteredTurnedProducts.Max(x => x.MajorLength);

                if (timeModel.Floor <= timeModel.Intercept)
                {
                    cyclePointsBF.Add(new(0, timeModel.Intercept));
                }
                else
                {
                    cyclePointsBF.Add(new(0, timeModel.Floor));
                    cyclePointsBF.Add(new((timeModel.Floor - timeModel.Intercept) / timeModel.Gradient, timeModel.Floor));
                }

                
                cyclePointsBF.Add(new(maxLength, timeModel.Gradient * maxLength + timeModel.Intercept));


                series = series.Append(new LineSeries<ObservablePoint>
                {
                    Values = cyclePointsBF,
                    Name = $"{(m == null ? "N/A" : m.MaterialCode)} Cycle Response Model",
                    Fill=null,
                    LineSmoothness=0,
                    GeometrySize=0,
                    TooltipLabelFormatter = (chartPoint) =>
                    $"{(m == null ? "N/A" : m.MaterialCode)} m:{timeModel.Gradient:0.00} c:{timeModel.Intercept:0.00} f:{timeModel.Floor:0}",
                }).ToArray();
            }

            CycleResponseSeries = series;
        }

        

        void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredProducts = new(Products);

                if (FilteredProducts.Count > 0)
                {
                    SelectedProduct = FilteredProducts.First();
                }
                else
                {
                    SelectedProduct = null;
                }
                return;
            }

            string token = SearchText.ToUpperInvariant();
            FilteredProducts = Products.Where(x => x.Name.ToUpperInvariant().Contains(token) || x.Description.ToUpperInvariant().Contains(token)).ToList();
            OnPropertyChanged(nameof(FilteredProducts));
            if (FilteredProducts.Count > 0)
            {
                SelectedProduct = FilteredProducts.First();
            }
            else
            {
                SelectedProduct = null;
            }
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


            AddProductGroupWindow window;


            if (g is null)
            {
                window = new(SelectedProduct, Products) { Owner = App.MainViewModel.MainWindow };
            }
            else
            {
                window = new(SelectedProduct, g, Products) { Owner = App.MainViewModel.MainWindow };
            }


            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            int id = SelectedProduct.Id;
            LoadData();
            SelectedProduct = FilteredProducts.Find(x => x.Id == id);

            if (SelectedProduct is null && FilteredProducts.Count > 0)
            {
                SelectedProduct = FilteredProducts.First();
            }
        }

        public void AddTurnedProduct()
        {
            AddTurnedProductWindow window = new(ProductGroups, groupId: SelectedProductGroup.Id) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            TurnedProducts.Add(window.Product);
            LoadProductGroup();
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

            SelectedProduct = FilteredProducts.Find(x => x.Id == selectedProductId);
            if (SelectedProduct is null && FilteredProducts.Count > 0)
            {
                SelectedProduct = FilteredProducts.First();
                return;
            }
            SelectedProductGroup = FilteredProductGroups.Find(x => x.Id == selectedGroupId);
        }

        public void ViewWebPage()
        {
            if (string.IsNullOrEmpty(SelectedProduct.WebUrl))
            {
                MessageBox.Show("No URL on record", "Failed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

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

            SelectedProduct = FilteredProducts.Find(x => x.Id == id);
            if (SelectedProduct is null && FilteredProducts.Count > 0)
            {
                SelectedProduct = FilteredProducts.First();
            }
        }
    }
}
