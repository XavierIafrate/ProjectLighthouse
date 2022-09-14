using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.View
{
    public partial class LMOContructorWindow : Window, INotifyPropertyChanged
    {
        private List<TurnedProduct> Products;
        public string[] ToolingGroups { get; set; }
        private string selectedToolingGroup;

        public string SelectedToolingGroup
        {
            get { return selectedToolingGroup; }
            set
            {
                selectedToolingGroup = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProductsInToolingGroup)));
                NewOrderItems.Clear();
                RefreshView();
            }
        }

        private ObservableCollection<TurnedProduct> productsInToolingGroup = new();
        public ObservableCollection<TurnedProduct> ProductsInToolingGroup
        {
            get { return productsInToolingGroup; }
            set
            {
                productsInToolingGroup = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProductsInToolingGroup)));
            }
        }

        private ObservableCollection<LatheManufactureOrderItem> newOrderItems = new();
        public ObservableCollection<LatheManufactureOrderItem> NewOrderItems
        {
            get { return newOrderItems; }
            set
            {
                newOrderItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewOrderItems)));
            }
        }

        public LatheManufactureOrder NewOrder { get; set; } = new();

        private NewOrderInsights insights = new();
        public NewOrderInsights Insights
        {
            get { return insights; }
            set
            {
                insights = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Insights)));
            }
        }


        private Request Request;
        private TurnedProduct RequiredProduct;
        public bool Cancelled = true;
        private List<BarStock> Bars;

        public event PropertyChangedEventHandler PropertyChanged;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public LMOContructorWindow(Request? request, List<LatheManufactureOrderItem>? preselectedItems, bool withAuthority = true)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            InitializeComponent();

            DataContext = this;
            Products = DatabaseHelper.Read<TurnedProduct>();
            Bars = DatabaseHelper.Read<BarStock>();

            ProductsInToolingGroup = new(Products);

            ToolingGroups = Products
                .Where(x => !x.isSpecialPart)
                .Select(x => x.ProductGroup)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            ApproveButton.IsEnabled = withAuthority;
            Footer.Visibility = withAuthority ? Visibility.Visible : Visibility.Collapsed;
            if (!withAuthority)
            {
                TitleText.Text = $"Request #{request.Id}: Make or Buy";
                this.Title = $"Make or Buy Helper";
            }

            if (request != null)
            {
                Request = request;
                RequiredProduct = Products.Find(x => x.ProductName == Request.Product);
                if (!RequiredProduct.isSpecialPart)
                {
                    Products = Products.Where(x => !x.isSpecialPart).ToList();
                }

                SelectedToolingGroup = RequiredProduct.ProductGroup;

                if (preselectedItems != null)
                {
                    for (int i = 0; i < preselectedItems.Count; i++)
                    {
                        LatheManufactureOrderItem newItem = preselectedItems[i];
                        newItem.EditMade += CalculateInsights;
                        NewOrderItems.Add(newItem);
                    }
                }
                else
                {
                    NewOrderItems.Add(new LatheManufactureOrderItem(RequiredProduct));
                }
                NewOrder.BarID = RequiredProduct.BarID;
                ChooseToolingGroupControls.Visibility = Visibility.Collapsed;
            }
            else
            {
                Products = Products.Where(x => !x.isSpecialPart).ToList();
                if (ToolingGroups.Length > 0)
                {
                    SelectedToolingGroup = ToolingGroups[0];
                }
                ChooseToolingGroupControls.Visibility = Visibility.Visible;
            }

            RefreshView();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableProductsListView.SelectedValue is not TurnedProduct product)
            {
                return;
            }
            LatheManufactureOrderItem newItem = new(product);
            newItem.EditMade += CalculateInsights;
            NewOrderItems.Add(newItem);
            ProductsInToolingGroup.Remove(product);

            if (NewOrderItems.Count == 1)
            {
                NewOrder.BarID = product.BarID;
                RequiredProduct = product;
            }

            RefreshView();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewOrderItemsListView.SelectedValue is not LatheManufactureOrderItem item)
            {
                return;
            }

            if (item.RequiredQuantity > 0)
            {
                return;
            }

            NewOrderItems.Remove(item);

            RefreshView();
        }

        private void RefreshView()
        {
            string[] selectedForOrder = NewOrderItems.Select(x => x.ProductName).ToArray();
            List<TurnedProduct> products;
            if (NewOrderItems.Count > 0)
            {
                products = Products
                    .Where(x => x.BarID == NewOrder.BarID
                        && x.ProductGroup == SelectedToolingGroup
                        && !selectedForOrder.Contains(x.ProductName))
                    .ToList();
            }
            else
            {
                products = Products
                    .Where(x => x.ProductGroup == SelectedToolingGroup)
                    .ToList();
            }

            products = products.OrderBy(x => x.Material).ThenBy(x => x.ProductName).ToList();

            ProductsInToolingGroup.Clear();
            for (int i = 0; i < products.Count; i++)
            {
                ProductsInToolingGroup.Add(products[i]);
            }

            CalculateInsights();
        }

        private void CalculateInsights()
        {
            if (NewOrderItems.Count == 0)
            {
                InsightsStackPanel.Visibility = Visibility.Collapsed;
                NoInsights.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                InsightsStackPanel.Visibility = Visibility.Visible;
                NoInsights.Visibility = Visibility.Collapsed;
            }

            Insights.BarId = NewOrder.BarID;
            BarStock bar = Bars.Find(x => x.Id == NewOrder.BarID);
            Insights.BarPrice = bar.Cost;

            double numBars = 0;
            int time = 3600 * 4;
            int value = 0;
            Insights.TimeIsEstimate = false;

            List<LatheManufactureOrderItem> itemsWithCycleTime = NewOrderItems.Where(x => x.CycleTime != 0).ToList();

            for (int i = 0; i < NewOrderItems.Count; i++)
            {
                double partBudget = NewOrderItems[i].MajorLength + 2;
                double availablePerBar = bar.Length - 300;
                double partsPerBar = Math.Floor(availablePerBar / partBudget);
                numBars += NewOrderItems[i].TargetQuantity / partsPerBar;

                value += (int)(NewOrderItems[i].SellPrice * NewOrderItems[i].TargetQuantity * 0.7);

                if (NewOrderItems[i].CycleTime > 0)
                {
                    time += NewOrderItems[i].CycleTime * NewOrderItems[i].TargetQuantity;
                }
                else
                {
                    time += NewOrderItems[i].GetCycleTime() * NewOrderItems[i].TargetQuantity;
                    Insights.TimeIsEstimate = true;
                }
            }

            Insights.TimeToComplete = time;
            Insights.NumberOfBarsRequired = numBars;
            Insights.TotalBarCost = numBars * bar.Cost;

            time = Math.Max(time, 3600 * 24);

            TimeSpan timeToComplete = TimeSpan.FromSeconds(time);
            Insights.CostOfMachineTime = timeToComplete.TotalMinutes * 0.45;

            Insights.CostOfOrder = Insights.CostOfMachineTime + (Insights.TotalBarCost / 100);
            Insights.ValueProduced = value / 100;
            Insights.NetProfit = Insights.ValueProduced - Insights.CostOfOrder;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Insights)));
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
            if (NewOrderItems.Count == 0)
            {
                return;
            }
            CreateManufactureOrder();
        }

        private void CreateManufactureOrder()
        {
            NewOrder.Name = GetNewOrderId();
            NewOrder.CreatedAt = DateTime.Now;
            NewOrder.CreatedBy = App.CurrentUser.GetFullName();
            NewOrder.MajorDiameter = NewOrderItems.First().MajorDiameter;
            NewOrder.BarsInStockAtCreation = Bars.Find(x => x.Id == NewOrder.BarID).InStock;
            NewOrder.NumberOfBars = Math.Ceiling(Insights.NumberOfBarsRequired);
            NewOrder.ToolingGroup = SelectedToolingGroup;

            int time = 0;
            List<TurnedProduct> comparableProducts = ProductsInToolingGroup.Where(x => x.CycleTime > 0).ToList();
            int lastCycleTime = comparableProducts.Count > 0
                ? comparableProducts.Min(x => x.CycleTime)
                : 0;
            List<LatheManufactureOrderItem> items = NewOrderItems.OrderByDescending(x => x.CycleTime).ToList();
            for (int i = 0; i < items.Count; i++)
            {
                if (NewOrderItems[i].CycleTime > 0)
                {
                    time += NewOrderItems[i].CycleTime * NewOrderItems[i].TargetQuantity;
                }
                else if (lastCycleTime > 0)
                {
                    time += NewOrderItems[i].GetCycleTime() * NewOrderItems[i].TargetQuantity;
                }
                else
                {
                    time += NewOrderItems[i].GetCycleTime() * NewOrderItems[i].TargetQuantity;
                }

                lastCycleTime = NewOrderItems[i].CycleTime;
            }
            NewOrder.TimeToComplete = time;
            items = items.Where(x => x.CycleTime > 0).ToList();

            if (items.Count > 0)
            {
                NewOrder.TargetCycleTime = items.Min(x => x.CycleTime);
                NewOrder.TargetCycleTimeEstimated = false;
            }
            else
            {
                NewOrder.TargetCycleTime = NewOrderItems[0].GetCycleTime();
                NewOrder.TargetCycleTimeEstimated = true;
            }

            List<TechnicalDrawing> allDrawings = DatabaseHelper.Read<TechnicalDrawing>().Where(x => x.DrawingType == TechnicalDrawing.Type.Production).ToList();
            List<TechnicalDrawing> drawings = TechnicalDrawing.FindDrawings(allDrawings, NewOrderItems.ToList(), NewOrder.ToolingGroup);

            _ = DatabaseHelper.Insert(NewOrder);
            foreach (LatheManufactureOrderItem item in NewOrderItems)
            {
                if (item.TargetQuantity == 0)
                {
                    continue;
                }

                item.CycleTime = 0; //force to update if different
                if (Request != null)
                {
                    item.NeedsCleaning = Request.CleanCustomerRequirement && item.RequiredQuantity > 0;
                }
                item.AssignedMO = NewOrder.Name;
                item.AddedBy = App.CurrentUser.GetFullName();
                item.DateAdded = DateTime.Now;
                DatabaseHelper.Insert(item);
            };

            foreach (TechnicalDrawing drawing in drawings)
            {
                OrderDrawing o = new() { DrawingId = drawing.Id, OrderId = NewOrder.Name };
                DatabaseHelper.Insert(o);
            }

            Cancelled = false;
            Close();
        }

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
