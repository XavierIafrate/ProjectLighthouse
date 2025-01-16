using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Orders;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class GeneralOrderInspector : UserControl, INotifyPropertyChanged
    {
        public Axis[] XAxes { get; set; } = new[]
            {
                new Axis()
                {
                    Labeler = value => value == -1 ? "" : new DateTime((long)value).ToString("dd/MM"),
                            CrosshairLabelsBackground = SKColors.Black.AsLvcColor(),
                            CrosshairLabelsPaint = new SolidColorPaint(SKColors.White, 1),
                            CrosshairPaint = new SolidColorPaint(SKColors.Black, 1),
                        }
                };

        public Axis[] YAxes { get; set; } =
            {
            new Axis
            {
                    Labeler = value => $"{(int)value:0}",

                CrosshairLabelsBackground = SKColors.Black.AsLvcColor(),
                CrosshairLabelsPaint = new SolidColorPaint(SKColors.White, 1),
                CrosshairPaint = new SolidColorPaint(SKColors.Black, 1),
                CrosshairSnapEnabled = false,
                MinLimit=0
            }
        };

        public bool EditMode
        {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof(bool), typeof(GeneralOrderInspector), new PropertyMetadata(false));

        public GeneralManufactureOrder Order
        {
            get { return (GeneralManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(GeneralManufactureOrder), typeof(GeneralOrderInspector), new PropertyMetadata(null, SetValues));


        public List<User> ProductionStaff
        {
            get { return (List<User>)GetValue(ProductionStaffProperty); }
            set { SetValue(ProductionStaffProperty, value); }
        }

        public static readonly DependencyProperty ProductionStaffProperty =
            DependencyProperty.Register("ProductionStaff", typeof(List<User>), typeof(GeneralOrderInspector), new PropertyMetadata(null));


        private ISeries[] productionData;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ISeries[] ProductionData
        {
            get { return productionData; }
            set { productionData = value; OnPropertyChanged(); }
        }
        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not GeneralOrderInspector control) return;

            control.CalculateProductionData();
            if (control.Order is null) return;

            control.OrderTabControl.SelectedIndex = control.Order.State < OrderState.Running ? 0 : 2;

            control.Order.PropertyChanged += control.Order_PropertyChanged;
            if (e.OldValue is GeneralManufactureOrder prevOrder)
            {
                prevOrder.PropertyChanged -= control.Order_PropertyChanged;
            }
        }

        private void Order_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GeneralManufactureOrder.RequiredQuantity))
            {
                CalculateProductionData();
            }
        }

        private void CalculateProductionData()
        {
            if (Order is null)
            {
                ProductionData = null;
                return;
            }


            DateTime start = Order.StartDate == DateTime.MinValue ? DateTime.Today.AddDays(1) : Order.StartDate.Date;


            List<DateTimePoint> theoreticalDataPoints = new();
            List<DateTimePoint> actualProductionDataPoints = new();
            int totalDone = 0;
            int daysElapsed = 0;

            theoreticalDataPoints.Add(new(start, 0));


            while (totalDone < Order.RequiredQuantity)
            {
                DateTime cursor = start.AddDays(daysElapsed);
                daysElapsed++;
                OpeningHours.Day cursorOpeningHours = App.Constants.OpeningHours.Data[cursor.DayOfWeek];

                if (!cursorOpeningHours.OpensOnDay)
                {
                    continue;
                }

                TimeSpan availableTime = cursorOpeningHours.GetOpeningHoursTimeSpan();

                int totalOnDay = (int)Math.Floor(availableTime.TotalSeconds / Order.Item.CycleTime);
                totalDone += totalOnDay;

                totalDone = Math.Min(totalDone, Order.RequiredQuantity);

                theoreticalDataPoints.Add(new(cursor, totalDone));
            }

            Order.TimeToComplete = daysElapsed * 86400;

            StepLineSeries<DateTimePoint> theoreticalProduction = new()
            {
                Values = theoreticalDataPoints,
                Name = "Perfect Production Run",
                Fill = null,
                GeometrySize = 0,
                GeometryStroke = new SolidColorPaint(SKColors.DarkGray) { StrokeThickness = 3 },
                Stroke = new SolidColorPaint(SKColors.DarkGray) { StrokeThickness = 3 },
                TooltipLabelFormatter = (chartPoint) =>
                $"Theoretical: {new DateTime((long)chartPoint.SecondaryValue):dd/MM/yy}: {chartPoint.PrimaryValue:0}",
            };

            List<ISeries> series = new()
            {
                theoreticalProduction,
            };

            if (Order.Lots.Where(x => x.IsAccepted).Any())
            {
                List<DateTime> datesWithDeliveries = Order.Lots.Where(x => x.IsAccepted)
                    .Select(x => x.DateProduced.Date)
                    .Distinct()
                    .ToList();

                actualProductionDataPoints.Add(new(datesWithDeliveries.Min(), 0));
                int cumulative = 0;
                foreach (DateTime date in datesWithDeliveries)
                {
                    int totalOnDay = Order.Lots.Where(x => x.DateProduced.Date == date && x.IsAccepted).Sum(x => x.Quantity);
                    cumulative += totalOnDay;
                    actualProductionDataPoints.Add(new(date, cumulative));
                }

                StepLineSeries<DateTimePoint> actualProduction = new()
                {
                    Values = actualProductionDataPoints,
                    Name = "Actual Production",
                    Fill = null,
                    GeometrySize = 0,
                    GeometryStroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 3 },
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 3 },
                    TooltipLabelFormatter = (chartPoint) =>
                    $"Actual" +
                    $": {new DateTime((long)chartPoint.SecondaryValue):dd/MM/yy}: {chartPoint.PrimaryValue:0}",
                };

                series.Add(actualProduction);
            }

            if (Order.Lots.Where(x => x.IsAccepted).Any() && Order.FinishedQuantity < Order.RequiredQuantity)
            {
                List<DateTimePoint> optimisticSeries = new();

                int startQty = Order.Lots.Where(x => x.IsAccepted).Sum(x => x.Quantity);
                DateTime startDate = Order.Lots.Where(x => x.IsAccepted).Max(x => x.DateProduced);

                optimisticSeries.Add(new(startDate, startQty));

                while (startQty < Order.RequiredQuantity)
                {
                    startDate = startDate.AddDays(1);
                    OpeningHours.Day cursorOpeningHours = App.Constants.OpeningHours.Data[startDate.DayOfWeek];

                    if (!cursorOpeningHours.OpensOnDay)
                    {
                        continue;
                    }

                    TimeSpan availableTime = cursorOpeningHours.GetOpeningHoursTimeSpan();

                    int totalOnDay = (int)Math.Floor(availableTime.TotalSeconds / Order.Item.CycleTime);
                    startQty += totalOnDay;

                    startQty = Math.Min(startQty, Order.RequiredQuantity);

                    optimisticSeries.Add(new(startDate, startQty));

                    StepLineSeries<DateTimePoint> optimisticFuture = new()
                    {
                        Values = optimisticSeries,
                        Name = "Optimistic Future",
                        Fill = null,
                        GeometrySize = 0,
                        GeometryStroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 2 },
                        Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 2 },
                        TooltipLabelFormatter = (chartPoint) =>
                        $"Optimistic" +
                        $": {new DateTime((long)chartPoint.SecondaryValue):dd/MM/yy}: {chartPoint.PrimaryValue:0}",
                    };

                    series.Add(optimisticFuture);
                }
            }


            ProductionData = series.ToArray();


        }

        public GeneralOrderInspector()
        {
            InitializeComponent();
        }
    }
}
