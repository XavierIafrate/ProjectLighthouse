using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using MigraDoc.DocumentObjectModel;
using ProjectLighthouse.Model.Scheduling;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProjectLighthouse.Model.Orders;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class TimeModelEditor : UserControl, INotifyPropertyChanged
    {
        public LatheManufactureOrder Order
        {
            get { return (LatheManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(LatheManufactureOrder), typeof(TimeModelEditor), new PropertyMetadata(null, SetTimeModel));

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void SetTimeModel(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimeModelEditor control) return;
            control.SetChartData();
        }

        public ISeries[] CycleTimeSeries { get; set; }
        public RectangularSection[] Sections { get; set; }
        public Axis[] CycleTimeYAxes { get; set; } =
    {
            new Axis
            {
                Labeler = value => $"{Math.Floor(value/60):0}m {value%60}s",
                MinLimit = 0,
                MinStep= 30,
            }
        };

        public Axis[] CycleTimeXAxes { get; set; } =
        {
            new Axis
            {
                LabelsRotation = 0,
            }
        };


        void SetChartData()
        {
            if (Order is null) return;
            if (Order.TimeModelPlanned is null) return;

            TimeModel model = Order.TimeModelPlanned;

            List<RectangularSection> sections = new();
            ISeries[] series = Array.Empty<ISeries>();

            ObservableCollection<ObservablePoint> modelledPoints = new();
            ObservableCollection<ObservablePoint> newReadings = new();
            ObservableCollection<ObservablePoint> historicalPoints = new();

            if (Order.OrderItems.Count == 0) return;
            double maxX = Order.OrderItems.Max(x => x.MajorLength) + 10;
            LineSeries<ObservablePoint> newSeries = TimeModel.GetSeries(model, maxX, "Time model");
            series = series.Append(newSeries).ToArray();

            Order.OrderItems.ForEach(x =>
            {
                if (x.CycleTime > 0)
                {
                    newReadings.Add(new(x.MajorLength, x.CycleTime));
                }

                if (x.PreviousCycleTime is not null)
                {
                    historicalPoints.Add(new(x.MajorLength, x.PreviousCycleTime));

                    if (x.CycleTime > 0 && x.CycleTime != x.PreviousCycleTime)
                    {
                        sections.Add(new RectangularSection
                        {
                            Yi = x.PreviousCycleTime,
                            Yj = x.CycleTime,
                            Xi = x.MajorLength,
                            Xj = x.MajorLength,
                            Stroke = new SolidColorPaint
                            {
                                Color = x.CycleTime > x.PreviousCycleTime ? SKColors.DarkRed.WithAlpha(50) : SKColors.DarkGreen.WithAlpha(50),
                                StrokeThickness = 8
                            }
                        });
                    }
                }
                else if (x.ModelledCycleTime is not null)
                {
                    modelledPoints.Add(new(x.MajorLength, x.ModelledCycleTime));
                }
            });


            if (newReadings.Count > 0)
            {
                series = series.Append(new ScatterSeries<ObservablePoint>
                {
                    Values = newReadings,
                    Name = "New Readings",
                    GeometrySize = 8,
                    Fill = new SolidColorPaint(SKColors.DarkRed)
                }).ToArray();
            }

            if (modelledPoints.Count > 0)
            {
                series = series.Append(new ScatterSeries<ObservablePoint>
                {
                    Values = modelledPoints,
                    Name = "Modelled",
                    GeometrySize = 8,
                    Fill = new SolidColorPaint(SKColors.MediumPurple)
                }).ToArray();
            }

            if (historicalPoints.Count > 0)
            {
                series = series.Append(new ScatterSeries<ObservablePoint>
                {
                    Values = historicalPoints,
                    GeometrySize = 8,
                    Name = "Historical",
                    Fill = new SolidColorPaint(SKColors.Black)
                }).ToArray();
            }

            Sections = sections.ToArray();
            OnPropertyChanged(nameof(Sections));

            CycleTimeSeries = series;
            OnPropertyChanged(nameof(CycleTimeSeries));
        }

        public TimeModelEditor()
        {
            InitializeComponent();
        }
    }
}
