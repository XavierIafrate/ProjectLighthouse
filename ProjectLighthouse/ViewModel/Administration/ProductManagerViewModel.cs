using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using ProjectLighthouse.Model.Administration;
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
using System.Threading.Tasks;
using System.Windows;

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
        public List<BarStock> Bars { get; set; }
        private List<string> latheFeatures;

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

        private TurnedProduct selectedPart;

        public TurnedProduct SelectedPart
        {
            get { return selectedPart; }
            set
            {
                selectedPart = value;
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

        public Dictionary<MaterialInfo, TimeModel> TimeModels { get; set; }


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
            Products = null;
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
            Bars = DatabaseHelper.Read<BarStock>();

            TurnedProducts = DatabaseHelper.Read<TurnedProduct>()
                .ToList();

            Programs = DatabaseHelper.Read<NcProgram>();

            SelectedProduct = Products.First();

            List<Lathe> lathes = DatabaseHelper.Read<Lathe>().ToList();
            List<string> features = new();
            foreach (Lathe lathe in lathes)
            {
                features.AddRange(lathe.FeatureList);
            }

            features = features.OrderBy(x => x).Distinct().ToList();
            latheFeatures = features;
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

            if (FilteredTurnedProducts.Count == 0)
            {
                return;
            }

            List<TurnedProduct> withCycleTimes = FilteredTurnedProducts.Where(x => x.CycleTime > 0 && !x.HasErrors && x.MaterialId is not null).ToList();

            int[] materials = withCycleTimes.Select(x => (int)x.MaterialId!).Distinct().ToArray();

            ISeries[] series = Array.Empty<ISeries>();

            TimeModels = new();

            double minLength = FilteredTurnedProducts.Min(x => x.MajorLength);
            double maxLength = FilteredTurnedProducts.Max(x => x.MajorLength);

            if (!string.IsNullOrEmpty(SelectedProductGroup.DefaultTimeCode))
            {
                TimeModels.Add(new() { Id = -1, MaterialText = "Any", GradeText = "-" }, new(SelectedProductGroup.DefaultTimeCode));
                string seriesName = "Archetype Default";

                series = series.Append(TimeModel.GetSeries(new(SelectedProductGroup.DefaultTimeCode), maxLength, seriesName)).ToArray();
            }

            // overall length or just major?
            for (int i = 0; i < materials.Length; i++)
            {
                int material = materials[i];
                List<TurnedProduct> withCycleTimesInMaterial = withCycleTimes.Where(x => x.MaterialId == material && !x.IsSpecialPart && !x.Retired).ToList();
                ObservableCollection<ObservablePoint> cyclePoints = new();
                withCycleTimesInMaterial.ForEach(x => cyclePoints.Add(new(x.MajorLength, x.CycleTime)));
                MaterialInfo? m = Materials.Find(x => x.Id == material);
                if (m is null) continue;

                TimeModel timeModel;

                try
                {
                    timeModel = OrderResourceHelper.GetCycleResponse(withCycleTimesInMaterial);
                }
                catch
                {
                    continue;
                }
                TimeModels.Add(m, timeModel);

                string seriesName = $"{(m == null ? "N/A" : m.MaterialCode)} Cycle Response Model";

                LineSeries<ObservablePoint> timeModelSeries = TimeModel.GetSeries(timeModel, maxLength, seriesName);
                series = series.Append(timeModelSeries).ToArray();

                series = series.Append(new ScatterSeries<ObservablePoint>
                {
                    Values = cyclePoints,
                    Name = "Cycle Response",
                    TooltipLabelFormatter = (chartPoint) =>
                    $"{(m == null ? "N/A" : m.MaterialCode)} {chartPoint.SecondaryValue}: {chartPoint.PrimaryValue}",
                }).ToArray();

            }

            CycleResponseSeries = series;
            OnPropertyChanged(nameof(TimeModels));

            Task.Run(() => CostFilteredTurnedProducts());
        }

        void CostFilteredTurnedProducts()
        {
            foreach (TurnedProduct product in FilteredTurnedProducts)
            {
                if (SelectedProductGroup is null) return;
                MaterialInfo? material = Materials.Find(x => x.Id == product.MaterialId);
                if (material is null)
                {
                    continue;
                }

                BarStock? bar = SelectedProductGroup.GetRequiredBarStock(Bars, material.Id);
                if (bar is null)
                {
                    continue;
                }

                TimeModel model;
                try
                {
                    model = TimeModels[material];
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Not enough data" && product.CycleTime > 0)
                    {
                        model = new() { Floor = product.CycleTime };
                    }
                    else
                    {
                        continue;
                    }
                }
                double materialBudget = product.MajorLength + product.PartOffLength + 2;
                product.ItemCost = new TurnedProduct.Cost(material, bar, model, product.MajorLength, materialBudget);

            }
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
                window = new(SelectedProduct, Products, latheFeatures) { Owner = App.MainViewModel.MainWindow };
            }
            else
            {
                window = new(SelectedProduct, g, Products, latheFeatures) { Owner = App.MainViewModel.MainWindow };
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

            AddProductWindow window = new(latheFeatures, p) { Owner = App.MainViewModel.MainWindow };

            window.ShowDialog();


            if (!window.SaveExit)
            {
                return;
            }


            int id = window.Product.Id;

            LoadData();
            Search();

            SelectedProduct = FilteredProducts.Find(x => x.Id == id);
            if (SelectedProduct is null && FilteredProducts.Count > 0)
            {
                SelectedProduct = FilteredProducts.First();
            }
        }
    }
}
