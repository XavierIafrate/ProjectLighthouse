using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders
{
    public partial class GeneralOrderConstructorWindow : Window, INotifyPropertyChanged
    {
        private GeneralManufactureOrder newOrder;

        public GeneralManufactureOrder NewOrder
        {
            get { return newOrder; }
            set { newOrder = value; OnPropertyChanged(); }
        }

        private List<NonTurnedItem> items;

        public List<NonTurnedItem> Items
        {
            get { return items; }
            set { items = value; OnPropertyChanged(); }
        }

        public bool SaveExit { get; set; }

        private NonTurnedItem newItem;

        public NonTurnedItem NewItem
        {
            get { return newItem; }
            set { newItem = value; OnPropertyChanged(); }
        }


        public GeneralOrderConstructorWindow()
        {
            InitializeComponent();
            NewOrder = new();
            NewItem = new();

            Items = DatabaseHelper.Read<NonTurnedItem>();

            if (Items.Count == 0)
            {
                throw new InvalidOperationException("No non-turned items in the database - a general order cannot be raised.");
            }

            DatePicker.DisplayDateStart = DateTime.Today.AddDays(1);
            DatePicker.SelectedDate = DateTime.Today.AddMonths(1);

            NewOrder.NonTurnedItemId = Items.First().Id;

            this.UpdateProductControls.Visibility = Visibility.Collapsed;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {

            NewOrder.ValidateAll();
            if (NewOrder.HasErrors)
            {
                return;
            }

            NewOrder.Item = Items.Find(x => x.Id == NewOrder.NonTurnedItemId);

            NewOrder.Name = GetNewOrderId();
            NewOrder.CreatedAt = DateTime.Now;
            NewOrder.CreatedBy = App.CurrentUser.GetFullName();
            NewOrder.TimeToComplete = NewOrder.CalculateTimeToComplete();

            if (!DatabaseHelper.Insert(NewOrder))
            {
                MessageBox.Show("Failed to create new order", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveExit = true;
            Close();
        }

        private static string GetNewOrderId()
        {
            List<GeneralManufactureOrder> orders = DatabaseHelper.Read<GeneralManufactureOrder>();
            int nOrders = orders.Count;
            string strOrderNum = Convert.ToString(nOrders + 1);
            int orderNumLen = strOrderNum.Length;
            const string blank = "G00000";

            return blank[..(6 - orderNumLen)] + strOrderNum;
        }

        private void CreateProductButton_Click(object sender, RoutedEventArgs e)
        {
            NewItem.ValidateAll();
            if (NewItem.HasErrors)
            {
                return;
            }

            try
            {
                DatabaseHelper.Insert(NewItem, throwErrs: true);
            }
            catch(Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
                return;
            }

            Items = DatabaseHelper.Read<NonTurnedItem>();
            NewOrder.NonTurnedItemId = NewItem.Id;

            NewItem = new();
        }

        private void SaveProductButton_Click(object sender, RoutedEventArgs e)
        {
            NewItem.ValidateAll();
            if (NewItem.HasErrors)
            {
                return;
            }

            try
            {
                DatabaseHelper.Update(NewItem, throwErrs: true);
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
                return;
            }

            Items = DatabaseHelper.Read<NonTurnedItem>();
            NewOrder.NonTurnedItemId = NewItem.Id;
            ExitEdit();
        }

        private void CancelProductChangesButton_Click(object sender, RoutedEventArgs e)
        {
            ExitEdit();
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewOrder.NonTurnedItemId == 0)
                return;

            EnterEdit();
        }

        private void EnterEdit()
        {
            EditProductButton.IsEnabled = false;
            NonTurnedItem? itemToEdit = Items.Find(x => x.Id == NewOrder.NonTurnedItemId);

            if(itemToEdit == null)
            {
                MessageBox.Show($"Could not find item with Id {NewOrder.NonTurnedItemId}");
                return;
            }

            NewItem = (NonTurnedItem)itemToEdit.Clone();
            ProductEditHeader.Text = "Edit Product";
            CreateProductButton.Visibility = Visibility.Collapsed;
            UpdateProductControls.Visibility = Visibility.Visible;
            ProductNameInput.IsEnabled = false;


        }

        private void ExitEdit()
        {
            ProductEditHeader.Text = "New Product";
            NewItem = new();
            CreateProductButton.Visibility = Visibility.Visible;
            UpdateProductControls.Visibility = Visibility.Collapsed;
            ProductNameInput.IsEnabled = true;
            EditProductButton.IsEnabled = true;
        }
    }
}
