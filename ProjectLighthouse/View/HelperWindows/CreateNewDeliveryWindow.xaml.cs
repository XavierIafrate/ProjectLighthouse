using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for CreateNewDeliveryWindow.xaml
    /// </summary>
    public partial class CreateNewDeliveryWindow : Window
    {
        private List<DeliveryItem> allUndeliveredItems { get; set; }
        private List<DeliveryItem> filteredUndeliveredItems { get; set; }
        private List<DeliveryItem> itemsOnNewNote { get; set; }

        public CreateNewDeliveryWindow()
        {
            InitializeComponent();
            allUndeliveredItems = new List<DeliveryItem>();
            filteredUndeliveredItems = new List<DeliveryItem>();
            itemsOnNewNote = new List<DeliveryItem>();
            GetUndelivered();
        }

        private void GetUndelivered()
        {
            List<LatheManufactureOrderItem> items = DatabaseHelper.Read<LatheManufactureOrderItem>().Where(n => n.QuantityDelivered < n.QuantityMade).ToList();
            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>().ToList();

            allUndeliveredItems.Clear();
            filteredUndeliveredItems.Clear();
            itemsOnNewNote.Clear();

            string _POref = String.Empty;
            foreach(var item in items)
            {
                foreach(var order in orders)
                {
                    if(order.Name == item.AssignedMO)
                    {
                        _POref = order.POReference;
                    }
                }
                allUndeliveredItems.Add(new DeliveryItem()
                {
                    ItemManufactureOrderNumber = item.AssignedMO,
                    PurchaseOrderReference = _POref,
                    Product = item.ProductName,
                    QuantityThisDelivery = item.QuantityMade - item.QuantityDelivered,
                    QuantityToFollow = Math.Max(item.TargetQuantity - item.QuantityDelivered - item.QuantityMade, (int)0)
                });
            }

            filteredUndeliveredItems = new List<DeliveryItem>(allUndeliveredItems.OrderBy(n=>n.Product));
            undeliveredList.ItemsSource = filteredUndeliveredItems;
            deliveryList.ItemsSource = itemsOnNewNote;
        }

        private void remButton_Click(object sender, RoutedEventArgs e)
        {
            DeliveryItem move_item = deliveryList.SelectedValue as DeliveryItem;
            if (move_item == null)
                return;
            itemsOnNewNote.Remove(move_item);
            filteredUndeliveredItems.Add(move_item);
            RefreshLists();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if(itemsOnNewNote.Count == 8)
            {
                MessageBox.Show("Max 8 items on a delivery", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DeliveryItem move_item = undeliveredList.SelectedValue as DeliveryItem;
            if (move_item == null)
                return;
            itemsOnNewNote.Add(move_item);
            filteredUndeliveredItems.Remove(move_item);
            RefreshLists();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (itemsOnNewNote.Count == 0)
            {
                MessageBox.Show("No items on delivery!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DeliveryNote newDeliveryNote = new DeliveryNote()
            {
                Name = GetDeliveryNum(),
                DeliveryDate = DateTime.Now,
                DeliveredBy = App.currentUser.GetFullName(),
            };

            DatabaseHelper.Insert(newDeliveryNote);

            List<LatheManufactureOrderItem> orderItems = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();

            foreach(var item in itemsOnNewNote)
            {
                item.AllocatedDeliveryNote = newDeliveryNote.Name;

                foreach(var orderItem in orderItems)
                {
                    if(item.Product == orderItem.ProductName && orderItem.AssignedMO == item.ItemManufactureOrderNumber)
                    {
                        orderItem.QuantityDelivered = item.QuantityThisDelivery + orderItem.QuantityDelivered;
                        DatabaseHelper.Update(orderItem);
                    }
                }

                DatabaseHelper.Insert(item);
            }
            this.Close();
        }

        private void RefreshLists() // bodge
        {
            filteredUndeliveredItems = new List<DeliveryItem>(filteredUndeliveredItems.OrderBy(n => n.Product));
            itemsOnNewNote = new List<DeliveryItem>(itemsOnNewNote.OrderBy(n => n.Product));

            undeliveredList.ItemsSource = null;
            undeliveredList.ItemsSource = filteredUndeliveredItems;
            deliveryList.ItemsSource = null;
            deliveryList.ItemsSource = itemsOnNewNote;
        }

        private string GetDeliveryNum()
        {
            List<DeliveryNote> deliveryNotes = DatabaseHelper.Read<DeliveryNote>().ToList();

            int nOrders = deliveryNotes.Count();
            string strOrderNum = Convert.ToString(nOrders + 1);
            int orderNumLen = strOrderNum.Length;
            const string blank = "DN00000";
            return blank.Substring(0, 7 - orderNumLen) + strOrderNum;

        }
    }
}
