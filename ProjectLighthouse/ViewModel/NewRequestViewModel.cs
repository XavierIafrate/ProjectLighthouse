using LiveCharts;
using LiveCharts.Wpf;
using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
{
    public class NewRequestViewModel : BaseViewModel
    {
        #region Variables
        public ObservableCollection<TurnedProduct> turnedProducts { get; set; }
        public ObservableCollection<TurnedProduct> filteredList { get; set; }
        public string PotentialQuantityText { get; set; }
        public string RecommendedStockText { get; set; }
        public string LikelinessText { get; set; }
        public string RequiredQtyPrefill { get; set; }

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
                    //int deficit = SelectedProduct.QuantityInStock - SelectedProduct.QuantityOnSO + SelectedProduct.QuantityOnPO;
                    //RequiredQtyPrefill = $"{Math.Abs(deficit)}";

                    OnPropertyChanged("RequiredQtyPrefill");


                    if (!selectedProduct.canBeManufactured())
                    {
                        MessageBox.Show(selectedProduct.ProductName + " cannot be made on the lathes." + Environment.NewLine
                             + Environment.NewLine + "Reason:" + Environment.NewLine + selectedProduct.GetReasonCannotBeMade(), "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    CalculateInsights();
                }
            }
        }

        public List<MachineInfoSnippet> snippets { get; set; }

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
                OnPropertyChanged("filteredList");
            }
        }

        public ICommand AddSpecialCommand { get; set; }
        public NewRequestCommand SubmitRequestCommand { get; set; }
        public event EventHandler SelectedGroupChanged;
        public event EventHandler SelectedProductChanged;


        //Graph stiff
        public string[] XAxisLabels { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> ThousandsSeparator { get; set; }
        public string GraphTitle { get; set; }
        public Visibility NotEnoughDataVis { get; set; }
        public Visibility GraphVis { get; set; }

        #endregion

        public NewRequestViewModel()
        {
            Debug.WriteLine("Init: NewRequestViewModel");

            SubmitRequestCommand = new NewRequestCommand(this);
            AddSpecialCommand = new NewSpecialPartCommand(this);

            NotEnoughDataVis = Visibility.Visible;
            GraphVis = Visibility.Hidden;

            ClearScreen();
            PopulateMachineInsights();
        }

        private void PopulateMachineInsights()
        {
            var lathes = DatabaseHelper.Read<Lathe>().ToList();
            snippets = new List<MachineInfoSnippet>();

            foreach (var lathe in lathes)
            {
                MachineInfoSnippet tmpSnippet = new MachineInfoSnippet
                {
                    MachineID = lathe.Id,
                    MachineFullName = lathe.FullName,
                    LeadTime = GetMachineLeadTime(lathe.Id)
                };

                snippets.Add(tmpSnippet);
            }
            OnPropertyChanged("snippets");
        }

        private TimeSpan GetMachineLeadTime(string MachineID)
        {
            int totalTime = (int)0;
            DateTime earliestStart = DateTime.MaxValue;

            var orders = DatabaseHelper.Read<LatheManufactureOrder>().Where(n => n.AllocatedMachine == MachineID && !n.IsComplete).ToList();
            var items = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();

            foreach (var order in orders)
            {
                foreach (var item in items)
                {
                    if (order.Name == item.AssignedMO)
                        totalTime += item.CycleTime * item.TargetQuantity;
                }
            }

            return TimeSpan.FromSeconds(totalTime);
        }

        private void PopulateComboBox()
        {
            var products = DatabaseHelper.Read<TurnedProduct>().ToList();
            turnedProducts.Clear();
            Families.Clear();

            foreach (var product in products)
            {
                turnedProducts.Add(product);
                if (!product.isSpecialPart)
                    if (!Families.Any(item => item.ToString() == product.ProductName.Substring(0, 5)))
                        Families.Add(product.ProductName.Substring(0, 5));

            }

            Families = Families.OrderBy(n => n).ToList();
            Families.Add("Specials");
        }

        public void PopulateListBox()
        {
            filteredList.Clear();
            if (SelectedGroup == "Specials")
            {
                foreach (var product in turnedProducts)
                {
                    if (product.isSpecialPart)
                        filteredList.Add(product);
                }
                return;
            }
            else
            {
                foreach (var product in turnedProducts)
                {
                    if (product.ProductName.Substring(0, Math.Min(5, product.ProductName.Length)) == SelectedGroup && !product.isSpecialPart)
                        filteredList.Add(product);
                }
            }
            filteredList = new ObservableCollection<TurnedProduct>(filteredList.OrderBy(n => n.Material).ThenBy(n => n.ProductName));
            LoadGraph();
        }

        private void LoadGraph()
        {
            List<string> labels = new();
            ThousandsSeparator = value => string.Format($"{value:#,##0}");

            if (filteredList.Count == 0)
            {
                GraphTitle = "Select a product group to view analytics";
                GraphVis = Visibility.Hidden;
                NotEnoughDataVis = Visibility.Visible;
            }

            for (int i = 0; i < filteredList.Count; i++) // labels
            {
                TurnedProduct turnedProduct = filteredList[i];
                if (!labels.Contains(turnedProduct.ProductGroup))
                    labels.Add(turnedProduct.ProductGroup);
            }

            XAxisLabels = labels.OrderBy(q => q).ToArray();
            SeriesCollection = new();
            var converter = new System.Windows.Media.BrushConverter();

            LineSeries _series = new()
            {
                Title = "Units Sold",
                Values = new ChartValues<double> { },
                PointGeometrySize = 10,
                LineSmoothness = 0,
                Foreground = (System.Windows.Media.Brush)converter.ConvertFromString("#6B303030")
            };

            int totalSold = 0;

            foreach (string label in XAxisLabels)
            {
                int sumSold = filteredList.Where(n => n.ProductGroup == label && !n.isSpecialPart).ToList().Sum(p=>p.QuantitySold);
                _series.Values.Add(Convert.ToDouble(sumSold));
                totalSold += sumSold;
            }


            SeriesCollection.Add(_series);

            if(_series.Values.Count < 4 || totalSold < 10000)
            {
                GraphTitle = "Lighthouse Analytics Not Available";
                GraphVis = Visibility.Hidden;
                NotEnoughDataVis = Visibility.Visible;
            }
            else
            {
                GraphTitle = string.Format($"At a Glance: {SelectedGroup}");
                GraphVis = Visibility.Visible;
                NotEnoughDataVis = Visibility.Hidden;
            }

            OnPropertyChanged("XAxisLabels");
            OnPropertyChanged("GraphTitle");
            OnPropertyChanged("GraphVis");
            OnPropertyChanged("NotEnoughDataVis");
            OnPropertyChanged("SeriesCollection");
            OnPropertyChanged("ThousandsSeparator");
        }

        public bool SubmitRequest()
        {
            bool result = false;
            if (String.IsNullOrEmpty(SelectedProduct.ProductName))
            {
                MessageBox.Show("Please select a product!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return result;
            }
            else if (!selectedProduct.canBeManufactured())
            {
                MessageBox.Show("This product can not be made on our machines!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return result;
            }

            if (newRequest.DateRequired <= DateTime.Now)
            {
                MessageBox.Show("Please select a date!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return result;
            }

            newRequest.Status = "Pending approval";
            newRequest.isProductionApproved = false;
            newRequest.isSchedulingApproved = false;
            newRequest.IsAccepted = false;
            newRequest.IsDeclined = false;
            newRequest.RaisedBy = App.CurrentUser.GetFullName();
            newRequest.DateRaised = DateTime.Now;
            newRequest.Product = selectedProduct.ProductName;
            newRequest.DeclinedReason = "";
            newRequest.Likeliness = LikelinessText;

            if (DatabaseHelper.Insert(newRequest))
            {
                string message = String.Format("{0} has submitted a request for {1:#,##0}pcs of {2}. {3} ({4}, {5}). Required for {6:d MMMM}.",
                    App.CurrentUser.GetFullName(),
                    newRequest.QuantityRequired,
                    newRequest.Product,
                    newRequest.Likeliness,
                    RecommendedStockText,
                    PotentialQuantityText,
                    newRequest.DateRequired);
                SMSHelper.SendText("+447979606705", message);
                MessageBox.Show("Your request has been submitted", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearScreen();
                result = true;
                return result;
            }
            else
            {
                MessageBox.Show("An error has occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return result;
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
            if (selectedProduct != null)
            {
                RecommendedStockText = String.Format("Recommended for stock: {0} pcs", selectedProduct.GetRecommendedQuantity());

                List<int> classQuantities = new List<int>();
                foreach (var product in filteredList)
                {
                    if (product.ProductName != selectedProduct.ProductName && product.IsScheduleCompatible(selectedProduct))
                        classQuantities.Add(product.GetRecommendedQuantity());
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

        public void AddSpecialRequest()
        {
            AddSpecialPartWindow window = new AddSpecialPartWindow();
            window.ShowDialog();
            if (!String.IsNullOrWhiteSpace(window.filename) &&
               !String.IsNullOrWhiteSpace(window.productName) &&
               !String.IsNullOrWhiteSpace(window.customerName) &&
               window.submitted)
            {
                TurnedProduct newSpecial = new TurnedProduct()
                {
                    ProductName = window.productName,
                    isSpecialPart = true,
                    CustomerRef = window.customerName,
                    DrawingFilePath = window.filename,
                    AddedBy = App.CurrentUser.UserName,
                    AddedDate = DateTime.Now,
                    ProductGroup = "Specials"
                };

                DatabaseHelper.Insert<TurnedProduct>(newSpecial);
                ClearScreen();
                SelectedGroup = "Specials";
            }
        }

        public class MachineInfoSnippet
        {
            public string MachineID { get; set; }
            public string MachineFullName { get; set; }
            public TimeSpan LeadTime { get; set; }
        }

        //public class ValueByGroup
        //{
        //    public string Group { get; set; }
        //    public double Value { get; set; }

        //    public ValueByGroup(string group, double value)
        //    {
        //        this.Value = value;
        //        this.Group = group;
        //    }
        //}
    }
}
