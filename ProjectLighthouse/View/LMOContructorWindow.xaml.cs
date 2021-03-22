using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for LMOContructorWindow.xaml
    /// </summary>
    public partial class LMOContructorWindow : Window
    {
        public bool wasCancelled;
        public Request ApprovedRequest;
        public LatheManufactureOrder constructLMO;
        public ObservableCollection<LatheManufactureOrderItem> LMOItems;
        public string OrderBarID;

        public ObservableCollection<TurnedProduct> ListboxProducts;
        public ObservableCollection<TurnedProduct> ProductPool;

        public LMOContructorWindow(Request approvedRequest)
        {
            InitializeComponent();
            OrderBarID = "";
            ApprovedRequest = approvedRequest;
            constructLMO = new LatheManufactureOrder();
            LMOItems = new ObservableCollection<LatheManufactureOrderItem>();
            ListboxProducts = new ObservableCollection<TurnedProduct>();
            ProductPool = new ObservableCollection<TurnedProduct>();
            wasCancelled = true;

            requiredProductTextBlock.Text = approvedRequest.Product;
            requiredQuantityTextBlock.Text = String.Format("{0} pcs", approvedRequest.QuantityRequired);
            requiredDateTextBlock.Text = String.Format("Delivery required by {0:dd/MM/yy}", approvedRequest.DateRequired);


            ReadDatabase(approvedRequest);

            LatheManufactureOrderItem requiredItem = new LatheManufactureOrderItem()
            {
                ProductName = approvedRequest.Product,
                RequiredQuantity = approvedRequest.QuantityRequired,
            };

            RefreshView();

        }

        public void ReadDatabase(Request request)
        {
            string productName = request.Product;

            var products = DatabaseHelper.Read<TurnedProduct>().Where(n => n.ProductGroup == productName.Substring(0, 9));

            if (products != null)
            {
                foreach(var prod in products)
                {
                    ProductPool.Add(prod);
                }
            }

            TurnedProduct requiredProduct = new TurnedProduct();

            foreach (var product in ProductPool)
            {
                if (product.ProductName == productName)
                {
                    requiredProduct = product;
                    constructLMO.BarID = product.BarID;
                    LMOItems.Add(TurnedProductToLMOItem(requiredProduct, request.QuantityRequired, request.DateRequired));
                }
            }

            foreach (var product in ProductPool.ToList())
            {
                if (product.MajorDiameter > 20 || product.MajorLength > 90 || product.Material != requiredProduct.Material || product.ThreadSize != requiredProduct.ThreadSize || product.DriveSize != requiredProduct.DriveSize || product.DriveType != requiredProduct.DriveType)
                {
                    ProductPool.Remove(product);
                }
            }

        }

        public LatheManufactureOrderItem TurnedProductToLMOItem(TurnedProduct product, int requiredQuantity, DateTime dateRequired)
        {
            const int MOQ = 500;
            LatheManufactureOrderItem newItem = new LatheManufactureOrderItem()
            {
                ProductName = product.ProductName,
                RequiredQuantity = requiredQuantity,
                TargetQuantity = Math.Max(requiredQuantity + Math.Max(500, product.GetRecommendedQuantity()), MOQ),
                DateRequired = dateRequired,
                CycleTime = product.CycleTime,
                MajorLength = product.MajorLength
            };
            if(newItem.CycleTime == 0)
            {
                newItem.CycleTime = 120;
            }

            return newItem;
        }

        public void RefreshView()
        {
            ListboxProducts.Clear();
            foreach(var product in ProductPool)
            {
                bool found = false;
                foreach(var item in LMOItems)
                {
                    if(item.ProductName == product.ProductName)
                    {
                        found = true;
                    }
                }

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

            foreach (var item in LMOItems)
            {
                totaltime += item.CycleTime * item.TargetQuantity;
                bars += (item.TargetQuantity * (item.MajorLength + 2)) / (double)2700;
                if(item.RequiredQuantity > 0)
                {
                    requiredtime += item.RequiredQuantity * item.CycleTime;
                }
            }

            bars = Math.Ceiling(bars);

            var barStock = DatabaseHelper.Read<BarStock>().Where(n => n.Id == constructLMO.BarID);
            int costPerBar = barStock.First().Cost;
            double dblmaterialCost = Math.Round((Convert.ToDouble(costPerBar) / (double)100) * bars, 2);


            nBars.Text = String.Format("{0}", bars);
            materialCost.Text = String.Format("£{0}", Math.Round(dblmaterialCost, 0));
            reqTime.Text = String.Format("{0} day(s)", Math.Round(requiredtime / (double)86400, 2) );
            totalTime.Text = String.Format("{0} day(s)", Math.Round(totaltime / (double)86400, 2) );
            constructLMO.TimeToComplete = totaltime;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private string GetNewMOName()
        {
            var orders = DatabaseHelper.Read<LatheManufactureOrder>();
            int nOrders = orders.Count();
            string strOrderNum = Convert.ToString(nOrders + 1);
            int orderNumLen = strOrderNum.Length;
            const string blank = "M00000";



            return blank.Substring(0, 6-orderNumLen) + strOrderNum;
        }

        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            constructLMO.Name = GetNewMOName();
            constructLMO.CreatedAt = DateTime.Now;
            constructLMO.CreatedBy = String.Format("{0} {1}", App.currentUser.FirstName, App.currentUser.LastName);
            constructLMO.IsComplete = false;
            constructLMO.Status = "Awaiting scheduling";
            constructLMO.IsReady = false;
            constructLMO.IsUrgent = false;
            constructLMO.HasProgram = false;
            constructLMO.HasStarted = false;

            DatabaseHelper.Insert(constructLMO);

            foreach (LatheManufactureOrderItem item in LMOItems) 
            {
                item.AssignedMO = constructLMO.Name;
                item.AddedBy = String.Format("{0} {1}", App.currentUser.FirstName, App.currentUser.LastName);
                item.DateAdded = DateTime.Now;
                DatabaseHelper.Insert(item);
            };


            MessageBox.Show(String.Format("Created {0}", constructLMO.Name), "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            wasCancelled = false;
            this.Close();
            
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedLMOItem = LMOItemsListBox.SelectedValue as LatheManufactureOrderItem;

            if (selectedLMOItem.RequiredQuantity > 0)
            {
                MessageBox.Show("This product is a requirement for the request!", "Cannot remove.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            LMOItems.Remove(selectedLMOItem);

            RefreshView();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {

            if (LMOItems.Count >= 4)
            {
                MessageBox.Show("Max Items reached", "Order Full", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            var selectedProduct = poolListBox.SelectedValue as TurnedProduct;

            if (selectedProduct == null)
            {
                return;
            }
            LatheManufactureOrderItem newItem = TurnedProductToLMOItem(selectedProduct, 0, DateTime.MinValue);
            
            LMOItems.Add(TurnedProductToLMOItem((TurnedProduct)poolListBox.SelectedValue, 0, DateTime.MinValue));
            RefreshView();
        }

        private void LMOConstructionDisplayProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Hello World");
        }
    }
}
