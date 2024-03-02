using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                Labeler = value => new DateTime((long)value).ToString("MM/yy"),
                CrosshairLabelsBackground = SKColors.DarkOrange.AsLvcColor(),
                CrosshairLabelsPaint = new SolidColorPaint(SKColors.DarkRed, 1),
                CrosshairPaint = new SolidColorPaint(SKColors.DarkOrange, 1),
            }
        };

        public Axis[] YAxes { get; set; } =
    {
        new Axis
        {
                Labeler = value => $"{(int)value:0}",

            CrosshairLabelsBackground = SKColors.DarkOrange.AsLvcColor(),
            CrosshairLabelsPaint = new SolidColorPaint(SKColors.DarkRed, 1),
            CrosshairPaint = new SolidColorPaint(SKColors.DarkOrange, 1),
            CrosshairSnapEnabled = false,
            MinLimit=0
        }
    };

        public GeneralManufactureOrder Order
        {
            get { return (GeneralManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(GeneralManufactureOrder), typeof(GeneralOrderInspector), new PropertyMetadata(null, SetValues));


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

            List<ISeries> series = new()
            {
                theoreticalProduction,
            };

            ProductionData = series.ToArray();

            Order.StartDate = DateTime.Today.AddDays(3);
            Order.TimeToComplete = daysElapsed * 86400;

            DatabaseHelper.Update(Order);
        }

        public GeneralOrderInspector()
        {
            InitializeComponent();
        }
    }
}
