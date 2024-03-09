using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.View.Administration;
using ProjectLighthouse.View.Orders;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Commands.Orders;
using ProjectLighthouse.ViewModel.Commands.Printing;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Orders
{
    public class BarStockViewModel : BaseViewModel
    {
        #region Variables
        public List<BarStock> BarStock { get; set; }
        public List<MaterialInfo> MaterialData { get; set; }

        public List<BarStockRequirementOverview> BarOverviews { get; set; }
        public List<BarIssue> BarIssues { get; set; }
        private List<BarStockRequirementOverview> filteredBarOverviews;

        public List<BarStockRequirementOverview> FilteredBarOverviews
        {
            get { return filteredBarOverviews; }
            set { filteredBarOverviews = value; OnPropertyChanged(); }
        }


        private BarStockRequirementOverview selectedBarStock;
        public BarStockRequirementOverview SelectedBarStock
        {
            get { return selectedBarStock; }
            set
            {
                selectedBarStock = value;
                if (value is not null)
                {
                    DependentOrdersVis = value.Orders.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                    OnPropertyChanged(nameof(DependentOrdersVis));
                }
                RecommendPurchaseOrder();
                OnPropertyChanged();
            }
        }

        public List<LatheManufactureOrder> Orders { get; set; }
        public List<BarStockRequirementOverview> BarStockOverview { get; set; }
        public PrintBarRequisitionCommand PrintCommand { get; set; }
        public AddBarCommand AddBarCmd { get; set; }

        private string searchString;
        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value;
                FilterBars(searchString, ShowRequisitionsOnly);
                OnPropertyChanged();
            }
        }

        private string? suggestedOrderText;

        public string? SuggestedOrderText
        {
            get { return suggestedOrderText; }
            set
            {
                suggestedOrderText = value;
                OnPropertyChanged();
            }
        }


        private bool showRequisitionsOnly;

        public bool ShowRequisitionsOnly
        {
            get { return showRequisitionsOnly; }
            set
            {
                showRequisitionsOnly = value;
                SearchString = "";
                FilterBars(requestsOnly: value);
                OnPropertyChanged();
            }
        }


        private LatheManufactureOrder selectedOrder;
        public LatheManufactureOrder SelectedOrder
        {
            get { return selectedOrder; }
            set { selectedOrder = value; OnPropertyChanged(); }
        }

        public Visibility DependentOrdersVis { get; set; } = Visibility.Hidden;


        public double CostOfNewBar { get; set; }
        public double NumberOfBars { get; set; }

        public IssueBarCommand IssueBarCmd { get; set; }
        #endregion

        public BarStockViewModel()
        {
            BarStock = new();
            MaterialData = new();
            Orders = new();
            BarOverviews = new();
            BarStockOverview = new();
            FilteredBarOverviews = new();
            PrintCommand = new(this);
            AddBarCmd = new(this);
            CostOfNewBar = new();
            NumberOfBars = new();
            BarIssues = new();

            IssueBarCmd = new(this);

            SearchString = "";

            LoadData();
            FilterBars();
        }

        public void CreateBarIssue()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("No order selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if ((SelectedOrder.StartDate.Date == DateTime.MinValue.Date || SelectedOrder.StartDate > DateTime.Now.AddMonths(1)) && App.CurrentUser.Role != UserRole.Administrator)
            {
                MessageBox.Show("Order will not be set in the next month", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CreateBarIssueWindow window = new()
            {
                Bar = SelectedBarStock.BarStock,
                Order = SelectedOrder,
                Owner = App.MainViewModel.MainWindow,
            };

            window.SetupInterface();

            window.ShowDialog();
            if (!window.Confirmed)
            {
                return;
            }

            string currentBar = SelectedBarStock.BarStock.Id;

            LoadData();
            FilterBars(SearchString, ShowRequisitionsOnly);
            SelectedBarStock = FilteredBarOverviews.Find(x => x.BarStock.Id == currentBar);

            if (SelectedBarStock == null)
            {
                if (FilteredBarOverviews.Count > 0)
                {
                    SelectedBarStock = FilteredBarOverviews.First();
                }
            }

        }

        public void LoadData()
        {
            BarStock = DatabaseHelper.Read<BarStock>()
                .OrderBy(b => b.MaterialId)
                .ThenBy(b => b.Size)
                .ToList();

            MaterialData = DatabaseHelper.Read<MaterialInfo>().ToList();

            BarStock.ForEach(x => x.MaterialData = MaterialData.Find(d => d.Id == x.MaterialId));

            BarOverviews = null;
            OnPropertyChanged(nameof(BarOverviews));
            BarOverviews = new();

            BarIssues.Clear();
            BarIssues = DatabaseHelper.Read<BarIssue>();

            Orders.Clear();
            Orders = DatabaseHelper.Read<LatheManufactureOrder>().Where(x => x.State < OrderState.Complete).ToList();

            for (int i = 0; i < BarStock.Count; i++)
            {
                List<LatheManufactureOrder> ordersUsingBar = Orders.Where(x => x.BarID == BarStock[i].Id && x.BarIsVerified).OrderByDescending(x => x.RequiresBar()).ThenBy(x => x.StartDate).ToList();
                for (int j = 0; j < ordersUsingBar.Count; j++)
                {
                    ordersUsingBar[j].BarIssues = BarIssues.Where(x => x.OrderId == ordersUsingBar[j].Name).ToList();
                }
                BarOverviews.Add(new(BarStock[i], ordersUsingBar));
            }
        }

        public void FilterBars(string searchString = "", bool requestsOnly = false)
        {
            searchString = searchString.Trim().ToUpperInvariant();

            if (string.IsNullOrEmpty(searchString))
            {
                FilteredBarOverviews = BarOverviews.Where(x => x.FreeBar > 0 || x.BarsRequiredForOrders > 0).ToList();
            }
            else
            {
                FilteredBarOverviews = BarOverviews.Where(x => x.BarStock.Id.ToUpperInvariant().Contains(searchString) || x.Orders.Any(o => o.Name.Contains(searchString))).ToList();
            }

            if (requestsOnly)
            {
                FilteredBarOverviews = FilteredBarOverviews.Where(x => x.Orders.Any(o => o.RequiresBar())).ToList();
            }

            FilteredBarOverviews = FilteredBarOverviews.OrderBy(x => x.Status).ThenBy(x => x.BarStock.MaterialId).ThenBy(x => x.BarStock.Size).ToList();

            if (FilteredBarOverviews.Count > 0)
            {
                SelectedBarStock = FilteredBarOverviews[0];
            }
            else
            {
                SelectedBarStock = null;
            }
        }

        void RecommendPurchaseOrder()
        {
            SuggestedOrderText = null;
            if (SelectedBarStock is null) return;

            bool requiresSuggestion = SelectedBarStock.FreeBar < 0 || SelectedBarStock.FreeBar < (int)Math.Floor(SelectedBarStock.BarStock.SuggestedStock * 0.1);
            if (!requiresSuggestion) return;

            int numBarsToBuy = SelectedBarStock.BarStock.SuggestedStock - (int)SelectedBarStock.FreeBar;
            if (numBarsToBuy == 0)
            {
                SuggestedOrderText = null;
            }
            else if (numBarsToBuy == 1)
            {
                SuggestedOrderText = $"Purchase order for {numBarsToBuy:0} bar suggested";
            }
            else
            {
                SuggestedOrderText = $"Purchase order for {numBarsToBuy:0} bars suggested";
            }
        }

        public void PrintRequisition()
        {
            PDFHelper.PrintBarRequisition(BarStockOverview);
        }

        public void AddNewBar()
        {
            NewBarWindow window = new()
            {
                Owner = App.MainViewModel.MainWindow,
            };

            window.ShowDialog();

            if (window.BarAdded)
            {
                SearchString = "";
                LoadData();
                SearchString = window.NewBar.Id;
            }
        }
    }
}
