using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class BarStockViewModel : BaseViewModel
    {
        #region Variables
        public List<BarStock> BarStock { get; set; }
        public List<LatheManufactureOrder> Orders { get; set; }
        public List<BarStockRequirementOverview> BarStockOverview { get; set; }
        public SendMetalexEmailCommand EmailCommand { get; set; }
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
            EmailCommand = new(this);
            QuickOrderVis = Visibility.Collapsed;

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

            Orders = DatabaseHelper.Read<LatheManufactureOrder>();

            foreach (BarStock bar in BarStock)
            {
                BarStockOverview.Add(new(bar, Orders.Where(o => o.BarID == bar.Id
                && o.Status != "Complete").ToList()));
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

        public void ComposeEmail()
        {
            List<BarStockRequirementOverview> needsStock = BarStockOverview.Where(b => b.FreeBar < 0).OrderBy(b => b.BarStock.Material).ThenBy(b => b.BarStock.Size).ToList();

            string body = DateTime.Now.Hour < 13
                ? "Good morning,"
                : "Good afternoon,";

            body += Environment.NewLine;
            body += Environment.NewLine;

            body += "Could you please provide price and delivery for:";

            body += Environment.NewLine;
            body += Environment.NewLine;
            string material = null;
            foreach (BarStockRequirementOverview bar in needsStock)
            {
                if (bar.BarStock.Material != material && material != null)
                {
                    body += Environment.NewLine;
                }
                material = bar.BarStock.Material;
                if (Math.Ceiling(Math.Abs(bar.FreeBar)) == 1)
                {
                    body += $"{bar.BarStock.Id} - {Math.Ceiling(Math.Abs(bar.FreeBar))} bar" + Environment.NewLine;
                }
                else
                {
                    body += $"{bar.BarStock.Id} - {Math.Ceiling(Math.Abs(bar.FreeBar))} bars" + Environment.NewLine;
                }
            }
            body += Environment.NewLine + "Best regards,";

            string url = "mailto:sales@metalex.co.uk?subject=Automotion%20Components%20RFQ&body=" + HttpUtility.UrlEncode(body);

            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
