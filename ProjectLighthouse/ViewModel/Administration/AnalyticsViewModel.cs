using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class AnalyticsViewModel : BaseViewModel
    {
        #region Vars
        public Axis[] XAxes { get; set; }
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
        public ISeries[] TurnaroundTime { get; set; }
        public ISeries[] ActiveOrders { get; set; }
        public ISeries[] ProductSeries { get; set; }
        public ISeries[] BreakdownFrequency { get; set; }
        public ISeries[] BreakdownTime { get; set; }
        public Axis[] BreakdownTimeXAxes { get; set; }

        public Axis[] YAxisStartAtZero { get; set; } =
        {
            new Axis
            {
                MinLimit=0
            }
        };

        public int TotalPartsMade { get; set; }
        public int TotalPartsMadeThisYear { get; set; }

        #endregion

        public AnalyticsViewModel()
        {
            GetAnalytics();
            GetWorkload();
            GetProductAnalytics();
            GetBreakdownAnalytics();
        }


        private void GetBreakdownAnalytics()
        {
            return;
            List<MachineBreakdown> breakdowns = DatabaseHelper.Read<MachineBreakdown>().Where(x => x.BreakdownStarted > DateTime.Today.AddDays(-90)).ToList();
            List<BreakdownCode> codes = DatabaseHelper.Read<BreakdownCode>();
            breakdowns.ForEach(b => b.BreakdownMeta = codes.Find(c => c.Id == b.BreakdownCode));

            //List<double> hours = new();
            List<string> codeLabels = new();

            List<ISeries> newSeries = new();
            foreach (BreakdownCode code in codes)
            {
                double timeAttributed = TimeSpan.FromSeconds(breakdowns.Where(x => x.BreakdownCode == code.Id).Sum(x => x.TimeElapsed)).Days;
                if (timeAttributed == 0) continue;

                List<double> newValue = new() { timeAttributed };
                codeLabels.Add(code.Id);
                newSeries.Add(new ColumnSeries<double>
                {
                    Values = newValue,
                    Name = code.Id,
                    TooltipLabelFormatter = (chartPoint) =>
                    $"[{code.Id}] {code.Name}: {chartPoint.PrimaryValue:#,##0} days",
                });
            }

            


            BreakdownTime = newSeries.ToArray();

            BreakdownTimeXAxes = new Axis[1];
            BreakdownTimeXAxes[0] = new()
            {
                Labels = codeLabels,
            };


            OnPropertyChanged(nameof(BreakdownTime));
            OnPropertyChanged(nameof(BreakdownTimeXAxes));
        }

        private void GetProductAnalytics()
        {
            List<Lot> stockLots = DatabaseHelper.Read<Lot>().Where(x => x.IsDelivered).ToList();
            List<TurnedProduct> turnedProducts = DatabaseHelper.Read<TurnedProduct>();
            List<ProductGroup> groups = DatabaseHelper.Read<ProductGroup>();
            List<Product> products = DatabaseHelper.Read<Product>();

            Dictionary<string, int> productionRecords = new();

            string[] uniqueProducts = stockLots.Select(x => x.ProductName).Distinct().ToArray();

            foreach (string productName in uniqueProducts)
            {
                int totalProduced = stockLots.Where(x => x.ProductName == productName).Sum(x => x.Quantity);

                TurnedProduct? sku = turnedProducts.Find(x => x.ProductName == productName);

                if (sku is null) continue;
                if (sku.GroupId is null) continue;

                ProductGroup? group = groups.Find(x => x.Id == sku.GroupId);
                if (group is null) continue;
                if (group.ProductId is null) continue;

                Product? product = products.Find(x => x.Id == group.ProductId);
                if (product is null) continue;

                if (!productionRecords.TryAdd(product.Name, totalProduced))
                {
                    productionRecords[product.Name] += totalProduced;
                }
            }

            List<KeyValuePair<string, int>> result = productionRecords
                .ToList()
                .OrderByDescending(x => x.Value)
                .Take(7)
                .ToList();

            int sumAccountedFor = result.Sum(x => x.Value);

            result.Add(new("Other", stockLots.Sum(x => x.Quantity) - sumAccountedFor));

            List<ISeries> pieChartValues = new();

            foreach (KeyValuePair<string, int> item in result)
            {
                pieChartValues.Add(new PieSeries<int>
                {
                    Name = item.Key,
                    Values = new List<int> { item.Value },
                    InnerRadius = 50,
                    TooltipLabelFormatter = (chartPoint) => $"{chartPoint.Context.Series.Name}: {chartPoint.PrimaryValue:#,##0}",
                    DataLabelsFormatter = (chartPoint) => $"{chartPoint.Context.Series.Name}: {chartPoint.PrimaryValue:#,##0}",
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(30, 30, 30))
                });
            }

            ProductSeries = pieChartValues.ToArray();
            OnPropertyChanged(nameof(ProductSeries));
        }

        private void GetAnalytics()
        {
            List<Lot> stockLots = DatabaseHelper.Read<Lot>();

            TotalPartsMade = stockLots.Where(x => x.IsDelivered).Sum(x => x.Quantity);
            TotalPartsMadeThisYear = stockLots.Where(x => x.IsDelivered && x.Date.Year == DateTime.Now.Year)
                .Sum(x => x.Quantity);

            OnPropertyChanged(nameof(TotalPartsMade));
            OnPropertyChanged(nameof(TotalPartsMadeThisYear));

            GetChartData(stockLots);
        }

        class WorkloadDay
        {
            public DateTime Day { get; set; }
            public string WeekId { get; set; }
            public int CountOfOrders { get; set; }
            public int CountOfProductionOrders { get; set; }
            public int CountOfDevelopmentOrders { get; set; }
            public double AverageTurnaroundTime { get; set; }
        }

        private void GetWorkload()
        {
            List<WorkloadDay> workload = new();

            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>()
                                                    .Where(x =>
                                                        x.State < OrderState.Cancelled
                                                        && x.StartDate.Date.AddMonths(18) > DateTime.Now
                                                        && !string.IsNullOrEmpty(x.AllocatedMachine))
                                                    .OrderBy(x => x.StartDate)
                                                    .ToList();

            List<DateTimePoint> turnaroundPoints = new();

            List<DateTimePoint> totalOrders = new();
            List<DateTimePoint> productionOrders = new();
            List<DateTimePoint> developmentOrders = new();


            for (int i = 0; i < 365; i++)
            {
                DateTime date = DateTime.Today.AddDays(i * -1);

                WorkloadDay data = new()
                {
                    Day = date,
                    WeekId = $"W{ISOWeek.GetWeekOfYear(date):00}-{date:yyyy}"
                };

                List<LatheManufactureOrder> ordersAtTime = orders.Where(x => x.CreatedAt.Date <= date && x.EndsAt() >= date).ToList();

                data.CountOfOrders = ordersAtTime.Count;
                data.CountOfProductionOrders = ordersAtTime.Where(x => !x.IsResearch).Count();
                data.CountOfDevelopmentOrders = ordersAtTime.Where(x => x.IsResearch).Count();

                TimeSpan totalWorkTime = new();
                ordersAtTime
                    .Where(x => !x.IsResearch)
                    .ToList()
                    .ForEach(x => totalWorkTime += x.EndsAt() - x.CreatedAt);

                data.AverageTurnaroundTime = totalWorkTime.TotalDays / 7;
                data.AverageTurnaroundTime /= ordersAtTime.Where(x => !x.IsResearch).Count();

                workload.Add(data);
            }

            workload.Reverse();

            foreach (WorkloadDay data in workload)
            {
                turnaroundPoints.Add(new(data.Day, data.AverageTurnaroundTime));

                totalOrders.Add(new(data.Day, data.CountOfOrders));
                productionOrders.Add(new(data.Day, data.CountOfProductionOrders));
                developmentOrders.Add(new(data.Day, data.CountOfDevelopmentOrders));
            }

            List<DateTimePoint> depletion = new();

            for (int i = 0; i < 60; i++)
            {
                DateTime date = DateTime.Today.AddDays(i);

                int count = orders.Where(x => x.EndsAt() > date).Count();

                depletion.Add(new(date, count));
            }

            TurnaroundTime = new ISeries[]
            {
                new LineSeries<DateTimePoint>
                {
                    Values = turnaroundPoints,
                    Name = "Average Production Turnaround",
                    Fill=null,
                    GeometrySize=0,
                    TooltipLabelFormatter = (chartPoint) =>
                    $"{new DateTime((long) chartPoint.SecondaryValue):dd/MM/yy}: {chartPoint.PrimaryValue:0.0} weeks",
                }
            };

            ActiveOrders = new ISeries[]
            {
                new StepLineSeries<DateTimePoint>
                {
                    Values = totalOrders,
                    Name = "Total",
                    Fill=null,
                    GeometrySize=0,
                    GeometryStroke= new SolidColorPaint(SKColors.DarkGray) {StrokeThickness=3},
                    Stroke=new SolidColorPaint(SKColors.DarkGray) {StrokeThickness=3},
                    TooltipLabelFormatter = (chartPoint) =>
                    $"Total: {new DateTime((long) chartPoint.SecondaryValue):dd/MM/yy}: {chartPoint.PrimaryValue:0}",
                },
                new StepLineSeries<DateTimePoint>
                {
                    Values = productionOrders,
                    Name = "Production",
                    Fill=null,
                    GeometryStroke=new SolidColorPaint(SKColors.ForestGreen) {StrokeThickness=2},
                    Stroke=new SolidColorPaint(SKColors.ForestGreen) {StrokeThickness=2},
                    GeometrySize=0,
                    TooltipLabelFormatter = (chartPoint) =>
                    $"Production: {new DateTime((long) chartPoint.SecondaryValue):dd/MM/yy}: {chartPoint.PrimaryValue:0}",
                },
                new StepLineSeries<DateTimePoint>
                {
                    Values = developmentOrders,
                    Name = "Development",
                    Fill=null,
                    GeometryStroke=new SolidColorPaint(SKColors.DodgerBlue) {StrokeThickness=2},
                    Stroke=new SolidColorPaint(SKColors.DodgerBlue) {StrokeThickness=2},
                    GeometrySize=0,
                    TooltipLabelFormatter = (chartPoint) =>
                    $"Development: {new DateTime((long) chartPoint.SecondaryValue):dd/MM/yy}: {chartPoint.PrimaryValue:0}",
                },
            };
        }

        private void GetChartData(List<Lot> stockLots)
        {
            int cutoffYear = DateTime.Today.Year - 5;
            List<IGrouping<DateTime, Lot>> chartLots = stockLots
                .Where(x => x.IsDelivered && x.Date.Year >= cutoffYear)
                .OrderBy(x => x.Date)
                .GroupBy(x => x.Date.Date)
                .ToList();

            if (stockLots.Count == 0)
            {
                return;
            }

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

            Axis axis = XAxes[0];

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
    }
}
