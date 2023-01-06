using IO.ClickSend.ClickSend.Model;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using ProjectLighthouse.Model.Orders;
using System.Linq;
using LiveChartsCore.Defaults;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class AnalyticsViewModel : BaseViewModel
    {
        #region Vars

        private AnalyticsHelper analytics;

        public AnalyticsHelper Analytics
        {
            get { return analytics; }
            set
            {
                analytics = value;
                OnPropertyChanged();
            }
        }

        public Axis[] XAxes { get; set;  }
        public List<string> YearsAvailable { get; set; }
        private string selectedYear;

        public string SelectedYear
        {
            get { return selectedYear; }
            set 
            { 
                selectedYear = value;
                SetGraphPagination(selectedYear);
                OnPropertyChanged();
            }
        }

        public ISeries[] Series { get; set; }

        public int TotalPartsMade { get; set; }
        public int TotalPartsMadeThisYear { get; set; }

        #endregion

        #region Commands
        public SendRuntimeReportCommand RuntimeReportCommand { get; set; }
        #endregion

        public AnalyticsViewModel()
        {
            //Analytics = new();
            GetAnalytics();

            RuntimeReportCommand = new(this);
        }

        private void GetAnalytics()
        {
            List<Lot> stockLots = DatabaseHelper.Read<Lot>();

            TotalPartsMade = stockLots.Where(x => x.IsDelivered).Sum(x => x.Quantity);
            TotalPartsMadeThisYear = stockLots.Where(x => x.IsDelivered && x.Date.Year == DateTime.Now.Year)
                .Sum(x => x.Quantity);

            GetChartData(stockLots);
        }

        private void GetChartData(List<Lot> stockLots)
        {
            int cutoffYear = DateTime.Today.Year - 5;
            List<IGrouping<DateTime, Lot>> chartLots = stockLots
                .Where(x => x.IsDelivered && x.Date.Year >= cutoffYear)
                .OrderBy(x => x.Date)
                .GroupBy(x => x.Date.Date)
                .ToList();

            List<DateTimePoint> chartData = new();

            int total = stockLots
                .Where(x => x.IsDelivered && x.Date.Year < cutoffYear)
                .Sum(x => x.Quantity);

            YearsAvailable = new() { "All" };

            YearsAvailable.AddRange(chartLots
                .Select(x => x.Key.Year.ToString("0"))
                .Distinct()
                .ToList());

            for (int i = 0; i < chartLots.Count; i++)
            {
                total += chartLots[i].Sum(x => x.Quantity);
                chartData.Add(new(chartLots[i].Key, total));
            }

            Series = new ISeries[]
            {
                new LineSeries<DateTimePoint>
                {
                    Values = chartData,
                    Name = "Total Parts Made",
                    Fill=null,
                    GeometrySize=0,
                    TooltipLabelFormatter = (chartPoint) =>
                    $"{new DateTime((long) chartPoint.SecondaryValue):dd/MM/yy}: {chartPoint.PrimaryValue:#,##0}",
                }
            };

            SelectedYear = "All";

            XAxes = new[] { new Axis() { Labeler = value => new DateTime((long)value).ToString("MM/yyyy") } };
            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(YearsAvailable));
        }

        void SetGraphPagination(string filter)
        {
            if (XAxes is null) return;

            var axis = XAxes[0];
            
            if (filter == "All")
            {
                axis.MinLimit = null;
                axis.MaxLimit = null;
            }
            else
            {
                int year = int.Parse(filter);
                DateTime yearDate = new(year, 1, 1);

                axis.MinLimit = yearDate.Ticks;
                axis.MaxLimit = yearDate.AddYears(1).Ticks;
            }
        }

        public void SendRuntimeReport(bool test)
        {
            EmailHelper emailHelper = new();

            List<EmailRecipient> emailRecipients = new();
            emailRecipients.Add(new(email: "x.iafrate@wixroydgroup.com", name: "Xavier Iafrate"));
            
            if (!test)
            {
                emailRecipients.Add(new(email: "r.budd@wixroydgroup.com", name: "Richard Budd"));
                emailRecipients.Add(new(email: "m.iafrate@wixroyd.com", name: "Marcus Iafrate"));
                emailRecipients.Add(new(email: "a.harandizadeh@wixroydgroup.com", name: "Arian Harandizadeh"));
            }

            if (DateTime.Today.DayOfWeek == DayOfWeek.Monday)
            {
                emailHelper.SendDailyRuntimeReport(emailRecipients, new AnalyticsHelper(), DateTime.Today.AddDays(-3).AddHours(6));
                emailHelper.SendDailyRuntimeReport(emailRecipients, new AnalyticsHelper(), DateTime.Today.AddDays(-2).AddHours(6));
            }
            emailHelper.SendDailyRuntimeReport(emailRecipients, new AnalyticsHelper());
        }
    }
}
