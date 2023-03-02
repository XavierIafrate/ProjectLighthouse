using DocumentFormat.OpenXml.Spreadsheet;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using ProjectLighthouse.ViewModel.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders
{
    public partial class AddItemToOrderWindow : Window, INotifyPropertyChanged
    {
        private List<TurnedProduct> PossibleProducts;

        public event PropertyChangedEventHandler PropertyChanged;

        private List<LatheManufactureOrderItem> possibleItems;
        int parentOrderId;
        string parentOrderName;
        public bool ItemsWereAdded { get; set; }

        public List<LatheManufactureOrderItem> PossibleItems
        {
            get { return possibleItems; }
            set
            {
                possibleItems = value;
                OnPropertyChanged();
            }
        }

        public AddItemToOrderWindow(int OrderId)
        {
            InitializeComponent();
            parentOrderId = OrderId;
            LoadData(OrderId);
            DataContext = this;

        }

        void LoadData(int orderId)
        {
            LatheManufactureOrder order = DatabaseHelper.Read<LatheManufactureOrder>().Find(x => x.Id == orderId);
            if (order is null)
            {
                throw new System.Exception($"Order with ID '{orderId}' not found.");
            }

            parentOrderName = order.Name;

            List<LatheManufactureOrderItem> currentOrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>().Where(x => x.AssignedMO == order.Name).ToList();
            
            PossibleProducts = DatabaseHelper.Read<TurnedProduct>()
                .Where(x => 
                    x.MaterialId == order.MaterialId && 
                    x.GroupId == order.GroupId && 
                    !currentOrderItems.Any(y => y.ProductName == x.ProductName)
                ).ToList();

            List<LatheManufactureOrderItem> items = new();
            for (int i = 0; i < PossibleProducts.Count; i++)
            {
                items.Add(new(PossibleProducts[i]) 
                { 
                    TargetQuantity = Math.Max(
                        PossibleProducts[i].GetRecommendedQuantity(), 
                        RequestsEngine.GetMiniumumOrderQuantity(PossibleProducts[i])
                        )
                });
        }

            PossibleItems = new(items);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListView list)
            {
                return;
            }

            AddButton.Tag = $"Add Items [{list.SelectedItems.Count:0}]";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newItems = ItemsListView.SelectedItems;

            for (int i = 0; i < newItems.Count; i++)
            {
                LatheManufactureOrderItem newItem = (LatheManufactureOrderItem)newItems[i];
                newItem.AssignedMO = parentOrderName;
                DatabaseHelper.Insert(newItem);
            }

            ItemsWereAdded = true;
            Close();
        }
    }
}
