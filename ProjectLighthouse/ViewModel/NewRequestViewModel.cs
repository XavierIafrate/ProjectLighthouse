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
        public string PotentialQuantityText { get; set; }
        public string RecommendedStockText { get; set; }
        public string LikelinessText { get; set; }



        private TurnedProduct selectedProduct;

        public TurnedProduct SelectedProduct
        {
            get { return selectedProduct; }
            set 
            { 
                selectedProduct = value;
                OnPropertyChanged("SelectedProduct");
                OnPropertyChanged("RecommendedStockText");
                OnPropertyChanged("PotentialQuantityText");
                SelectedProductChanged?.Invoke(this, new EventArgs());
                if (selectedProduct != null)
                {
                    if (!selectedProduct.canBeManufactured())
                    {
                        MessageBox.Show(selectedProduct.ProductName + " cannot be made on the lathes." + Environment.NewLine
                             + Environment.NewLine + "Reason:" + Environment.NewLine + selectedProduct.GetReasonCannotBeMade(), "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    CalculateInsights();
                }
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
            else if (!selectedProduct.canBeManufactured())
            {
                MessageBox.Show("This product can not be made on our machines!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (newRequest.DateRequired <= DateTime.Now)
            {
                MessageBox.Show("Please select a date!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            newRequest.Status = "Pending approval";
            newRequest.isProductionApproved = false;
            newRequest.isSchedulingApproved = false;
            newRequest.IsAccepted = false;
            newRequest.IsDeclined = false;
            newRequest.RaisedBy = App.currentUser.GetFullName();
            newRequest.DateRaised = DateTime.Now;
            newRequest.Product = selectedProduct.ProductName;
            newRequest.DeclinedReason = "";
            newRequest.Likeliness = LikelinessText;

            if (DatabaseHelper.Insert(newRequest))
            {
                string message = String.Format("{0} has submitted a request for {1:#,##0}pcs of {2}. {3} ({4}, {5}). Required for {6:d MMMM}.", 
                    App.currentUser.GetFullName(), 
                    newRequest.QuantityRequired, 
                    newRequest.Product, 
                    newRequest.Likeliness,
                    RecommendedStockText,
                    PotentialQuantityText,
                    newRequest.DateRequired);
                //SMSHelper.SendText("+447979606705", message);
                MessageBox.Show("Your request has been submitted", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearScreen();
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
            SelectedProduct = new TurnedProduct();

            PopulateComboBox();
            PopulateListBox();
            OnPropertyChanged("filteredList");
            OnPropertyChanged("SelectedGroup");
            OnPropertyChanged("SelectedProduct");

        }

        public void CalculateInsights()
        {
            if( selectedProduct != null)
            {
                RecommendedStockText = String.Format("Recommended for stock: {0} pcs", selectedProduct.GetRecommendedQuantity());
                

                List<int> classQuantities = new List<int>();
                foreach (var product in filteredList)
                {
                    if (product.ProductName != selectedProduct.ProductName && product.IsScheduleCompatible(selectedProduct))
                    {
                        classQuantities.Add(product.GetRecommendedQuantity());
                    }
                }

                classQuantities.Sort();
                classQuantities.Reverse();
                int tmpQuant = (int)0;
                for (int i = 0; i < Math.Min(classQuantities.Count, 3); i += 1)
                {
                    tmpQuant += classQuantities[i];
                }

                PotentialQuantityText = String.Format("Schedule compatible: {0} pcs", tmpQuant);

                LikelinessText = "";
                int hypothetical = selectedProduct.GetRecommendedQuantity() + tmpQuant + newRequest.QuantityRequired;
                if (hypothetical > 5000)
                {
                    LikelinessText = "Almost certain";
                }
                else if (hypothetical > 2000)
                {
                    LikelinessText = "Good chance";
                }
                else if (hypothetical > 500)
                {
                    LikelinessText = "Workload dependant";
                }
                else
                {
                    LikelinessText = "Unlikely";
                }

                LikelinessText = "Likeliness: " + LikelinessText;

                OnPropertyChanged("RecommendedStockText");
                OnPropertyChanged("LikelinessText");
                OnPropertyChanged("PotentialQuantityText");
            }
        }

    }
}
