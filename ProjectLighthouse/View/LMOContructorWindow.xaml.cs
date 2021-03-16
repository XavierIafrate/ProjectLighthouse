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

        public ObservableCollection<TurnedProduct> ListboxProducts;
        public ObservableCollection<TurnedProduct> ProductPool;

        public LMOContructorWindow(Request approvedRequest)
        {
            InitializeComponent();

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
                    LMOItems.Add(TurnedProductToLMOItem(requiredProduct, request.QuantityRequired, request.DateRequired));
                }
            }

            foreach (var product in ProductPool)
            {
                if (product.MajorDiameter > 20 && product.MajorLength > 90 && product.Material != requiredProduct.Material && product.ThreadSize != requiredProduct.ThreadSize && product.DriveSize != requiredProduct.DriveSize && product.DriveType != requiredProduct.DriveType)
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
                TargetQuantity = Math.Max(requiredQuantity + product.GetRecommendedQuantity(), MOQ),
                DateRequired = dateRequired
            };


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
    }
}
