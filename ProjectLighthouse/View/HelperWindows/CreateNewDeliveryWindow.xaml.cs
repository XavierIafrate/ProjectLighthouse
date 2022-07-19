using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.View
{
    public partial class CreateNewDeliveryWindow : Window
    {
        private List<DeliveryItem> allUndeliveredItems { get; set; }
        private List<DeliveryItem> filteredUndeliveredItems { get; set; }
        private List<DeliveryItem> itemsOnNewNote { get; set; }
        private List<Lot> Lots { get; set; }
        public bool SaveExit = false;

        public CreateNewDeliveryWindow()
        {
            InitializeComponent();
            allUndeliveredItems = new List<DeliveryItem>();
            filteredUndeliveredItems = new List<DeliveryItem>();
            itemsOnNewNote = new List<DeliveryItem>();
            Lots = new List<Lot>();
            GetUndelivered();
        }

        private void GetUndelivered()
        {
            Lots = DatabaseHelper.Read<Lot>().Where(n => !n.IsDelivered && n.IsAccepted && n.Quantity > 0 && n.AllowDelivery).ToList();
            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>().ToList();
            //List<LatheManufactureOrderItem> items = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();

            allUndeliveredItems.Clear();
            filteredUndeliveredItems.Clear();
            itemsOnNewNote.Clear();

            string _POref = string.Empty;
            foreach (Lot lot in Lots)
            {
                foreach (LatheManufactureOrder order in orders)
                {
                    if (order.Name == lot.Order)
                    {
                        _POref = order.POReference;
                    }
                }

                allUndeliveredItems.Add(new DeliveryItem()
                {
                    ItemManufactureOrderNumber = lot.Order,
                    PurchaseOrderReference = _POref,
                    Product = lot.ProductName,
                    QuantityThisDelivery = lot.Quantity,
                    LotID = lot.ID,
                    FromMachine = lot.FromMachine
                });
            }

            filteredUndeliveredItems = new List<DeliveryItem>(allUndeliveredItems.OrderBy(n => n.Product));
            undeliveredList.ItemsSource = filteredUndeliveredItems;
            deliveryList.ItemsSource = itemsOnNewNote;
        }

        private void remButton_Click(object sender, RoutedEventArgs e)
        {
            if (deliveryList.SelectedValue is not DeliveryItem move_item)
                return;
            itemsOnNewNote.Remove(move_item);
            filteredUndeliveredItems.Add(move_item);
            RefreshLists();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (undeliveredList.SelectedValue is not DeliveryItem move_item)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(move_item.PurchaseOrderReference))
            {
                MessageBox.Show("The associated order requires a Purchase Order reference to proceed.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            itemsOnNewNote.Add(move_item);
            filteredUndeliveredItems.Remove(move_item);
            RefreshLists();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (itemsOnNewNote.Count == 0)
            {
                MessageBox.Show("No items on delivery!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DeliveryNote newDeliveryNote = new()
            {
                Name = GetDeliveryNum(),
                DeliveryDate = DateTime.Now,
                DeliveredBy = App.CurrentUser.GetFullName(),
            };

            DatabaseHelper.Insert(newDeliveryNote);

            List<LatheManufactureOrderItem> orderItems = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();

            foreach (DeliveryItem item in itemsOnNewNote)
            {
                item.AllocatedDeliveryNote = newDeliveryNote.Name;

                LatheManufactureOrderItem x = orderItems.SingleOrDefault(i => i.ProductName == item.Product
                                                                            && i.AssignedMO == item.ItemManufactureOrderNumber);
                x.QuantityDelivered += item.QuantityThisDelivery;
                DatabaseHelper.Update<LatheManufactureOrderItem>(x);

                Lot lot = Lots.SingleOrDefault(l => l.ID == item.LotID);
                lot.IsDelivered = true;
                DatabaseHelper.Update<Lot>(lot);

                DatabaseHelper.Insert(item);
            }
            SaveExit = true;
            Close();
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

        private static string GetDeliveryNum()
        {
            List<DeliveryNote> deliveryNotes = DatabaseHelper.Read<DeliveryNote>().ToList();

            int nOrders = deliveryNotes.Count;
            string strOrderNum = Convert.ToString(nOrders + 1);
            int orderNumLen = strOrderNum.Length;
            const string blank = "DN00000";
            return blank[..(7 - orderNumLen)] + strOrderNum;
        }

        private void updateQty_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }
    }
}
