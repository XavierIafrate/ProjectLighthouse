using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.CodeDom;
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
                OnPropertyChanged();
            }
        }

        private int MaterialId;


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

            ProductGroup? targetGroup = ProductGroups.Find(x => x.Id == requiredGroup);

            if (targetGroup is null)
            {
                throw new System.ArgumentNullException("Target group is null");
            }

            SelectedProduct = Products.Find(x => x.Id == targetGroup.ProductId);
            SelectedGroup = targetGroup;

        }

        public OrderConstructorWindow(int requiredGroup, int materialId, (TurnedProduct, int, DateTime) required)
        {
            InitializeComponent();

            ProductsControlGroup.Visibility = Visibility.Collapsed;
            GroupsLabel.Visibility = Visibility.Collapsed;
            GroupsListBox.Visibility = Visibility.Collapsed;

            LoadData();

            ProductGroup? targetGroup = ProductGroups.Find(x => x.Id == requiredGroup);

            if (targetGroup is null)
            {
                throw new Exception("Target group is null");
            }


            SelectedProduct = Products.Find(x => x.Id == targetGroup.ProductId);
            SelectedGroup = targetGroup;


            TurnedProduct product = required.Item1;

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

            NewOrderItems.Add(new(product, quantity, date));

            TurnedProduct toRemove = AvailableTurnedProducts.ToList().Find(x => x.Id == product.Id);

            if (toRemove == null)
            {
                return;
            }

            AvailableTurnedProducts.Remove(toRemove);

            for (int j = AvailableTurnedProducts.Count - 1; j >= 0; j--)
            {
                MaterialId = (int)product.MaterialId;

                if (AvailableTurnedProducts[j].MaterialId != product.MaterialId)
                {
                    AvailableTurnedProducts.RemoveAt(j);
                }
                //else if (NewOrderItems.Any(x => x.ProductId == AvailableTurnedProducts[j].Id))
                //{
                //    object test = AvailableItemsListBox.ItemsSource. disable stuff?
                //}

            }
        }

        private void LoadData()
        {
            Products = DatabaseHelper.Read<Product>();
            ProductGroups = DatabaseHelper.Read<ProductGroup>();
            TurnedProducts = DatabaseHelper.Read<TurnedProduct>();
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
                .OrderBy(x => x.ProductName)
                .ThenBy(x => !x.IsSpecialPart)
                .ToList()
                .ForEach(x => AvailableTurnedProducts.Add(x));
        }

        public void SetConfirmButtonsVisibility(Visibility vis)
        {
            Footer.Visibility = vis;
        }

        private void CalculateInsights()
        {
            //if (NewOrderItems.Count == 0)
            //{
            //    InsightsStackPanel.Visibility = Visibility.Collapsed;
            //    NoInsights.Visibility = Visibility.Visible;
            //    return;
            //}
            //else
            //{
            //    InsightsStackPanel.Visibility = Visibility.Visible;
            //    NoInsights.Visibility = Visibility.Collapsed;
            //}

            //Insights.BarId = NewOrder.BarID;
            //BarStock bar = Bars.Find(x => x.Id == NewOrder.BarID);
            //Insights.BarPrice = bar.Cost;

            //double numBars = 0;
            //int time = 3600 * 4;
            //int value = 0;
            //Insights.TimeIsEstimate = false;

            //List<LatheManufactureOrderItem> itemsWithCycleTime = NewOrderItems.Where(x => x.CycleTime != 0).ToList();

            //for (int i = 0; i < NewOrderItems.Count; i++)
            //{
            //    double partBudget = NewOrderItems[i].MajorLength + 2;
            //    double availablePerBar = bar.Length - 300;
            //    double partsPerBar = Math.Floor(availablePerBar / partBudget);
            //    numBars += NewOrderItems[i].TargetQuantity / partsPerBar;

            //    value += (int)(NewOrderItems[i].SellPrice * NewOrderItems[i].TargetQuantity * 0.7);

            //    if (NewOrderItems[i].CycleTime > 0)
            //    {
            //        time += NewOrderItems[i].CycleTime * NewOrderItems[i].TargetQuantity;
            //    }
            //    else
            //    {
            //        time += NewOrderItems[i].GetCycleTime() * NewOrderItems[i].TargetQuantity;
            //        Insights.TimeIsEstimate = true;
            //    }
            //}

            //Insights.TimeToComplete = time;
            //Insights.NumberOfBarsRequired = numBars;
            //Insights.TotalBarCost = numBars * bar.Cost;

            //time = Math.Max(time, 3600 * 24);

            //TimeSpan timeToComplete = TimeSpan.FromSeconds(time);
            //Insights.CostOfMachineTime = timeToComplete.TotalMinutes * 0.45;

            //Insights.CostOfOrder = Insights.CostOfMachineTime + (Insights.TotalBarCost / 100);
            //Insights.ValueProduced = value / 100;
            //Insights.NetProfit = Insights.ValueProduced - Insights.CostOfOrder;

            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Insights)));
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableItemsListBox.SelectedValue is not TurnedProduct product) return;


            if (product.MaterialId is null)
            {
                MessageBox.Show("Material ID for selected product is not set.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MaterialId = (int)product.MaterialId;

            NewOrderItems.Add(new(product));
            AvailableTurnedProducts.Remove(product);


            for (int i = AvailableTurnedProducts.Count - 1; i >= 0; i--)
            {
                if (AvailableTurnedProducts[i].MaterialId != product.MaterialId)
                {
                    AvailableTurnedProducts.RemoveAt(i);
                }
            }
        }


        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewOrderItemsListView.SelectedValue is not LatheManufactureOrderItem item) return;

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

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewOrderItems.Count == 0)
            {
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool CreateManufactureOrder()
        {
            NewOrder.Name = GetNewOrderId();
            NewOrder.CreatedAt = DateTime.Now;
            NewOrder.CreatedBy = App.CurrentUser.GetFullName();

            if (SelectedGroup is null)
            {
                throw new Exception("Selected group is null, cannot proceed.");
            }

            ProductGroup group = SelectedGroup;
            NewOrder.MajorDiameter = group.MajorDiameter;

            List<BarStock> potentialBar = BarStock
                .Where(x => x.MaterialId == MaterialId && x.Size >= SelectedGroup.GetRequiredBarSize())
                .OrderBy(x => x.Size)
                .ToList();

            if (potentialBar.Count == 0)
            {
                throw new Exception("No bar records available that meet the size or material requirements for the product.");
            }

            BarStock orderBar = potentialBar.First();

            NewOrder.BarsInStockAtCreation = orderBar.InStock;
            NewOrder.MaterialId = MaterialId;
            NewOrder.GroupId = SelectedGroup.Id;
            NewOrder.NumberOfBars = NewOrderItems.CalculateNumberOfBars(orderBar, 0);

            List<TurnedProduct> comparableProducts = TurnedProducts
                .Where(x => x.GroupId == SelectedGroup.Id 
                    && x.MaterialId == MaterialId 
                    && x.CycleTime > 0)
                .ToList();


            int? defaultCycleTime = null;
            if (comparableProducts.Count > 0)
            {
                defaultCycleTime = comparableProducts.Min(x => x.CycleTime);
            }

            (int totalTime, int targetCycleTime, bool estimated) = NewOrderItems.CalculateOrderRuntime(defaultCycleTime);

            NewOrder.TimeToComplete = totalTime;
            NewOrder.TargetCycleTime = targetCycleTime;
            NewOrder.TargetCycleTimeEstimated = estimated;

            List<TechnicalDrawing> allDrawings = DatabaseHelper.Read<TechnicalDrawing>().Where(x => x.DrawingType == TechnicalDrawing.Type.Production).ToList();
            List<TechnicalDrawing> drawings = TechnicalDrawing.FindDrawings(allDrawings, NewOrderItems.ToList(), NewOrder.GroupId, NewOrder.MaterialId);

            _ = DatabaseHelper.Insert(NewOrder);

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
    }
}
