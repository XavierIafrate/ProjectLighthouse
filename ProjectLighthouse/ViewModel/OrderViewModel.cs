using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
{
    public class OrderViewModel : BaseViewModel
    {
        public ObservableCollection<LatheManufactureOrder> LatheManufactureOrders { get; set; }
        public ObservableCollection<LatheManufactureOrderItem> LMOItems { get; set; }
        public ObservableCollection<LatheManufactureOrderItem> FilteredLMOItems { get; set; }

        private LatheManufactureOrder selectedLatheManufactureOrder;
        public LatheManufactureOrder SelectedLatheManufactureOrder
        {
            get { return selectedLatheManufactureOrder; }
            set 
            { 
                selectedLatheManufactureOrder = value;
                OnPropertyChanged("SelectedLatheManufactureOrder");
                SelectedLatheManufactureOrderChanged?.Invoke(this, new EventArgs());
                LoadLMOItems();

                if (String.IsNullOrEmpty(selectedLatheManufactureOrder.ModifiedBy))
                {
                    ModifiedVis = Visibility.Collapsed;
                }
                else
                {
                    ModifiedVis = Visibility.Visible;
                }

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


        public ICommand PrintOrderCommand { get; set; }
        public ICommand EditCommand { get; set; }

        public event EventHandler SelectedLatheManufactureOrderChanged;

        public OrderViewModel()
        {
            LatheManufactureOrders = new ObservableCollection<LatheManufactureOrder>();
            LMOItems = new ObservableCollection<LatheManufactureOrderItem>();
            FilteredLMOItems = new ObservableCollection<LatheManufactureOrderItem>();
            SelectedLatheManufactureOrder = new LatheManufactureOrder();

            PrintOrderCommand = new PrintCommand(this);
            EditCommand = new EditManufactureOrderCommand(this);

            GetLatheManufactureOrders();
            GetLatheManufactureOrderItems();
            SelectedLatheManufactureOrder = LatheManufactureOrders.First();
        }

        private void GetLatheManufactureOrders()
        {
            var orders = DatabaseHelper.Read<LatheManufactureOrder>().ToList().Where(n => n.IsComplete != true);
            LatheManufactureOrders.Clear();
            foreach (var order in orders)
            {
                LatheManufactureOrders.Add(order);
            }
        }

        private void GetLatheManufactureOrderItems()
        {
            var items = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();
            LMOItems.Clear();
            foreach (var item in items)
            {
                LMOItems.Add(item);
            }
        }


        private void LoadLMOItems()
        {
            string selectedMO = selectedLatheManufactureOrder.Name;
            FilteredLMOItems.Clear();
            foreach(var item in LMOItems)
            {
                if(item.AssignedMO == selectedMO)
                {
                    FilteredLMOItems.Add(item);
                }
            }
        }


        public void PrintSelectedOrder()
        {

            PDFHelper.PrintOrder(SelectedLatheManufactureOrder, FilteredLMOItems);

        }

        public void EditLMO()
        {
            MessageBox.Show("Hello, world");
        }

    }
}