using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class BarStockViewModel : BaseViewModel
    {
        #region Variables
        public List<BarStock> BarStock { get; set; }

        public List<BarStockRequirementOverview> BarOverviews { get; set; }
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
                OnPropertyChanged();
            }
        }

        public List<LatheManufactureOrder> Orders { get; set; }
        public List<BarStockRequirementOverview> BarStockOverview { get; set; }
        public PrintBarRequisitionCommand PrintCommand { get; set; }

        private string searchString;
        public string SearchString
        {
            get { return searchString; }
            set 
            { 
                searchString = value; 
                FilterBars(searchString); 
                OnPropertyChanged(); 
            }
        }


        public Visibility BarStockVis { get; set;  } = Visibility.Visible;
        public Visibility NoneFoundVis { get; set; } = Visibility.Hidden;


        public double CostOfNewBar { get; set; }
        public double NumberOfBars { get; set; }
        #endregion

        public BarStockViewModel()
        {
            BarStock = new();
            Orders = new();
            BarOverviews = new();
            BarStockOverview = new();
            FilteredBarOverviews = new();
            PrintCommand = new(this);
            CostOfNewBar = new();
            NumberOfBars = new();

            SearchString = "";

            LoadData();
            FilterBars();

            //MigrateBarModel();
        }


        void MigrateBarModel()
        {
            List<LatheManufactureOrder> allOrders = DatabaseHelper.Read<LatheManufactureOrder>();

            foreach (LatheManufactureOrder order in allOrders)
            {
                if (!order.BarIsAllocated) continue;  /////// CHECK THIS BEFORE RUNNING

                order.NumberOfBarsIssued = (int)System.Math.Ceiling(order.NumberOfBars);
                BarIssue retrospectiveIssue = new()
                {
                    Date = order.CreatedAt,
                    IssuedBy = "system",
                    BarId = order.BarID,
                    MaterialBatch = "n/a",
                    OrderId = order.Name,
                    Quantity = order.NumberOfBarsIssued,
                    MaterialInfo = "-"
                };

                DatabaseHelper.Update(order);
                DatabaseHelper.Insert(retrospectiveIssue);
            }
        }
        public void LoadData()
        {
            BarStock = DatabaseHelper.Read<BarStock>()
                .OrderBy(b => b.Material)
                .ThenBy(b => b.Size)
                .ToList();

            BarOverviews.Clear();

            Orders.Clear();
            Orders = DatabaseHelper.Read<LatheManufactureOrder>().Where(x => x.State < OrderState.Complete).ToList();

            for (int i = 0; i < BarStock.Count; i++)
            {
                BarOverviews.Add(new(BarStock[i], Orders.Where(x => x.BarID == BarStock[i].Id).ToList()));
            }
        }

        public void FilterBars(string searchString = "")
        {
            searchString = searchString.Trim().ToUpperInvariant();

            if (string.IsNullOrEmpty(searchString))
            {
                FilteredBarOverviews = BarOverviews.Where(x => x.FreeBar > 0 || x.BarsRequiredForOrders > 0).ToList();
            }
            else
            {
                FilteredBarOverviews = BarOverviews.Where(x => x.BarStock.Id.ToUpperInvariant().Contains(searchString)).ToList();
            }

            FilteredBarOverviews = FilteredBarOverviews.OrderBy(x => x.Priority).ToList();

            if (FilteredBarOverviews.Count > 0)
            {
                SelectedBarStock = FilteredBarOverviews[0];
                NoneFoundVis = Visibility.Hidden;
                BarStockVis = Visibility.Visible;
            }
            else
            {
                NoneFoundVis = Visibility.Visible;
                BarStockVis = Visibility.Hidden;
            }

            OnPropertyChanged(nameof(NoneFoundVis));
            OnPropertyChanged(nameof(BarStockVis));
        }

        public void PrintRequisition()
        {
            PDFHelper.PrintBarRequisition(BarStockOverview);
        }
    }
}
