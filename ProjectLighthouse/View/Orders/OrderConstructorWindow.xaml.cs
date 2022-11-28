using Model.Products;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProjectLighthouse.View.Orders
{
    public partial class OrderConstructorWindow : Window, INotifyPropertyChanged
    {
        public List<Product> Products { get; set; }
        public List<ProductGroup> ProductGroups { get; set; }
        public List<TurnedProduct> TurnedProducts { get; set; }


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


        private List<TurnedProduct> availableTurnedProducts;
        public List<TurnedProduct> AvailableTurnedProducts
        {
            get { return availableTurnedProducts; }
            set 
            {
                availableTurnedProducts = value;
                OnPropertyChanged();
            }
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

        private void LoadData()
        {
            Products = DatabaseHelper.Read<Product>();
            ProductGroups = DatabaseHelper.Read<ProductGroup>();
            TurnedProducts = DatabaseHelper.Read<TurnedProduct>();
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
        }

        private void FilterTurnedProducts()
        {
            if (SelectedGroup is null)
            {
                AvailableTurnedProducts = new();
                return;
            }

            AvailableTurnedProducts = TurnedProducts
                .Where(x => x.GroupId == SelectedGroup.Id)
                .ToList();
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            //if (NewOrderItems.Count == 0)
            //{
            //    return;
            //}
            //CreateManufactureOrder();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void CreateManufactureOrder()
        //{
        //    NewOrder.Name = GetNewOrderId();
        //    NewOrder.CreatedAt = DateTime.Now;
        //    NewOrder.CreatedBy = App.CurrentUser.GetFullName();
        //    NewOrder.MajorDiameter = NewOrderItems.First().MajorDiameter;
        //    NewOrder.BarsInStockAtCreation = Bars.Find(x => x.Id == NewOrder.BarID).InStock;
        //    NewOrder.NumberOfBars = Math.Ceiling(Insights.NumberOfBarsRequired);
        //    NewOrder.ToolingGroup = SelectedToolingGroup;

        //    int time = 0;
        //    List<TurnedProduct> comparableProducts = ProductsInToolingGroup.Where(x => x.CycleTime > 0).ToList();
        //    int lastCycleTime = comparableProducts.Count > 0
        //        ? comparableProducts.Min(x => x.CycleTime)
        //        : 0;
        //    List<LatheManufactureOrderItem> items = NewOrderItems.OrderByDescending(x => x.CycleTime).ToList();
        //    for (int i = 0; i < items.Count; i++)
        //    {
        //        if (NewOrderItems[i].CycleTime > 0)
        //        {
        //            time += NewOrderItems[i].CycleTime * NewOrderItems[i].TargetQuantity;
        //        }
        //        else if (lastCycleTime > 0)
        //        {
        //            time += NewOrderItems[i].GetCycleTime() * NewOrderItems[i].TargetQuantity;
        //        }
        //        else
        //        {
        //            time += NewOrderItems[i].GetCycleTime() * NewOrderItems[i].TargetQuantity;
        //        }

        //        lastCycleTime = NewOrderItems[i].CycleTime;
        //    }
        //    NewOrder.TimeToComplete = time;
        //    items = items.Where(x => x.CycleTime > 0).ToList();

        //    if (items.Count > 0)
        //    {
        //        NewOrder.TargetCycleTime = items.Min(x => x.CycleTime);
        //        NewOrder.TargetCycleTimeEstimated = false;
        //    }
        //    else
        //    {
        //        NewOrder.TargetCycleTime = NewOrderItems[0].GetCycleTime();
        //        NewOrder.TargetCycleTimeEstimated = true;
        //    }

        //    List<TechnicalDrawing> allDrawings = DatabaseHelper.Read<TechnicalDrawing>().Where(x => x.DrawingType == TechnicalDrawing.Type.Production).ToList();
        //    List<TechnicalDrawing> drawings = TechnicalDrawing.FindDrawings(allDrawings, NewOrderItems.ToList(), NewOrder.ToolingGroup);

        //    _ = DatabaseHelper.Insert(NewOrder);
        //    foreach (LatheManufactureOrderItem item in NewOrderItems)
        //    {
        //        if (item.TargetQuantity == 0)
        //        {
        //            continue;
        //        }

        //        item.CycleTime = 0; //force to update if different
        //        if (Request != null)
        //        {
        //            item.NeedsCleaning = Request.CleanCustomerRequirement && item.RequiredQuantity > 0;
        //        }
        //        item.AssignedMO = NewOrder.Name;
        //        item.AddedBy = App.CurrentUser.GetFullName();
        //        item.DateAdded = DateTime.Now;
        //        DatabaseHelper.Insert(item);
        //    };

        //    foreach (TechnicalDrawing drawing in drawings)
        //    {
        //        OrderDrawing o = new() { DrawingId = drawing.Id, OrderId = NewOrder.Name };
        //        DatabaseHelper.Insert(o);
        //    }

        //    Cancelled = false;
        //    Close();
        //}

        //private static string GetNewOrderId()
        //{
        //    List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>();
        //    int nOrders = orders.Count;
        //    string strOrderNum = Convert.ToString(nOrders + 1);
        //    int orderNumLen = strOrderNum.Length;
        //    const string blank = "M00000";

        //    return blank[..(6 - orderNumLen)] + strOrderNum;
        //}
    }
}
