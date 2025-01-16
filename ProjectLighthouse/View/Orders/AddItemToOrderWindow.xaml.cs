using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
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
        private List<LatheManufactureOrderItem> possibleItems;
        public List<LatheManufactureOrderItem> PossibleItems
        {
            get { return possibleItems; }
            set
            {
                possibleItems = value;
                OnPropertyChanged();
            }
        }

        private LatheManufactureOrder order;

        public AddItemToOrderWindow(LatheManufactureOrder order)
        {
            InitializeComponent();
            this.order = order;
            LoadData();
        }

        void LoadData()
        {
            TimeModel timeModel = order.TimeModelPlanned;


            PossibleProducts = DatabaseHelper.Read<TurnedProduct>()
                .Where(x =>
                    x.MaterialId == order.MaterialId &&
                    x.GroupId == order.GroupId &&
                    !order.OrderItems.Any(y => y.ProductName == x.ProductName)
                ).ToList();

            List<LatheManufactureOrderItem> items = new();
            for (int i = 0; i < PossibleProducts.Count; i++)
            {
                LatheManufactureOrderItem newItem = new(PossibleProducts[i])
                {
                    TargetQuantity = Math.Max(
                        PossibleProducts[i].GetRecommendedQuantity(),
                        RequestsEngine.GetMiniumumOrderQuantity(PossibleProducts[i])
                        )
                };

                if (newItem.PreviousCycleTime is null)
                {
                    newItem.ModelledCycleTime = timeModel.At(newItem.MajorLength);
                }

                items.Add(newItem);
            }

            PossibleItems = new(items);
        }

        public event PropertyChangedEventHandler PropertyChanged;
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
            System.Collections.IList newItems = ItemsListView.SelectedItems;

            for (int i = 0; i < newItems.Count; i++)
            {
                if (newItems[i] is not LatheManufactureOrderItem newItem) throw new InvalidCastException();
                newItem.AssignedMO = order.Name;
                order.OrderItems.Add(newItem);  
            }
            
            if(newItems.Count > 0)
            {
                order.OrderItems = order.OrderItems.OrderByDescending(x => x.RequiredQuantity).ThenBy(x => x.ProductName).ToList();
            }

            Close();
        }
    }
}
