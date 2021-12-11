using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class BarStockViewModel : BaseViewModel
    {
        #region Variables
        public List<BarStock> BarStock { get; set; }
        public List<LatheManufactureOrder> Orders { get; set; }
        public List<BarStockRequirementOverview> BarStockOverview { get; set; }
        public PrintBarRequisitionCommand PrintCommand { get; set; }

        private bool requisitionAvailable;

        public bool RequisitionAvailable
        {
            get { return requisitionAvailable; }
            set
            {
                requisitionAvailable = value;
                OnPropertyChanged("RequisitionAvailable");
            }
        }


        public double CostOfNewBar { get; set; }
        public double NumberOfBars { get; set; }

        private Visibility quickOrderVis;
        public Visibility QuickOrderVis
        {
            get { return quickOrderVis; }
            set
            {
                quickOrderVis = value;
                OnPropertyChanged("QuickOrderVis");
            }
        }
        #endregion

        public BarStockViewModel()
        {
            BarStock = new();
            Orders = new();
            BarStockOverview = new();
            PrintCommand = new(this);
            QuickOrderVis = Visibility.Hidden;

            CostOfNewBar = new();
            NumberOfBars = new();
            RequisitionAvailable = false;

            LoadData();
        }

        public void LoadData()
        {
            BarStock = DatabaseHelper.Read<BarStock>()
                .OrderBy(b => b.Material)
                .ThenBy(b => b.Size)
                .ToList();

            Orders = DatabaseHelper.Read<LatheManufactureOrder>();

            foreach (BarStock bar in BarStock)
            {
                BarStockOverview.Add(new(bar, Orders.Where(o => o.BarID == bar.Id
                    && o.State < OrderState.Complete
                    && o.BarIsVerified).ToList()));
            }

            BarStockOverview = BarStockOverview
                .Where(b => b.BarStock.InStock > 0 || b.Orders.Count > 0)
                .OrderBy(x => x.Priority)
                .ToList();

            foreach (BarStockRequirementOverview bar in BarStockOverview)
            {
                if (bar.FreeBar < 0)
                {
                    NumberOfBars += Math.Abs(bar.FreeBar);
                    CostOfNewBar += Math.Abs(bar.FreeBar) * bar.BarStock.Cost / 100;
                }
                foreach (LatheManufactureOrder order in bar.Orders)
                {
                    if (order.BarIsVerified && !order.BarIsAllocated && order.NumberOfBars < bar.BarStock.InStock && DateTime.Now.AddDays(14) >= order.StartDate)
                    {
                        RequisitionAvailable = true;
                    }
                }
            }

            if (CostOfNewBar >= 1000)
            {
                CostOfNewBar /= 100;
                CostOfNewBar = Math.Round(CostOfNewBar, 0);
                CostOfNewBar *= 100;
            }
            else
            {
                CostOfNewBar /= 10;
                CostOfNewBar = Math.Round(CostOfNewBar, 0);
                CostOfNewBar *= 10;
            }

            if (NumberOfBars > 0)
            {
                QuickOrderVis = Visibility.Visible;
            }

            OnPropertyChanged("NumberOfBars");
            OnPropertyChanged("CostOfNewBar");
            OnPropertyChanged("BarStockOverview");
        }

        public void PrintRequisition()
        {
            PDFHelper.PrintBarRequisition(BarStockOverview);
        }
    }
}
