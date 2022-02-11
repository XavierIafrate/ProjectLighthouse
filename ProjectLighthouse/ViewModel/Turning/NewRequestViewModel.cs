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

        public List<TurnedProduct> ItemsOnOrder { get; set; }
        public List<Request> RecentlyDeclinedRequests { get; set; }

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
        public Visibility GIFVis { get; set; }
        public Visibility AddSpecialVisibility { get; set; }
        public Visibility HighLeadTimeVisibility { get; set; }

        #endregion

        public NewRequestViewModel()
        {
            NewRequest = new();
            SubmitRequestCommand = new(this);
            AddSpecialCommand = new(this);

            TurnedProducts = new();

            NotEnoughDataVis = Visibility.Visible;
            GraphVis = Visibility.Hidden;
            GIFVis = Visibility.Visible;
            HighLeadTimeVisibility = Visibility.Collapsed;

            ClearScreen();
            SelectedProduct = new();
            SelectedGroup = "Live";
            PopulateMachineInsights();
        }

        private void PopulateMachineInsights()
        {
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>();
            Snippets = new List<MachineInfoSnippet>();

            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>().Where(x => x.State < OrderState.Complete).ToList();
            List<LatheManufactureOrderItem> items = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();

            List<MachineService> servicing = DatabaseHelper.Read<MachineService>().Where(x => x.StartDate.AddSeconds(x.TimeToComplete) > DateTime.Now).ToList();
            List<ResearchTime> research = DatabaseHelper.Read<ResearchTime>().Where(x => x.StartDate.AddSeconds(x.TimeToComplete) > DateTime.Now).ToList();

            List<ScheduleItem> CompleteOrders = new();
            CompleteOrders.AddRange(research);
            CompleteOrders.AddRange(servicing);
            foreach (LatheManufactureOrder order in orders)
            {
                order.OrderItems = items.Where(i => i.AssignedMO == order.Name).ToList();
                CompleteOrders.Add(order);
            }

            TimeSpan L20_leadTimes = TimeSpan.Zero;
            int num_L20s = 0;

            for (int i = 0; i < lathes.Count; i++)
            {
                List<ScheduleItem> ordersForLathe = CompleteOrders.Where(o => o.AllocatedMachine == lathes[i].Id).ToList();
                Tuple<TimeSpan, DateTime> workload = WorkloadCalculationHelper.GetMachineWorkload(ordersForLathe);

                MachineInfoSnippet tmpSnippet = new()
                {
                    MachineID = lathes[i].Id,
                    MachineFullName = lathes[i].FullName,
                    LeadTime = workload.Item1
                };

                if (lathes[i].Model.StartsWith("L20"))
                {
                    num_L20s++;
                    L20_leadTimes += tmpSnippet.LeadTime;
                }

                Snippets.Add(tmpSnippet);
            }

            if (num_L20s > 0)
            {
                L20_leadTimes /= num_L20s;
                HighLeadTimeVisibility = L20_leadTimes.TotalDays > 33
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                OnPropertyChanged("HighLeadTimeVisibility");
            }

            OnPropertyChanged("Snippets");
        }

        private void PopulateComboBox()
        {
            List<TurnedProduct> products = DatabaseHelper.Read<TurnedProduct>().OrderBy(x => x.ProductName).ToList();
            TurnedProducts.Clear();
            Families.Clear();
            Families.Add("Live");

            foreach (TurnedProduct product in products)
            {
                TurnedProducts.Add(product);
                if (!product.isSpecialPart)
                {
                    if (!Families.Any(item => item.ToString() == product.ProductName[..5]))
                    {
                        Families.Add(product.ProductName[..5]);
                    }
                }
            }

            Families = Families.OrderBy(n => n).ToList();
            Families.Add("Specials");
        }

        public void PopulateListBox()
        {
            AddSpecialVisibility = Visibility.Collapsed;
            if (TurnedProducts.Count == 0) // called before loading
            {
                return;
            }

            RecentlyDeclinedRequests = DatabaseHelper.Read<Request>()
                .Where(r => r.DateRaised.AddDays(14) > DateTime.Now && r.IsDeclined)
                .ToList();

            FilteredList.Clear();
            if (SelectedGroup == "Specials")
            {
                if (App.CurrentUser.CanCreateSpecial)
                {
                    AddSpecialVisibility = Visibility.Visible;
                }

                FilteredList.AddRange(TurnedProducts.Where(p => p.isSpecialPart));
            }
            else if (SelectedGroup == "Live")
            {
                FilteredList.AddRange(TurnedProducts.Where(p => p.FreeStock() < 0));
            }
            else
            {
                foreach (TurnedProduct product in TurnedProducts)
                {
                    if (product.ProductName[..Math.Min(5, product.ProductName.Length)] == SelectedGroup && !product.isSpecialPart)
                    {
                        FilteredList.Add(product);
                    }
                }
            }

            List<LatheManufactureOrder> activeOrders = DatabaseHelper.Read<LatheManufactureOrder>()
                .Where(o => o.State < OrderState.Complete)
                .ToList();
            List<LatheManufactureOrderItem> activeOrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>().Where(i => activeOrders.Any(o => i.AssignedMO == o.Name)).ToList();

            activeOrders = null;
            ItemsOnOrder = new();

            for (int i = 0; i < activeOrderItems.Count; i++)
            {
                TurnedProduct productOnOrder = TurnedProducts.Single(p => p.ProductName == activeOrderItems[i].ProductName);
                productOnOrder.OrderReference = activeOrderItems[i].AssignedMO;
                productOnOrder.LighthouseGuaranteedQuantity = activeOrderItems[i].RequiredQuantity;
                ItemsOnOrder.Add(productOnOrder);
            }

            activeOrders = null;
            activeOrderItems = null;

            // populate recent Lighthouse decisions
            foreach (TurnedProduct item in FilteredList)
            {
                foreach (TurnedProduct orderItem in ItemsOnOrder)
                {
                    if (item.IsScheduleCompatible(orderItem))
                    {
                        item.OrderReference = orderItem.OrderReference;
                    }
                    if (item.ProductName == orderItem.ProductName)
                    {
                        item.IsAlreadyOnOrder = true;
                        item.LighthouseGuaranteedQuantity = orderItem.LighthouseGuaranteedQuantity;
                    }
                }

                if (!string.IsNullOrEmpty(item.OrderReference))
                {
                    continue;
                }

                foreach (Request declinedRequest in RecentlyDeclinedRequests)
                {
                    if (declinedRequest.Product == item.ProductName)
                    {
                        item.RecentlyDeclined = true;
                        break;
                    }
                }
            }

            if (SelectedGroup != "Live")
            {
                FilteredList = new(FilteredList.OrderBy(n => n.Material).ThenBy(x => x.DriveType).ThenBy(n => n.ProductName));
            }
            else
            {
                FilteredList = new(FilteredList
                    .Where(x => x.IsAlreadyOnOrder || x.QuantityOnPO == 0)
                    .OrderBy(n => n.Material)
                    .ThenBy(x => x.DriveType)
                    .ThenBy(n => n.ProductName));
            }

            if (FilteredList.Count > 0)
            {
                SelectedProduct = FilteredList.First();
            }
           
            OnPropertyChanged("FilteredList");
            OnPropertyChanged("AddSpecialVisibility");
            LoadGraph();
        }

        private void LoadGraph()
        {
            List<string> labels = new();
            ThousandsSeparator = value => $"{value:#,##0}";

            if (SelectedGroup is "Live" or "Specials")
            {
                GraphTitle = "Select a product group to view analytics";
                GraphVis = Visibility.Hidden;
                NotEnoughDataVis = Visibility.Visible;
                GIFVis = Visibility.Collapsed;
                OnPropertyChanged("GraphTitle");
                OnPropertyChanged("GraphVis");
                OnPropertyChanged("GIFVis");
                OnPropertyChanged("NotEnoughDataVis");
                return;
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
                GIFVis = Visibility.Visible;
            }
            else
            {
                GraphTitle = $"At a Glance: {SelectedGroup}";
                GraphVis = Visibility.Visible;
                NotEnoughDataVis = Visibility.Hidden;
            }

            OnPropertyChanged("XAxisLabels");
            OnPropertyChanged("GraphTitle");
            OnPropertyChanged("GraphVis");
            OnPropertyChanged("GIFVis");
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
            SelectedGroup = "Live";
            SelectedProduct = new();
            RecentlyDeclinedRequests = new();

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
                RecommendedStockText = $"{selectedProduct.GetRecommendedQuantity():#,##0} pcs";

                List<int> classQuantities = new();
                foreach (TurnedProduct product in TurnedProducts)
                {
                    if (product.ProductName != selectedProduct.ProductName && product.IsScheduleCompatible(selectedProduct))
                    {
                        classQuantities.Add(product.GetRecommendedQuantity());
                    }
                }

                classQuantities.Sort();
                classQuantities.Reverse();

                int tmpQuant = 0;
                for (int i = 0; i < Math.Min(classQuantities.Count, 3); i += 1)
                {
                    tmpQuant += classQuantities[i];
                }

                PotentialQuantityText = $"{tmpQuant:#,##0} pcs";

                LikelinessText = "";
                int hypothetical = selectedProduct.GetRecommendedQuantity() + tmpQuant + newRequest.QuantityRequired;
                if (hypothetical == 2147483647)
                {
                    LikelinessText = "Upper limit of the software";
                }
                else if (hypothetical > 500000)
                {
                    LikelinessText = "1000yr lead time";
                }
                else if (hypothetical > 40000)
                {
                    LikelinessText = "Seriously?";
                }
                else if (hypothetical > 3000)
                {
                    LikelinessText = "Strong";
                }
                else if (hypothetical > 1500)
                {
                    LikelinessText = "Good";
                }
                else
                {
                    LikelinessText = hypothetical > 500
                        ? "Fair"
                        : "Unlikely";
                }
            }
            else
            {
                RecommendedStockText = "";
                LikelinessText = "";
                PotentialQuantityText = "";
            }

            OnPropertyChanged("RecommendedStockText");
            OnPropertyChanged("LikelinessText");
            OnPropertyChanged("PotentialQuantityText");
        }

        public void AddSpecialRequest()
        {
            AddSpecialPartWindow window = new();
            window.ShowDialog();

            if (window.SaveExit)
            {
                if (DatabaseHelper.Insert(window.NewProduct))
                {
                    MessageBox.Show($"Successfully added {window.NewProduct.ProductName} to Specials.", "Success");
                    ClearScreen();
                }
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
