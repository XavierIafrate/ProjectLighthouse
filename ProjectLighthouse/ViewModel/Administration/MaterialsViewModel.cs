using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class MaterialsViewModel : BaseViewModel
    {
        private List<MaterialInfo> materials;

        public List<MaterialInfo> Materials
        {
            get { return materials; }
            set { materials = value; OnPropertyChanged(); }
        }

        private MaterialInfo? selectedMaterial;

        public MaterialInfo? SelectedMaterial
        {
            get { return selectedMaterial; }
            set { selectedMaterial = value; LoadMaterial(); OnPropertyChanged(); }
        }

        private List<BarStock> barStock = new();

        private List<BarStock> filteredBarStock;

        public List<BarStock> FilteredBarStock
        {
            get { return filteredBarStock; }
            set { filteredBarStock = value; OnPropertyChanged(); }
        }

        public RectangularSection[] Rates { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxisStartAtZero { get; set; } =
        {
            new Axis
            {
                MinLimit=0
            }
        };

        public Axis[] XAxisStartAtZero { get; set; } =
        {
            new Axis
            {
                MinLimit=0
            }
        };

        public ISeries[] Series { get; set; }
        public ISeries[] PriceToMassSeries { get; set; }

        private List<BarStockPurchase> barStockPurchases;

        public MaterialsViewModel()
        {
            XAxes = new[] { new Axis() { Labeler = value => value == -1 ? "n/a" : new DateTime((long)value).ToString("MM/yyyy") } };
            OnPropertyChanged(nameof(XAxes));
            GetData();
        }

        private void GetData()
        {
            Materials = DatabaseHelper.Read<MaterialInfo>().OrderBy(x => x.MaterialText).ThenBy(x => x.GradeText).ToList();
            barStock = DatabaseHelper.Read<BarStock>().OrderBy(x => x.Size).ToList();

            foreach (BarStock bar in barStock)
            {
                bar.MaterialData = Materials.Find(m => m.Id == bar.MaterialId);
            }

            barStockPurchases = DatabaseHelper.Read<BarStockPurchase>();

            if (Materials.Count > 0)
            {
                SelectedMaterial = Materials.First();
            }
        }

        private void LoadMaterial()
        {
            if (SelectedMaterial is null)
            {
                FilteredBarStock = new();
                return;
            }

            FilteredBarStock = barStock.Where(x => x.MaterialId == SelectedMaterial.Id).ToList();

            GetChart(FilteredBarStock, SelectedMaterial);
        }

        private void GetChart(List<BarStock> barStock, MaterialInfo material)
        {
            List<BarStockPurchase> purchasesOfMaterial = barStockPurchases.Where(x => barStock.Any(b => b.Id == x.BarId)).OrderBy(x => x.DateRequired).ToList();
            List<DateTimePoint> chronChartData = new();
            List<ObservablePoint> massChartData = new();
            //if (purchasesOfMaterial.Count == 0)
            //{
            //    Series = new();
            //}
            List<RectangularSection> rates = new()
            {
                new RectangularSection
                {
                    Yi = material.GetRate(),
                    Yj = material.GetRate(),
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.DodgerBlue,
                        StrokeThickness = 3
                    }
                }
            };

            foreach (BarStockPurchase purchase in purchasesOfMaterial)
            {
                BarStock? bar = barStock.Find(x => x.Id == purchase.BarId);
                if (bar is null) continue;

                double barMass = bar.GetUnitMassOfBar();
                double barValue = purchase.LineValue / purchase.QuantityRequired;

                double rate = barValue / barMass;

                chronChartData.Add(new(purchase.DateRequired, rate));
            }

            List<string> seenBarIds = new();
            List<ScatterSeries<ObservablePoint>> barMassVsValue = new();
            foreach (BarStockPurchase purchase in purchasesOfMaterial.OrderByDescending(x => x.DateRequired).ToList())
            {
                if (seenBarIds.Contains(purchase.BarId))
                {
                    continue;
                }

                BarStock? bar = barStock.Find(x => x.Id == purchase.BarId);
                if (bar is null) continue;

                double barMass = bar.GetUnitMassOfBar();
                double barValue = purchase.LineValue / purchase.QuantityRequired;

                byte alpha = 255;

                alpha -= (byte)Math.Min(200, (DateTime.Today - purchase.DateRequired).Days);

                barMassVsValue.Add(new ScatterSeries<ObservablePoint>
                {
                    Values = new List<ObservablePoint>() { new(barMass, barValue) },
                    Name = bar.Id,
                    GeometrySize = 8,
                    Fill = new SolidColorPaint(SKColors.MediumPurple.WithAlpha(alpha)),
                    TooltipLabelFormatter = (chartPoint) =>
                    $"{bar.Id}{Environment.NewLine}{chartPoint.SecondaryValue:0.0}kg : £{chartPoint.PrimaryValue:#,##0.00}",
                });

                seenBarIds.Add(bar.Id);
            }

            Series = new ISeries[]
            {
                new ScatterSeries<DateTimePoint>
                {
                    Values = chronChartData,
                    Name = "Purchases",
                    GeometrySize = 8,
                    Fill = new SolidColorPaint(SKColors.MediumPurple),
                    TooltipLabelFormatter = (chartPoint) =>
                    $"{new DateTime((long) chartPoint.SecondaryValue):dd/MM/yy} : £{chartPoint.PrimaryValue:0.00}/kg",
                }
            };

            PriceToMassSeries = barMassVsValue.ToArray();

            Rates = rates.ToArray();
            OnPropertyChanged(nameof(Rates));
            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(PriceToMassSeries));
        }
    }
}
