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
using Windows.UI.WebUI;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class GeneralOrderInspector : UserControl, INotifyPropertyChanged
    {
        public Axis[] XAxes { get; set; } = new[]
        {
            new Axis()
            {
                Labeler = value => new DateTime((long)value).ToString("dd/MM"),
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

        private Lot newLot;

        public Lot NewLot
        {
            get { return newLot; }
            set { newLot = value; OnPropertyChanged(); }
        }



        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not GeneralOrderInspector control) return;

            control.CalculateProductionData();
            if(control.Order is null) return;

            control.NewLot.AddedBy = App.CurrentUser.UserName;
            control.NewLot.Order = control.Order.Name;
            control.NewLot.FromMachine = control.Order.AllocatedMachine;
            control.NewLot.CycleTime = control.Order.Item.CycleTime;
            control.NewLot.AllowDelivery = true;
            control.NewLot.ProductName = control.Order.Item.Name;
            if (control.Order.Lots.Count > 0)
            {
                control.NewLot.MaterialBatch = control.Order.Lots.Last().MaterialBatch;
            }
        }

        private void CalculateProductionData()
        {
            if (Order is null)
            {
                ProductionData = null;
                return;
            }

            DateTime start = DateTime.Today.AddDays(1);


            List<DateTimePoint> theoreticalDataPoints = new();
            List<DateTimePoint> actualProductionDataPoints = new();
            int totalDone = 0;
            int daysElapsed = 0;
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

            List<DateTime> datesWithDeliveries = Order.Lots.Select(x => x.DateProduced.Date).Distinct().ToList();
            actualProductionDataPoints.Add(new(datesWithDeliveries.Min(), 0));
            foreach (DateTime date in datesWithDeliveries)
            {
                int totalOnDay = Order.Lots.Where(x => x.DateProduced.Date == date && x.IsAccepted).Sum(x => x.Quantity);
                actualProductionDataPoints.Add(new(date, totalOnDay));
            }

            StepLineSeries<DateTimePoint> theoreticalProduction = new()
            {
                Values = theoreticalDataPoints,
                Name = "Theoretical Production Rate",
                Fill = null,
                GeometrySize = 0,
                GeometryStroke = new SolidColorPaint(SKColors.DarkGray) { StrokeThickness = 3 },
                Stroke = new SolidColorPaint(SKColors.DarkGray) { StrokeThickness = 3 },
                TooltipLabelFormatter = (chartPoint) =>
                $"Theoretical: {new DateTime((long)chartPoint.SecondaryValue):dd/MM/yy}: {chartPoint.PrimaryValue:0}",
            };

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

            List<ISeries> series = new()
            {
                theoreticalProduction,
                actualProduction,
            };

            ProductionData = series.ToArray();
        }

        public GeneralOrderInspector()
        {
            InitializeComponent();

            NewLot = new();
        }
    }
}
