using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        public GeneralOrderConstructorWindow()
        {
            InitializeComponent();
            NewOrder = new();
            Items = DatabaseHelper.Read<NonTurnedItem>();

             if (Items.Count == 0)
            {
                throw new InvalidOperationException("No non-turned items in the database - a general order cannot be raised.");
            }

            DatePicker.DisplayDateStart = DateTime.Today.AddDays(1);
            DatePicker.SelectedDate = DateTime.Today.AddMonths(1);

            NewOrder.NonTurnedItemId = Items.First().Id;
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
    }
}
