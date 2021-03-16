using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class NewRequestViewModel : BaseViewModel
    {
        public ObservableCollection<TurnedProduct> turnedProducts { get; set; }
        public ObservableCollection<TurnedProduct> filteredList { get; set; }

        private TurnedProduct selectedProduct;

        public TurnedProduct SelectedProduct
        {
            get { return selectedProduct; }
            set 
            { 
                selectedProduct = value;
                OnPropertyChanged("SelectedProduct");
                SelectedProductChanged?.Invoke(this, new EventArgs());
            }
        }


        public List<string> Families { get; set; }
        public Request newRequest;
        private string selectedGroup;

        public string SelectedGroup
        {
            get { return selectedGroup; }
            set 
            { 
                selectedGroup = value;
                OnPropertyChanged("SelectedGroup");
                SelectedGroupChanged?.Invoke(this, new EventArgs());
                PopulateListBox();
            }
        }


        public event EventHandler SelectedGroupChanged;
        public event EventHandler SelectedProductChanged;

        public NewRequestViewModel()
        {
            ClearScreen();
        }

        private void PopulateComboBox()
        {
            var products = DatabaseHelper.Read<TurnedProduct>().ToList();
            turnedProducts.Clear();
            Families.Clear();
            
            foreach(var product in products)
            {
                turnedProducts.Add(product);
                if(!Families.Any(item => item.ToString() == product.ProductName.Substring(0, 5)))
                {
                    Families.Add(product.ProductName.Substring(0, 5));
                }
            }

            Families = Families.OrderBy(n => n).ToList();
        }
        
        public void PopulateListBox()
        {
            filteredList.Clear();
            foreach(var product in turnedProducts)
            {
                if(product.ProductName.Substring(0,5) == SelectedGroup)
                {
                    filteredList.Add(product);
                }
            }
        }

        public void SubmitRequest()
        {
            if (String.IsNullOrEmpty(SelectedProduct.ProductName))
            {
                MessageBox.Show("Please select a product!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            newRequest.Status = "Pending approval";
            newRequest.isProductionApproved = false;
            newRequest.isSchedulingApproved = false;
            newRequest.IsAccepted = false;
            newRequest.IsDeclined = false;
            newRequest.RaisedBy = String.Format("{0} {1}", App.currentUser.FirstName, App.currentUser.LastName);
            newRequest.DateRaised = DateTime.Now;
            newRequest.Product = selectedProduct.ProductName;
            newRequest.DeclinedReason = "";

            if (DatabaseHelper.Insert(newRequest))
            {
                MessageBox.Show("Your request has been submitted", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
            }
            else
            {
                MessageBox.Show("An error has occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ClearScreen()
        {
            turnedProducts = new ObservableCollection<TurnedProduct>();
            filteredList = new ObservableCollection<TurnedProduct>();
            newRequest = new Request();
            Families = new List<string>();
            SelectedGroup = "";
            selectedProduct = new TurnedProduct();

            PopulateComboBox();
            PopulateListBox();
        }

    }
}
