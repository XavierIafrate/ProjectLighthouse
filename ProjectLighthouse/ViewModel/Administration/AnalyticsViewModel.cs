using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Model.Analytics;
using ProjectLighthouse.Model.Analytics;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using static Model.Analytics.KpiReport;

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
        public IEnumerable<ISeries> GaugeSeries { get; set; }
        public ISeries[] TestSeries { get; set; }
        public ISeries[] KpiSeries { get; set; }

        public ISeries[] WeeklyKpis { get; set; }

        public string TitleText { get; set; }
        public Axis[] KpiXAxes { get; set; }
        public Axis[] YAxes { get; set; } =
        {
            new Axis
            {
                Labels = new string[]
                {
                    "Quality Loss",
                    "Cycle Time",
                    "Not Running", 
                    //"Breakdown", 
                    "Setting",
                    "Schedule Loss",
                    "Maintenance",
                    "Development",
                    "Available",
                },
                LabelsRotation=0,
                MinStep=1,
                ForceStepToMin=true,
                MinLimit=-0.5,
                MaxLimit=7.5,
            }
        };

        public Axis[] KpiOverTimeXAxes { get; set; }

        public int TotalPartsMade { get; set; }
        public int TotalPartsMadeThisYear { get; set; }

        #endregion

        #region Commands
        public SendRuntimeReportCommand RuntimeReportCommand { get; set; }
        #endregion

        public AnalyticsViewModel()
        {
            Analytics = new();

            RuntimeReportCommand = new(this);

            //Task.Run(() => GetAnalytics());
            GetAnalytics();

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
            //KpiReport report = new(start: new(2023, 1, 10), daysSpan: 7);

            //GetKpis();
            //GetMachineHistoryGraph();
        }

        private void GetMachineHistoryGraph()
        {
            int span = 7;

            DateTime date = DateTime.Today.AddDays(-span);

            List<MachineStatistics> machineStates = DatabaseHelper.QueryMachineHistory(date);

            machineStates = machineStates
                .OrderBy(x => x.DataTime)
                .Where(x => x.MachineID == "C01")
                .ToList();


            List<DateTimePoint> points = new();

            for (int i = 0; i < span; i++)
            {
                List<MachineStatistics> stats = machineStates.Where(x => x.DataTime.Date == date).ToList();

                for (int j = 0; j < 4; j++)
                {
                    List<MachineStatistics> statsOnHour = stats
                        .Where(x => x.DataTime.Hour == j * 6)
                        .ToList();

                    double? maxParts = null;

                    if (statsOnHour.Count > 0)
                    {
                        maxParts = statsOnHour.Max(x => x.PartCountAll);
                    }


                    DateTime dateAndTime = date.AddHours(j * 6);
                    points.Add(new(dateAndTime, maxParts));
                }

                date = date.AddDays(1);
            }

            TestSeries = new ISeries[]
            {
                new LineSeries<DateTimePoint>
                {
                    Values = points,
                    Name = "C01 Part Counter",
                    Fill=null,
                    GeometrySize=3,
                    TooltipLabelFormatter = (chartPoint) =>
                    $"{new DateTime((long) chartPoint.SecondaryValue):dd/MM HH}: {chartPoint.PrimaryValue:#,##0}",
                }
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

        public void GetKpis()
        {
            int dayOfWeekToday = (int)DateTime.Now.DayOfWeek; // Sunday = 0
            //DateTime mondayThisWeek = DateTime.Today.AddDays(1 - dayOfWeekToday -7); // TODO Change back
            DateTime reportStartDate = new(2023, 1, 4);
            reportStartDate = reportStartDate.Date.AddHours(6);

            int totalReportDaysSpan = 7;

            OperatingData baseData = new(reportStartDate, totalReportDaysSpan);
            baseData.GetData();


            Dictionary<DateTime, OperatingEfficiencyKpi> weeklyKpis = new();

            for (int i = 0; i < totalReportDaysSpan; i++)
            {
                DateTime startDate = reportStartDate.AddDays(i);

                OperatingEfficiencyKpi weeksKpi = baseData.GetKpi(startDate, span: 1);
                weeksKpi.Normalise(24 * 4);
                weeklyKpis.Add(startDate, weeksKpi);
            }

            DateTime targetKey = weeklyKpis.Keys.ElementAt(0);
            OperatingEfficiencyKpi kpi = weeklyKpis[targetKey];

            //OperatingEfficiencyKpi kpi = baseData.GetKpi(reportStartDate, totalReportDaysSpan);

            Tuple<double, double>[] seriesPoints = new Tuple<double, double>[8];

            double baseline = kpi.AvailableTime.TotalHours;
            double delta = 0;
            seriesPoints[0] = new(baseline, delta);

            baseline += delta;
            delta = -kpi.DevelopmentTime.TotalHours;
            seriesPoints[1] = new(baseline, delta);

            baseline += delta;
            delta = -kpi.PlannedMaintenanceTime.TotalHours;
            seriesPoints[2] = new(baseline, delta);

            baseline += delta;
            delta = -kpi.UnscheduledTime.TotalHours;
            seriesPoints[3] = new(baseline, delta);

            baseline += delta;
            delta = -kpi.BudgetedSettingTime.TotalHours;
            seriesPoints[4] = new(baseline, delta);

            baseline += delta;
            delta = -kpi.NonRunningTime.TotalHours;
            seriesPoints[5] = new(baseline, delta);

            baseline += delta;
            delta = -kpi.PerformanceDeltaTime.TotalHours;
            seriesPoints[6] = new(baseline, delta);

            baseline += delta;
            delta = -kpi.QualityLossTime.TotalHours;
            seriesPoints[7] = new(baseline, delta);

            KpiSeries = new ISeries[]
            {
                new RowSeries<double> // LOSS
                {
                    IsHoverable = false, // disables the series from the tooltips 
                    Values = new double[]
                    {
                        seriesPoints[7].Item1,
                        seriesPoints[6].Item1,
                        seriesPoints[5].Item1,
                        seriesPoints[4].Item1,
                        seriesPoints[3].Item1,
                        seriesPoints[2].Item1,
                        seriesPoints[1].Item1,
                        seriesPoints[0].Item1,
                    },
                    Stroke = null,
                    Fill = new SolidColorPaint(SKColors.OrangeRed),
                    IgnoresBarPosition = true,

                },
                new RowSeries<double> // GAIN
                {
                    IsHoverable = false, // disables the series from the tooltips 
                    Values = new double[]
                    {
                        seriesPoints[7].Item1 + seriesPoints[7].Item2,
                        seriesPoints[6].Item1 + seriesPoints[6].Item2,
                        seriesPoints[5].Item1 + seriesPoints[5].Item2,
                        seriesPoints[4].Item1 + seriesPoints[4].Item2,
                        seriesPoints[3].Item1 + seriesPoints[3].Item2,
                        seriesPoints[2].Item1 + seriesPoints[2].Item2,
                        seriesPoints[1].Item1 + seriesPoints[1].Item2,
                        seriesPoints[0].Item1 + seriesPoints[0].Item2,

                    },
                    Stroke = null,
                    Fill = new SolidColorPaint(SKColors.SpringGreen),
                    IgnoresBarPosition = true,
                    TooltipLabelFormatter = (chartPoint) =>
                    $"{chartPoint.PrimaryValue:0.0} h",
                },
                new RowSeries<double> // SMALLEST
                {
                    IsHoverable = true, // disables the series from the tooltips 
                    Values = new double[]
                    {
                        Math.Min(seriesPoints[7].Item1, seriesPoints[7].Item1 + seriesPoints[7].Item2),
                        Math.Min(seriesPoints[6].Item1, seriesPoints[6].Item1 + seriesPoints[6].Item2),
                        Math.Min(seriesPoints[5].Item1, seriesPoints[5].Item1 + seriesPoints[5].Item2),
                        Math.Min(seriesPoints[4].Item1, seriesPoints[4].Item1 + seriesPoints[4].Item2),
                        Math.Min(seriesPoints[3].Item1, seriesPoints[3].Item1 + seriesPoints[3].Item2),
                        Math.Min(seriesPoints[2].Item1, seriesPoints[2].Item1 + seriesPoints[2].Item2),
                        Math.Min(seriesPoints[1].Item1, seriesPoints[1].Item1 + seriesPoints[1].Item2),
                        Math.Min(seriesPoints[0].Item1, seriesPoints[0].Item1 + seriesPoints[0].Item2),
                    },
                    Stroke = null,
                    Fill = new SolidColorPaint(SKColors.DodgerBlue),
                    IgnoresBarPosition = true,
                    TooltipLabelFormatter = (chartPoint) =>
                    $"{chartPoint.PrimaryValue:0.0}",
                }
            };

            OnPropertyChanged(nameof(KpiSeries));

            KpiXAxes = new Axis[]
            {
                new Axis { MinLimit = 0, MaxLimit = seriesPoints.Max(x => x.Item1) }
            };

            OnPropertyChanged(nameof(KpiXAxes));



            TitleText = $"Efficiency for date range {targetKey:ddd dd/MM/yy HHmm} to {targetKey.AddDays(1):ddd dd/MM/yy HHmm}";

            OnPropertyChanged(nameof(TitleText));


            double[] oees = new double[totalReportDaysSpan];
            double[] ooes = new double[totalReportDaysSpan];
            string[] xAxisLabels = new string[totalReportDaysSpan];

            for (int x = 0; x < totalReportDaysSpan; x++)
            {
                OperatingEfficiencyKpi kvp = weeklyKpis.ElementAt(x).Value;
                oees[x] = kvp.GetEquipmentEfficiency() * 100;
                ooes[x] = kvp.GetOperationsEfficiency() * 100;
                xAxisLabels[x] = kvp.StartDate.ToString("dd/MM");
            }



            WeeklyKpis = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = oees,
                    Fill = null,
                    Name = "OEE"
                },
                new LineSeries<double>
                {
                    Values = ooes,
                    Fill = null,
                    Name = "OOE"
                }
            };

            KpiOverTimeXAxes = new Axis[]
            {
                new Axis { MinLimit = 0, Labels =  xAxisLabels, MinStep=1, ForceStepToMin=true}
            };

            OnPropertyChanged(nameof(KpiOverTimeXAxes));

            OnPropertyChanged(nameof(WeeklyKpis));




            GaugeSeries = new GaugeBuilder()
            .WithLabelsSize(20)
            .WithLabelsPosition(PolarLabelsPosition.Start)
            .WithLabelFormatter(point => $"{point.Context.Series.Name} {point.PrimaryValue:0.0}")
            .WithInnerRadius(20)
            .WithOffsetRadius(8)
            .WithBackgroundInnerRadius(20)
            .AddValue(kpi.GetOperationsEfficiency() * 100, "OOE %")
            .AddValue(kpi.GetSchedulingEfficiency() * 100, "OSE %")
            .AddValue(kpi.GetEquipmentEfficiency() * 100, "OEE %")
            .BuildSeries();

            OnPropertyChanged(nameof(GaugeSeries));
        }
    }
}
