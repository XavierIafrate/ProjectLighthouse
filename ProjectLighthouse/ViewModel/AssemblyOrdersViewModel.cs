using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Assembly;
using ProjectLighthouse.View.AssemblyViews;
using ProjectLighthouse.ViewModel.Commands.Assembly;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
{
    public class AssemblyOrdersViewModel : BaseViewModel
    {
        #region Variables
        private AssemblyManufactureOrder selectedOrder;
        public AssemblyManufactureOrder SelectedOrder
        {
            get { return selectedOrder; }
            set
            {
                selectedOrder = value;
                GetDrops();
                if (selectedOrder != null)
                {
                    ModifiedVis = string.IsNullOrEmpty(selectedOrder.ModifiedBy) ?
                        Visibility.Collapsed : Visibility.Visible;

                    FilteredOrderItems = OrderItems.Where(n => n.OrderReference == selectedOrder.Name).ToList();
                }


                OnPropertyChanged("SelectedOrder");
            }
        }

        private List<AssemblyOrderItem> orderItems;

        public List<AssemblyOrderItem> OrderItems
        {
            get { return orderItems; }
            set { orderItems = value; }
        }


        private List<AssemblyManufactureOrder> orders;
        public List<AssemblyManufactureOrder> Orders
        {
            get { return orders; }
            set { orders = value; }
        }

        private List<AssemblyManufactureOrder> filteredOrders;
        public List<AssemblyManufactureOrder> FilteredOrders
        {
            get { return filteredOrders; }
            set
            {
                filteredOrders = value;
                OnPropertyChanged("FilteredOrders");
            }
        }

        private List<Drop> drops;
        public List<Drop> Drops
        {
            get { return drops; }
            set { drops = value; }
        }

        private List<Drop> filteredDrops;
        public List<Drop> FilteredDrops
        {
            get { return filteredDrops; }
            set
            {
                filteredDrops = value;
                OnPropertyChanged("FilteredDrops");
            }
        }

        private List<AssemblyOrderItem> filteredOrderItems;

        public List<AssemblyOrderItem> FilteredOrderItems
        {
            get { return filteredOrderItems; }
            set
            {
                filteredOrderItems = value;
                OnPropertyChanged("FilteredOrderItems");
            }
        }

        private string selectedFilter;
        public string SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                selectedFilter = value;
                FilterOrders();
                NothingFoundVis = FilteredOrders.Count > 0 ?
                    Visibility.Hidden : Visibility.Visible;
                OrderVis = FilteredOrders.Count > 0 ?
                    Visibility.Visible : Visibility.Hidden;
            }
        }

        private Visibility modifiedVis;
        public Visibility ModifiedVis
        {
            get { return modifiedVis; }
            set
            {
                modifiedVis = value;
                OnPropertyChanged("ModifiedVis");
            }
        }

        private Visibility orderVis;
        public Visibility OrderVis
        {
            get { return orderVis; }
            set
            {
                orderVis = value;
                OnPropertyChanged("OrderVis");
            }
        }

        private Visibility nothingFoundVis;
        public Visibility NothingFoundVis
        {
            get { return nothingFoundVis; }
            set
            {
                nothingFoundVis = value;
                OnPropertyChanged("NothingFoundVis");
            }
        }

        public ICommand EditOrderCommand { get; set; }
        public ICommand NewOrderCommand { get; set; }
        public ICommand GeneratePDFCommand { get; set; }

        #endregion

        public AssemblyOrdersViewModel()
        {
            Orders = new List<AssemblyManufactureOrder>();
            OrderItems = new List<AssemblyOrderItem>();
            Drops = new List<Drop>();
            SelectedOrder = new AssemblyManufactureOrder();

            NewOrderCommand = new NewAssemblyOrderCommand(this);
            EditOrderCommand = new EditAssemblyOrderCommand(this);

            ModifiedVis = new Visibility();
            OrderVis = new Visibility();
            NothingFoundVis = new Visibility();

            LoadData();
            SelectedFilter = "All Active";
        }

        private void FilterOrders()
        {
            if (SelectedFilter == "All Active")
            {
                FilteredOrders = Orders.Where(n => n.Status != "Complete" || n.ModifiedAt.AddDays(1) > DateTime.Now).ToList();
            }
            else
            {
                FilteredOrders = Orders.Where(n => n.Status == "Complete").ToList();
            }
            if (Orders.Count > 0)
                SelectedOrder = Orders.First();
        }

        private void GetDrops()
        {
            if (selectedOrder == null)
                return;
            FilteredDrops = Drops.Where(n => n.ForOrder == selectedOrder.Name).OrderBy(m => m.DateRequired).ToList();
        }

        private void LoadData()
        {
            Orders = DatabaseHelper.Read<AssemblyManufactureOrder>().ToList();
            Drops = DatabaseHelper.Read<Drop>().ToList();
            OrderItems = DatabaseHelper.Read<AssemblyOrderItem>().ToList();
            FilterOrders();
        }

        public void EditOrder(AssemblyManufactureOrder order)
        {
            EditAssemblyOrderWindow EditWindow = new EditAssemblyOrderWindow();
            EditWindow.Drops = new List<Drop>(FilteredDrops);
            EditWindow.Order = order.Clone();
            EditWindow.ShowDialog();
        }

        public void GeneratePDF()
        {

        }

        public void NewOrder()
        {
            NewAssemblyOrderWindow window = new NewAssemblyOrderWindow();
            window.ShowDialog();
        }
    }
}
