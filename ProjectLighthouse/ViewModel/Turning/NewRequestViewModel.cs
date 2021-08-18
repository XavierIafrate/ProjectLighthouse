using LiveCharts;
using LiveCharts.Wpf;
using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class NewRequestViewModel : BaseViewModel
    {
        #region Variables
        private Request newRequest;
        public Request NewRequest
        {
            get { return newRequest; }
            set
            {
                newRequest = value;
                OnPropertyChanged("NewRequest");
            }
        }

        public List<TurnedProduct> TurnedProducts { get; set; }
        public List<TurnedProduct> FilteredList { get; set; }
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
                //OnPropertyChanged("RecommendedStockText");
                //OnPropertyChanged("PotentialQuantityText");

                if (selectedProduct != null)
                {
                    if (!selectedProduct.CanBeManufactured())
                    {
                        MessageBox.Show(selectedProduct.ProductName + " cannot be made on the lathes." + Environment.NewLine
                             + Environment.NewLine + "Reason:" + Environment.NewLine + selectedProduct.GetReasonCannotBeMade(), "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    CalculateInsights();
                }
            }
        }

        public List<MachineInfoSnippet> Snippets { get; set; }

        public List<string> Families { get; set; }

        private string selectedGroup;
        public string SelectedGroup
        {
            get { return selectedGroup; }
            set
            {
                selectedGroup = value;
                OnPropertyChanged("SelectedGroup");
                PopulateListBox();
                
            }
        }

        public NewSpecialPartCommand AddSpecialCommand { get; set; }
        public NewRequestCommand SubmitRequestCommand { get; set; }
        public event EventHandler SelectedGroupChanged;
        public event EventHandler SelectedProductChanged;

        //Graph stuff
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

            NewRequest = new();

            SubmitRequestCommand = new(this);
            AddSpecialCommand = new(this);

            NotEnoughDataVis = Visibility.Visible;
            GraphVis = Visibility.Hidden;

            ClearScreen();
            SelectedGroup = "P0130";
            PopulateMachineInsights();
        }

        private void PopulateMachineInsights()
        {
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>();
            Snippets = new List<MachineInfoSnippet>();

            foreach (Lathe lathe in lathes)
            {
                MachineInfoSnippet tmpSnippet = new()
                {
                    MachineID = lathe.Id,
                    MachineFullName = lathe.FullName,
                    LeadTime = GetMachineLeadTime(lathe.Id)
                };

                Snippets.Add(tmpSnippet);
            }
            OnPropertyChanged("Snippets");
        }

        private static TimeSpan GetMachineLeadTime(string MachineID)
        {
            int totalTime = 0;
            DateTime earliestStart = DateTime.MaxValue;

            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>().Where(n => n.AllocatedMachine == MachineID && !n.IsComplete).ToList();
            List<LatheManufactureOrderItem> items = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();

            foreach (LatheManufactureOrder order in orders)
            {
                foreach (LatheManufactureOrderItem item in items)
                {
                    if (order.Name == item.AssignedMO)
                        totalTime += item.CycleTime * item.TargetQuantity;
                }
            }

            return TimeSpan.FromSeconds(totalTime);
        }

        private void PopulateComboBox()
        {
            List<TurnedProduct> products = DatabaseHelper.Read<TurnedProduct>().ToList();
            TurnedProducts.Clear();
            Families.Clear();

            foreach (TurnedProduct product in products)
            {
                TurnedProducts.Add(product);
                if (!product.isSpecialPart)
                    if (!Families.Any(item => item.ToString() == product.ProductName.Substring(0, 5)))
                        Families.Add(product.ProductName.Substring(0, 5));

            }

            Families = Families.OrderBy(n => n).ToList();
            Families.Add("Specials");
        }

        public void PopulateListBox()
        {
            FilteredList.Clear();
            if (SelectedGroup == "Specials")
            {
                foreach (TurnedProduct product in TurnedProducts)
                {
                    if (product.isSpecialPart)
                        FilteredList.Add(product);
                }
            }
            else
            {
                foreach (TurnedProduct product in TurnedProducts)
                {
                    if (product.ProductName.Substring(0, Math.Min(5, product.ProductName.Length)) == SelectedGroup && !product.isSpecialPart)
                        FilteredList.Add(product);
                }
            }

            FilteredList = new List<TurnedProduct>(FilteredList.OrderBy(n => n.Material).ThenBy(n => n.ProductName));
            OnPropertyChanged("FilteredList");
            LoadGraph();
        }

        private void LoadGraph()
        {
            List<string> labels = new();
            ThousandsSeparator = value => string.Format($"{value:#,##0}");

            if (FilteredList.Count == 0)
            {
                GraphTitle = "Select a product group to view analytics";
                GraphVis = Visibility.Hidden;
                NotEnoughDataVis = Visibility.Visible;
            }

            for (int i = 0; i < FilteredList.Count; i++) // labels
            {
                TurnedProduct turnedProduct = FilteredList[i];
                if (!labels.Contains(turnedProduct.ProductGroup))
                    labels.Add(turnedProduct.ProductGroup);
            }

            XAxisLabels = labels.OrderBy(q => q).ToArray();
            SeriesCollection = new();
            System.Windows.Media.BrushConverter converter = new();

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
                int sumSold = FilteredList.Where(n => n.ProductGroup == label && !n.isSpecialPart).ToList().Sum(p => p.QuantitySold);
                _series.Values.Add(Convert.ToDouble(sumSold));
                totalSold += sumSold;
            }


            SeriesCollection.Add(_series);

            if (_series.Values.Count < 4 || totalSold < 10000)
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
            if (string.IsNullOrEmpty(SelectedProduct.ProductName))
            {
                MessageBox.Show("Please select a product!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return result;
            }
            else if (!selectedProduct.CanBeManufactured())
            {
                MessageBox.Show("This product can not be made on our machines!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return result;
            }

            if (newRequest.DateRequired <= DateTime.Now)
            {
                MessageBox.Show("Please select a date!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return result;
            }

            newRequest.RaisedBy = App.CurrentUser.GetFullName();
            newRequest.DateRaised = DateTime.Now;
            newRequest.Product = selectedProduct.ProductName;
            newRequest.Likeliness = LikelinessText;

            if (DatabaseHelper.Insert(newRequest))
            {
                if (!Debugger.IsAttached)
                {
                    string message = $"{App.CurrentUser.GetFullName()} has submitted a request for {newRequest.QuantityRequired:#,##0}pcs of {newRequest.Product}. {newRequest.Likeliness} ({RecommendedStockText}, {PotentialQuantityText}). Required for {newRequest.DateRequired:d MMMM}.";
                    SMSHelper.SendText("+447979606705", message);
                }

                Task.Run(() => EmailHelper.NotifyNewRequest(newRequest, SelectedProduct, App.CurrentUser));

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
            NewRequest = new();
            TurnedProducts = new();
            FilteredList = new();
            Families = new();
            SelectedGroup = "";
            SelectedProduct = new();

            PopulateComboBox();
            PopulateListBox();
            OnPropertyChanged("FilteredList");
            OnPropertyChanged("SelectedGroup");
            OnPropertyChanged("SelectedProduct");
        }

        public void CalculateInsights()
        {
            if (selectedProduct != null)
            {
                RecommendedStockText = $"Recommended for stock: {selectedProduct.GetRecommendedQuantity():#,##0} pcs";

                List<int> classQuantities = new();
                foreach (TurnedProduct product in FilteredList)
                {
                    if (product.ProductName != selectedProduct.ProductName && product.IsScheduleCompatible(selectedProduct))
                        classQuantities.Add(product.GetRecommendedQuantity());
                }

                classQuantities.Sort();
                classQuantities.Reverse();

                int tmpQuant = 0;
                for (int i = 0; i < Math.Min(classQuantities.Count, 3); i += 1)
                {
                    tmpQuant += classQuantities[i];
                }

                PotentialQuantityText = $"Schedule compatible: {tmpQuant:#,##0} pcs";

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
            AddSpecialPartWindow window = new();
            window.ShowDialog();
            if (!string.IsNullOrWhiteSpace(window.filename) &&
               !string.IsNullOrWhiteSpace(window.productName) &&
               !string.IsNullOrWhiteSpace(window.customerName) &&
               window.submitted)
            {
                TurnedProduct newSpecial = new()
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
    }
}
