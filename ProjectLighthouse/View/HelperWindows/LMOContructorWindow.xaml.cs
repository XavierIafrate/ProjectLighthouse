using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.View
{
    public partial class LMOContructorWindow : Window
    {
        public bool wasCancelled;
        public Request ApprovedRequest;
        public LatheManufactureOrder constructLMO;
        public List<LatheManufactureOrderItem> LMOItems;
        public string OrderBarID;

        public List<TurnedProduct> ListboxProducts;
        public List<TurnedProduct> ProductPool;

        public LMOContructorWindow(Request approvedRequest)
        {
            InitializeComponent();
            OrderBarID = "";
            ApprovedRequest = approvedRequest;
            constructLMO = new LatheManufactureOrder();
            LMOItems = new List<LatheManufactureOrderItem>();
            ListboxProducts = new List<TurnedProduct>();
            ProductPool = new List<TurnedProduct>();
            wasCancelled = true;

            requiredProductTextBlock.Text = approvedRequest.Product;
            requiredQuantityTextBlock.Text = $"{approvedRequest.QuantityRequired:#,##0} pcs";
            requiredDateTextBlock.Text = $"Delivery required by {approvedRequest.DateRequired:dd/MM/yy}";

            ReadDatabase(approvedRequest);

            //LatheManufactureOrderItem requiredItem = new LatheManufactureOrderItem()
            //{
            //    ProductName = approvedRequest.Product,
            //    RequiredQuantity = approvedRequest.QuantityRequired,
            //};

            RefreshView();
        }

        public void ReadDatabase(Request request)
        {
            string productName = request.Product;
            ProductPool = DatabaseHelper.Read<TurnedProduct>().Where(n => n.ProductGroup == productName.Substring(0, 9)).ToList();
            TurnedProduct requiredProduct = new();

            // Assign required product
            foreach (TurnedProduct product in ProductPool)
            {
                if (product.ProductName == productName)
                {
                    requiredProduct = product;
                    constructLMO.BarID = product.BarID;
                    constructLMO.ItemNeedsCleaning = request.CleanCustomerRequirement;
                    LMOItems.Add(TurnedProductToLMOItem(requiredProduct, request.QuantityRequired, request.DateRequired, request.CleanCustomerRequirement));
                }
            }

            // remove incompatible
            foreach (TurnedProduct product in ProductPool.ToList())
            {
                if (!product.IsScheduleCompatible(requiredProduct) || !product.CanBeManufactured())
                {
                    ProductPool.Remove(product);
                }
            }
        }

        public static LatheManufactureOrderItem TurnedProductToLMOItem(TurnedProduct product, int requiredQuantity, DateTime dateRequired, bool cleaning)
        {
            int MOQ = product.MajorDiameter switch
            {
                > 35 => 10,
                > 30 => 50,
                > 25 => 100,
                > 20 => 200,
                > 15 => 300,
                _ => 500,
            };
            
            int recommendedQuantity = product.GetRecommendedQuantity();
            int target = recommendedQuantity + requiredQuantity > MOQ
                ? (int)Math.Ceiling((double)(recommendedQuantity + requiredQuantity) / 100) * 100
                : MOQ;


            LatheManufactureOrderItem newItem = new()
            {
                ProductName = product.ProductName,
                RequiredQuantity = requiredQuantity,
                TargetQuantity = target,
                DateRequired = dateRequired,
                CycleTime = product.CycleTime,
                MajorLength = product.MajorLength,
                IsSpecialPart = product.isSpecialPart,
                NeedsCleaning = cleaning,
            };

            if (newItem.CycleTime == 0)
            {
                newItem.CycleTime = 120;
            }

            return newItem;
        }

        public void RefreshView()
        {
            ListboxProducts.Clear();
            foreach (TurnedProduct product in ProductPool)
            {
                bool found = LMOItems.SingleOrDefault(x => x.ProductName == product.ProductName) != null;

                if (!found)
                {
                    ListboxProducts.Add(product);
                }
            }

            LMOItemsListBox.ItemsSource = LMOItems.ToList();
            poolListBox.ItemsSource = ListboxProducts;

            CalculateInsights();
        }

        private void CalculateInsights()
        {
            int totaltime = 0;
            int requiredtime = 0;
            double bars = 0;

            foreach (LatheManufactureOrderItem item in LMOItems)
            {
                totaltime += item.CycleTime * item.TargetQuantity;
                bars += item.TargetQuantity * (item.MajorLength + 2) / 2700;
                if (item.RequiredQuantity > 0)
                {
                    requiredtime += item.RequiredQuantity * item.CycleTime;
                }
            }

            bars = Math.Ceiling(bars);

            IEnumerable<BarStock> barStock = DatabaseHelper.Read<BarStock>().Where(n => n.Id == constructLMO.BarID);
            int costPerBar = barStock.First().Cost;
            double dblmaterialCost = Math.Round(Convert.ToDouble(costPerBar) / 100 * bars, 2);

            nBars.Text = $"{bars}";

            constructLMO.NumberOfBars = bars;
            materialCost.Text = $"£{Math.Round(dblmaterialCost, 0)}";
            reqTime.Text = $"{Math.Round(requiredtime / (double)86400, 2)} day(s)";
            totalTime.Text = $"{Math.Round(totaltime / (double)86400, 2)} day(s)";
            constructLMO.TimeToComplete = totaltime;
        }

        private static string GetNewMOName()
        {
            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>();
            int nOrders = orders.Count;
            string strOrderNum = Convert.ToString(nOrders + 1);
            int orderNumLen = strOrderNum.Length;
            const string blank = "M00000";

            return blank.Substring(0, 6 - orderNumLen) + strOrderNum;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            constructLMO.Name = GetNewMOName();
            constructLMO.CreatedAt = DateTime.Now;
            constructLMO.CreatedBy = App.CurrentUser.GetFullName();
            constructLMO.IsComplete = false;
            constructLMO.Status = "Awaiting scheduling";
            constructLMO.IsReady = false;
            constructLMO.IsUrgent = false;
            constructLMO.HasProgram = false;
            constructLMO.HasStarted = false;
            constructLMO.BarIsAllocated = false;

            // Add order & items to database
            _ = DatabaseHelper.Insert(constructLMO);
            foreach (LatheManufactureOrderItem item in LMOItems)
            {
                item.AssignedMO = constructLMO.Name;
                item.AddedBy = $"{App.CurrentUser.FirstName} {App.CurrentUser.LastName}";
                item.DateAdded = DateTime.Now;
                DatabaseHelper.Insert(item);
            };



            MessageBox.Show($"Created {constructLMO.Name}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            wasCancelled = false;
            Close();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (LMOItemsListBox.SelectedValue is not LatheManufactureOrderItem selectedLMOItem)
                return;

            if (selectedLMOItem.RequiredQuantity > 0)
            {
                MessageBox.Show("This product is a requirement for the request!", "Cannot remove.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            LMOItems.Remove(selectedLMOItem);

            RefreshView();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (LMOItems.Count >= 4)
            {
                MessageBox.Show("Max Items reached", "Order Full", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (poolListBox.SelectedValue is not TurnedProduct selectedProduct)
            {
                return;
            }

            //LatheManufactureOrderItem newItem = TurnedProductToLMOItem(selectedProduct, 0, DateTime.MinValue);

            LMOItems.Add(TurnedProductToLMOItem(selectedProduct, 0, DateTime.MinValue, false));
            RefreshView();
        }

        private void UpdateQty_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(UpdateQty.Text, out int j))
            {
                return;
            }

            List<LatheManufactureOrderItem> items = (List<LatheManufactureOrderItem>)LMOItemsListBox.ItemsSource;
            LatheManufactureOrderItem selected = (LatheManufactureOrderItem)LMOItemsListBox.SelectedValue;

            foreach (LatheManufactureOrderItem i in items)
            {
                if (i.ProductName == selected.ProductName)
                {
                    i.TargetQuantity = j;
                }
            }

            LMOItemsListBox.ItemsSource = new List<LatheManufactureOrderItem>(items);
            CalculateInsights();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
