using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.LatheOrderViews
{
    public partial class AddItemToOrderWindow : Window, INotifyPropertyChanged
    {
        private List<TurnedProduct> PossibleProducts;

        public event PropertyChangedEventHandler PropertyChanged;

        private List<LatheManufactureOrderItem> possibleItems;
        string parentOrderId;
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

        public AddItemToOrderWindow(string OrderId)
        {
            InitializeComponent();
            parentOrderId = OrderId;
            LoadData(OrderId);
            DataContext = this;

        }

        void LoadData(string orderId)
        {
            LatheManufactureOrder order = DatabaseHelper.Read<LatheManufactureOrder>().Find(x => x.Name == orderId);
            List<LatheManufactureOrderItem> currentOrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>().Where(x => x.AssignedMO == orderId).ToList();
            PossibleProducts = DatabaseHelper.Read<TurnedProduct>().Where(x => x.BarID == order.BarID && x.ProductGroup == order.ToolingGroup && !currentOrderItems.Any(y => y.ProductName == x.ProductName)).ToList();
            List<LatheManufactureOrderItem> items = new();
            for (int i = 0; i < PossibleProducts.Count; i++)
            {
                items.Add(new(PossibleProducts[i]));
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

            AddButton.Content = $"Add Items [{list.SelectedItems.Count:0}]";
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
                newItem.AssignedMO = parentOrderId;
                DatabaseHelper.Insert(newItem);
            }

            ItemsWereAdded = true;
            Close();
        }
    }
}
