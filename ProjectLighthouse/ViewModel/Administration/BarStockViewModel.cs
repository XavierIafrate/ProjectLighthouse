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
        private BarStock selectedBarStock;

        public BarStock SelectedBarStock
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


        public double CostOfNewBar { get; set; }
        public double NumberOfBars { get; set; }
        #endregion

        public BarStockViewModel()
        {
            BarStock = new();
            Orders = new();
            BarStockOverview = new();
            PrintCommand = new(this);
            SelectedBarStock = new();
            CostOfNewBar = new();
            NumberOfBars = new();

            LoadData();
        }

        public void LoadData()
        {
            BarStock = DatabaseHelper.Read<BarStock>()
                .OrderBy(b => b.Material)
                .ThenBy(b => b.Size)
                .ToList();
        }

        public void PrintRequisition()
        {
            PDFHelper.PrintBarRequisition(BarStockOverview);
        }
    }
}
