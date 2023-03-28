using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.ViewModel.Helpers;
using ProjectLighthouse.ViewModel.Requests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using ViewModel.Helpers;

namespace ProjectLighthouse.View.Orders
{
    public partial class OrderConstructorWindow : Window, INotifyPropertyChanged
    {
        public List<Product> Products { get; set; }
        public List<ProductGroup> ProductGroups { get; set; }
        public List<TurnedProduct> TurnedProducts { get; set; }
        public List<BarStock> BarStock { get; set; }

        public int? RequestId = null;


        #region Full Properties
        private Product? selectedProduct;
        public Product? SelectedProduct
        {
            get { return selectedProduct; }
            set
            {
                selectedProduct = value;
                FilterGroups();

                OnPropertyChanged();
            }
        }

        private ProductGroup? selectedGroup;
        public ProductGroup? SelectedGroup
        {
            get { return selectedGroup; }
            set
            {
                selectedGroup = value;
                FilterTurnedProducts();
                OnPropertyChanged();
            }
        }

        private List<ProductGroup> availableProductGroups;
        public List<ProductGroup> AvailableProductGroups
        {
            get { return availableProductGroups; }
            set
            {
                availableProductGroups = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<TurnedProduct> availableTurnedProducts = new();
        public ObservableCollection<TurnedProduct> AvailableTurnedProducts
        {
            get { return availableTurnedProducts; }
            set
            {
                availableTurnedProducts = value;
                OnPropertyChanged();
            }
        }


        private LatheManufactureOrder newOrder;
        public LatheManufactureOrder NewOrder
        {
            get { return newOrder; }
            set
            {
                newOrder = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<LatheManufactureOrderItem> newOrderItems = new();

        public ObservableCollection<LatheManufactureOrderItem> NewOrderItems
        {
            get { return newOrderItems; }
            set
            {
                newOrderItems = value;
                CalculateInsights();
                OnPropertyChanged();
            }
        }

        private int MaterialId;

        private NewOrderInsights insights;

        public NewOrderInsights Insights
        {
            get { return insights; }
            set { insights = value; OnPropertyChanged(); }
        }

        #endregion

        public bool SaveExit = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public OrderConstructorWindow()
        {
            InitializeComponent();
            LoadData();
        }

        public OrderConstructorWindow(int requiredGroup, int materialId)
        {
            InitializeComponent();

            ProductsControlGroup.Visibility = Visibility.Collapsed;
            GroupsLabel.Visibility = Visibility.Collapsed;
            GroupsListBox.Visibility = Visibility.Collapsed;

            LoadData();
            MaterialId = materialId;

            ProductGroup? targetGroup = ProductGroups.Find(x => x.Id == requiredGroup) 
                ?? throw new Exception($"{nameof(targetGroup)} is null");
            
            SelectedProduct = Products.Find(x => x.Id == targetGroup.ProductId);
            SelectedGroup = targetGroup;

            CalculateInsights();
        }

        public OrderConstructorWindow(int requiredGroup, int materialId, (TurnedProduct, int, DateTime) required)
        {
            InitializeComponent();

            ProductsControlGroup.Visibility = Visibility.Collapsed;
            GroupsLabel.Visibility = Visibility.Collapsed;
            GroupsListBox.Visibility = Visibility.Collapsed;

            LoadData();
            MaterialId = materialId;

            ProductGroup? targetGroup = ProductGroups.Find(x => x.Id == requiredGroup) 
                ?? throw new Exception("Target group is null");
            
            SelectedProduct = Products.Find(x => x.Id == targetGroup.ProductId);
            SelectedGroup = targetGroup;

            TurnedProduct product = required.Item1;

            product = AvailableTurnedProducts.ToList().Find(x => x.ProductName == product.ProductName);

            if (product is null)
            {
                throw new($"Product not found");
            }

            if (product.MaterialId is null)
            {
                throw new($"Material Id for {product.ProductName} is not set.");
            }

            int quantity = required.Item2;
            DateTime date = required.Item3;

            if (!AvailableTurnedProducts.Any(x => x.Id == product.Id))
            {
                throw new Exception($"{product.ProductName} not in group ID '{requiredGroup}'.");
            }

            AddProductToOrder(product, quantity, date);
            CalculateInsights();
        }

        public OrderConstructorWindow(int requiredGroup, int materialId, List<LatheManufactureOrderItem> recommendation)
        {
            InitializeComponent();

            ProductsControlGroup.Visibility = Visibility.Collapsed;
            GroupsLabel.Visibility = Visibility.Collapsed;
            GroupsListBox.Visibility = Visibility.Collapsed;

            LoadData();
            MaterialId = materialId;


            ProductGroup? targetGroup = ProductGroups.Find(x => x.Id == requiredGroup) 
                ?? throw new Exception("Target group is null");
            
            SelectedProduct = Products.Find(x => x.Id == targetGroup.ProductId);
            SelectedGroup = targetGroup;


            for (int i = 0; i < recommendation.Count; i++)
            {
                TurnedProduct? turnedProduct = AvailableTurnedProducts.ToList().Find(x => x.ProductName == recommendation[i].ProductName);

                if (turnedProduct is null)
                {
                    continue;
                }

                AddProductToOrder(turnedProduct, recommendation[i].RequiredQuantity, recommendation[i].DateRequired, recommendation[i].TargetQuantity);
            }

            CalculateInsights();
        }

        private void LoadData()
        {
            Products = DatabaseHelper.Read<Product>().OrderBy(x => x.Name).ToList();
            ProductGroups = DatabaseHelper.Read<ProductGroup>().OrderBy(x => x.Name).ToList();
            TurnedProducts = DatabaseHelper.Read<TurnedProduct>()
                .OrderBy(x => x.MaterialId)
                .ThenBy(x => x.ProductName)
                .ThenBy(x => !x.IsSpecialPart)
                .ToList();
            BarStock = DatabaseHelper.Read<BarStock>();

            NewOrder = new();
        }

        private void FilterGroups()
        {
            if (SelectedProduct is null)
            {
                AvailableProductGroups = new();
                return;
            }

            AvailableProductGroups = ProductGroups
                .Where(x => x.ProductId == SelectedProduct.Id)
                .ToList();

            NewOrderItems = new();
        }

        private void FilterTurnedProducts()
        {
            if (SelectedGroup is null)
            {
                AvailableTurnedProducts = new();
                return;
            }

            AvailableTurnedProducts.Clear();
            NewOrderItems.Clear();

            TurnedProducts
                .Where(x => x.GroupId == SelectedGroup.Id)
                .ToList()
                .ForEach(x => AvailableTurnedProducts.Add(x));
        }

        public void SetConfirmButtonsVisibility(Visibility vis)
        {
            Footer.Visibility = vis;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableItemsListBox.SelectedValue is not TurnedProduct product) return;

            product.ValidateForOrder();
            if (product.HasErrors)
            {
                MessageBox.Show("Selected product has data errors and cannot be added to the order.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (product.MaterialId is null)
            {
                MessageBox.Show("Material ID for selected product is not set.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MaterialId = (int)product.MaterialId;

            AddProductToOrder(product);

            CalculateInsights();
        }

        private void OnNewItemChanged(object sender, PropertyChangedEventArgs e)
        {
            CalculateInsights();
        }

        private void AddProductToOrder(TurnedProduct product, int requiredQuantity = 0, DateTime? requiredDate = null, int? targetQuantity = null)
        {
            LatheManufactureOrderItem newItem;

            if (requiredDate is not null)
            {
                newItem = new(product, requiredQuantity, (DateTime)requiredDate);
            }
            else if (requiredQuantity > 0)
            {
                newItem = new(product, requiredQuantity);
            }
            else
            {
                newItem = new(product);
            }

            newItem.TargetQuantity = Math.Max(newItem.TargetQuantity, RequestsEngine.GetMiniumumOrderQuantity(newItem));

            if (targetQuantity is not null)
            {
                newItem.TargetQuantity = (int)targetQuantity;
            }

            newItem.PropertyChanged += OnNewItemChanged;

            NewOrderItems.Add(newItem);

            AvailableTurnedProducts.Remove(product);

            for (int i = AvailableTurnedProducts.Count - 1; i >= 0; i--)
            {
                if (AvailableTurnedProducts[i].MaterialId != product.MaterialId)
                {
                    AvailableTurnedProducts.RemoveAt(i);
                }
            }
        }

        private void RemoveItemFromOrder(LatheManufactureOrderItem item)
        {
            if (item.RequiredQuantity > 0)
            {
                return;
            }

            NewOrderItems.Remove(item);
            AvailableTurnedProducts.Add(TurnedProducts.Find(x => x.Id == item.ProductId));

            if (NewOrderItems.Count == 0)
            {
                FilterTurnedProducts();
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewOrderItemsListView.SelectedValue is not LatheManufactureOrderItem item) return;

            RemoveItemFromOrder(item);
            CalculateInsights();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewOrderItems.Count == 0)
            {
                return;
            }

            try
            {
                CheckRequestCanBeApproved();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            try
            {
                CreateManufactureOrder();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveExit = true;
            Close();
        }

        private void CheckRequestCanBeApproved()
        {
            if (RequestId is null)
            {
                return;
            }
            Request? request;

            try
            {
                request = DatabaseHelper.Read<Request>(throwErrs:true).Find(x => x.Id == RequestId);
            }
            catch
            {
                throw new Exception("Error finding request");
            }

            if (request is null)
            {
                throw new Exception("Error finding request");
            }

            if (request.IsAccepted || request.IsDeclined)
            {
                throw new Exception("Request already approved");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool CreateManufactureOrder()
        {
            int failedValidation = 0;
            for (int i = 0; i < NewOrderItems.Count; i++)
            {
                NewOrderItems[i].ValidateAll();
                if (NewOrderItems[i].HasErrors)
                {
                    failedValidation++;
                }
            }


            if (failedValidation > 0)
            {
                throw new Exception($"{failedValidation:0} items have failed validation, cannot proceed.");
            }

            NewOrder.Name = GetNewOrderId();
            NewOrder.CreatedAt = DateTime.Now;
            NewOrder.CreatedBy = App.CurrentUser.GetFullName();

            if (SelectedGroup is null)
            {
                throw new Exception("Selected group is null, cannot proceed.");
            }

            ProductGroup group = SelectedGroup;
            if (group.Status == ProductGroup.GroupStatus.InDevelopment)
            {
                NewOrder.IsResearch = true;
            }

            NewOrder.MajorDiameter = group.MajorDiameter;

            BarStock? orderBar = group.GetRequiredBarStock(BarStock, MaterialId) 
                ?? throw new Exception("No bar records available that meet the size or material requirements for the product.");
            
            NewOrder.BarsInStockAtCreation = orderBar.InStock;
            NewOrder.MaterialId = MaterialId;
            NewOrder.GroupId = SelectedGroup.Id;
            NewOrder.BarID = orderBar.Id;
            NewOrder.NumberOfBars = NewOrderItems.CalculateNumberOfBars(orderBar, 0);

            (int totalTime, int targetCycleTime, bool estimated) = NewOrderItems.CalculateOrderRuntime();

            NewOrder.TimeToComplete = totalTime;
            NewOrder.TargetCycleTime = targetCycleTime;
            NewOrder.TargetCycleTimeEstimated = estimated;

            List<TechnicalDrawing> allDrawings = DatabaseHelper.Read<TechnicalDrawing>()
                .Where(x => x.DrawingType == (NewOrder.IsResearch? TechnicalDrawing.Type.Research : TechnicalDrawing.Type.Production))
                .ToList();
            List<TechnicalDrawing> drawings = TechnicalDrawing.FindDrawings(allDrawings, NewOrderItems.ToList(), NewOrder.GroupId, NewOrder.MaterialId);

            if (!DatabaseHelper.Insert(NewOrder))
            {
                MessageBox.Show("Failed to insert to database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            foreach (TechnicalDrawing drawing in drawings)
            {
                OrderDrawing o = new() { DrawingId = drawing.Id, OrderId = NewOrder.Name };
                DatabaseHelper.Insert(o);
            }

            foreach (LatheManufactureOrderItem item in NewOrderItems)
            {
                if (item.TargetQuantity == 0)
                {
                    continue;
                }

                item.CycleTime = 0; //force to update if different
                item.AssignedMO = NewOrder.Name;
                item.AddedBy = App.CurrentUser.GetFullName();
                item.DateAdded = DateTime.Now;
                DatabaseHelper.Insert(item);
            };
            return true;

        }

        // TODO refactor
        private static string GetNewOrderId()
        {
            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>();
            int nOrders = orders.Count;
            string strOrderNum = Convert.ToString(nOrders + 1);
            int orderNumLen = strOrderNum.Length;
            const string blank = "M00000";

            return blank[..(6 - orderNumLen)] + strOrderNum;
        }

        #region Insights
        private void CalculateInsights()
        {
            if (NewOrderItems.Count == 0)
            {
                Insights = null;
                return;
            }
            else
            {
                Insights = new();
            }

            if (SelectedGroup is null) return;

            NewOrderInsights insights = new();

            BarStock? bar = SelectedGroup.GetRequiredBarStock(BarStock, MaterialId) 
                ?? throw new Exception("No bar found for group");
            
            insights.BarId = bar.Id;
            insights.BarPrice = bar.Cost;
            insights.NumberOfBarsRequired = NewOrderItems.CalculateNumberOfBars(bar, 0);
            insights.TotalBarCost = bar.Cost * insights.NumberOfBarsRequired;

            (int totalTime, _, bool estimated) = NewOrderItems.CalculateOrderRuntime();

            insights.TimeIsEstimate = estimated;
            insights.TimeToComplete = Math.Max(totalTime, 86400);

            TimeSpan orderTime = TimeSpan.FromSeconds(insights.TimeToComplete);

            insights.CostOfMachineTime = orderTime.TotalMinutes * 0.45;

            insights.ValueProduced = (int)NewOrderItems.Sum(x => x.SellPrice * x.TargetQuantity * 0.7) / 100;

            insights.CostOfOrder = insights.CostOfMachineTime + (insights.TotalBarCost / 100);
            insights.NetProfit = insights.ValueProduced - insights.CostOfOrder;

            Insights = insights;
        }


        public class NewOrderInsights
        {
            public bool TimeIsEstimate { get; set; }
            public int TimeToComplete { get; set; }
            public string BarId { get; set; }
            public double BarPrice { get; set; }
            public double NumberOfBarsRequired { get; set; }
            public double TotalBarCost { get; set; }
            public double CostOfOrder { get; set; }
            public double CostOfMachineTime { get; set; }
            public double NetProfit { get; set; }
            public double ValueProduced { get; set; }
        }

    }
    #endregion
}
