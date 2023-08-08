using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Analytics;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
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

        public Axis[] YAxisStartAtZero { get; set; } =
        {
            new Axis
            {
                MinLimit=0
            }
        };

        public int TotalPartsMade { get; set; }
        public int TotalPartsMadeThisYear { get; set; }

        private OperatingEfficiencyKpi ooe;
        public OperatingEfficiencyKpi OOE
        {
            get { return ooe; }
            set
            {
                ooe = value;
                OnPropertyChanged();
            }
        }


        #endregion

        public AnalyticsViewModel()
        {
            GetAnalytics();
            GetWorkload();
            //GetOEE(
            //    start: DateTime.Today.AddDays((int)DateTime.Today.DayOfWeek * -1).AddDays(-7),
            //    end: DateTime.Today.AddDays((int)DateTime.Today.DayOfWeek * -1));

            GetProductAnalytics();
        }

        private void GetOEE(DateTime start, DateTime end)
        {
            //return;
            string lathe = "C03";
            List<MachineOperatingBlock> blocks = DatabaseHelper.Read<MachineOperatingBlock>()
                .Where(x => x.StateLeft >= start && x.StateEntered <= end && x.MachineID == lathe)
                .ToList();

            blocks = blocks.Denoise(start, end, 30 * 60);
            blocks = blocks.Slice(start);
            blocks = blocks.Slice(end);
            blocks = blocks.Where(x => x.StateEntered >= start && x.StateLeft <= end).ToList();



            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>()
                .Where(x => x.State < OrderState.Cancelled && x.AllocatedMachine == lathe && x.StartDate <= end && x.EndsAt() >= start)
                .OrderBy(x => x.StartDate)
                .ToList();

            //orders.ForEach(x => scheduleItems.Add(x));

            //Schedule s = new(scheduleItems);


            TimeSpan performanceChange = new();

            TimeSpan availabilityLoss = new();


            foreach (LatheManufactureOrder order in orders)
            {
                List<MachineOperatingBlock> orderBlocks = blocks.Slice(order.StartDate);
                orderBlocks = blocks.Slice((order.CompletedAt == DateTime.MinValue ? order.EndsAt() : order.CompletedAt.Date.AddHours(6)));

                orderBlocks = orderBlocks.Where(x => x.StateEntered >= order.StartDate && x.StateLeft <= (order.CompletedAt == DateTime.MinValue ? order.EndsAt() : order.CompletedAt.Date.AddHours(6))).ToList();

                availabilityLoss = availabilityLoss.Add(new(0, 0, orderBlocks.Where(x => x.State != "Running").Sum(x => (int)x.SecondsElapsed)));
                performanceChange = performanceChange.Add(new(0, 0, orderBlocks.Where(x => x.State == "Running").Sum(x => (int)x.SecondsElapsed / (x.CycleTime - order.TargetCycleTime))));
            }

            TimeSpan developmentTime = new();
            TimeSpan settingTime = new();
            TimeSpan maintenanceTime = new();

            List<ScheduleItem> scheduleItems = new();

            foreach (LatheManufactureOrder order in orders)
            {

                if (order.IsResearch)
                {
                    developmentTime = developmentTime.Add(new(
                        Math.Min(end.Ticks, order.StartDate.AddSeconds(Math.Max(order.TimeToComplete, 86400 * 2)).Ticks)
                        - Math.Max(start.Ticks, order.StartDate.Ticks)));
                }
                else
                {
                    if (order.StartDate >= start)
                    {
                        settingTime = settingTime.Add(new(6, 0, 0));
                    }
                    maintenanceTime = maintenanceTime.Add(new(0, 15, 0));
                }

                scheduleItems.Add(order);
            }

            TimeSpan scheduleLoss = Schedule.GetScheduleLoss(scheduleItems, start, end);

            List<MaintenanceEvent> maintenance = DatabaseHelper.Read<MaintenanceEvent>()
                .Where(x => x.AllocatedMachine == lathe && x.EndsAt() > start && x.StartDate < end)
                .ToList();
            foreach (MaintenanceEvent maintenanceEvent in maintenance)
            {
                maintenanceTime = maintenanceTime.Add(new(
                    Math.Min(end.Ticks, maintenanceEvent.EndsAt().Ticks)
                    - Math.Max(start.Ticks, maintenanceEvent.StartDate.Ticks)));
            }

            OperatingEfficiencyKpi kpi = new()
            {
                AvailableTime = end - start,
                MaintenanceLoss = maintenanceTime,
                DevelopmentLoss = developmentTime,
                ScheduleLoss = scheduleLoss,
                ChangeoverLoss = settingTime,
                AvailabilityLoss = availabilityLoss,
                PerformanceChange = performanceChange,
                QualityLoss = new(0, 0, 0)
            };

            kpi.OperationsTime = kpi.AvailableTime;

            OOE = kpi;
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

                if (!productionRecords.ContainsKey(product.Name))
                {
                    productionRecords.Add(product.Name, totalProduced);
                }
                else
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
                //new StepLineSeries<DateTimePoint>
                //{
                //    Values = depletion,
                //    Name = "Depletion",
                //    Fill=null,
                //    GeometryStroke=new SolidColorPaint(SKColors.Red) {StrokeThickness=2},
                //    Stroke=new SolidColorPaint(SKColors.Red) {StrokeThickness=2 },
                //    GeometrySize=0,
                //    TooltipLabelFormatter = (chartPoint) =>
                //    $"Forecast: {new DateTime((long) chartPoint.SecondaryValue):dd/MM/yy}: {chartPoint.PrimaryValue:0}",
                //}
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
    }
}
