using ProjectLighthouse.Model.Deliveries;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.View
{
    public partial class CreateNewDeliveryWindow : Window
    {
        private List<DeliveryItem> allUndeliveredItems;
        private List<DeliveryItem> filteredUndeliveredItems;
        private List<DeliveryItem> itemsOnNewNote;
        private List<TurnedProduct> turnedProducts;
        private List<NonTurnedItem> nonTurnedItems;
        private List<Lot> lots;

        public bool SaveExit;

        public CreateNewDeliveryWindow()
        {
            InitializeComponent();
            allUndeliveredItems = new List<DeliveryItem>();
            filteredUndeliveredItems = new List<DeliveryItem>();
            itemsOnNewNote = new List<DeliveryItem>();
            turnedProducts = DatabaseHelper.Read<TurnedProduct>();
            nonTurnedItems = DatabaseHelper.Read<NonTurnedItem>();
            lots = new List<Lot>();
            GetUndelivered();
        }

        private void GetUndelivered()
        {
            lots = DatabaseHelper.Read<Lot>().Where(n => !n.IsDelivered && n.IsAccepted && n.Quantity > 0 && n.AllowDelivery).ToList();
            List<ScheduleItem> orders = new();
            orders.AddRange(DatabaseHelper.Read<LatheManufactureOrder>().ToList());
            orders.AddRange(DatabaseHelper.Read<GeneralManufactureOrder>().ToList());

            allUndeliveredItems.Clear();
            filteredUndeliveredItems.Clear();
            itemsOnNewNote.Clear();

            foreach (Lot lot in lots)
            {
                string _POref = orders.Find(x => x.Name == lot.Order)!.POReference;
                 
                DeliveryItem newDeliveryItem = new()
                {
                    ItemManufactureOrderNumber = lot.Order,
                    PurchaseOrderReference = _POref,
                    Product = lot.ProductName,
                    QuantityThisDelivery = lot.Quantity,
                    LotID = lot.ID,
                    FromMachine = lot.FromMachine
                };


                TurnedProduct deliveringTurnedItem = turnedProducts.Find(x => lot.ProductName == x.ProductName);
                if(deliveringTurnedItem != null )
                {
                    newDeliveryItem.ExportProductName = string.IsNullOrEmpty(deliveringTurnedItem.ExportProductName) 
                        ? lot.ProductName 
                        : deliveringTurnedItem.ExportProductName;
                    
                }
                else
                {
                    NonTurnedItem deliveringNonTurnedItem = nonTurnedItems.Find(x => x.Name == lot.ProductName);
                    if (deliveringNonTurnedItem != null)
                    {
                        newDeliveryItem.ExportProductName = lot.ProductName;
                    }
                }


                allUndeliveredItems.Add(newDeliveryItem);
            }

            filteredUndeliveredItems = new List<DeliveryItem>(allUndeliveredItems.OrderBy(n => n.Product));
            undeliveredList.ItemsSource = filteredUndeliveredItems;
            deliveryList.ItemsSource = itemsOnNewNote;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (deliveryList.SelectedValue is not DeliveryItem move_item)
            {
                return;
            }

            itemsOnNewNote.Remove(move_item);
            filteredUndeliveredItems.Add(move_item);
            RefreshLists();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
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

            SQLiteConnection conn = DatabaseHelper.GetConnection();

            List<LatheManufactureOrderItem> orderItems = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();
            List<GeneralManufactureOrder> generalOrders = DatabaseHelper.Read<GeneralManufactureOrder>().ToList();

            List<object> insertTransactions = new();
            List<object> updateTransactions = new();

            foreach (DeliveryItem item in itemsOnNewNote)
            {
                item.AllocatedDeliveryNote = newDeliveryNote.Name;
                Lot? lot = lots.Find(l => l.ID == item.LotID);
                if (lot is null)
                {
                    MessageBox.Show($"Could not find lot with ID '{item.LotID}'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    conn.Rollback();
                    conn.Close();
                    return;
                }

                lot.IsDelivered = true;

                updateTransactions.Add(lot);


                LatheManufactureOrderItem? orderItem = orderItems.Find(i => i.ProductName == item.Product
                                                                        && i.AssignedMO == item.ItemManufactureOrderNumber);
                if (orderItem != null)
                {
                    orderItem.QuantityDelivered += item.QuantityThisDelivery;
                    updateTransactions.Add(orderItem);
                }
                else
                {
                    GeneralManufactureOrder? order = generalOrders.Find(x => x.Name == lot.Order);
                    if (order is null)
                    {
                        MessageBox.Show($"Could not find parent for lot with ID '{item.LotID}'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        conn.Rollback();
                        conn.Close();
                        return;
                    }

                    order.DeliveredQuantity += lot.Quantity;
                    updateTransactions.Add(order);
                }

                insertTransactions.Add(item);
            }

            insertTransactions.Add(newDeliveryNote);
            conn.BeginTransaction();


            foreach(object  o in updateTransactions)
            {
                if (conn.Update(o) != 1)
                {
                    conn.Rollback();
                    conn.Close();
                    MessageBox.Show($"Failed to update database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            foreach (object o in insertTransactions)
            {
                if (conn.Insert(o) != 1)
                {
                    conn.Rollback();
                    conn.Close();
                    MessageBox.Show($"Failed to update database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            conn.Commit();
            conn.Close();

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
