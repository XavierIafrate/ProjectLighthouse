using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProjectLighthouse.ViewModel.Requests;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;

namespace ProjectLighthouse.Model.Scheduling
{
    public class TimeModel : BaseObject
    {
        private double gradient;
        public double Gradient
        {
            get { return gradient; }
            set
            {
                if (gradient == value) return;
                gradient = value;
                OnPropertyChanged();
            }
        }

        private double intercept;
        public double Intercept
        {
            get { return intercept; }
            set { if (intercept == value) return; intercept = value; OnPropertyChanged(); }
        }

        private int floor;
        public int Floor
        {
            get { return floor; }
            set { floor = value; OnPropertyChanged(); }
        }

        private double coefficientOfDetermination;
        public double CoefficientOfDetermination
        {
            get { return coefficientOfDetermination; }
            set { coefficientOfDetermination = value; OnPropertyChanged(); }
        }

        private int recordCount;
        public int RecordCount
        {
            get { return recordCount; }
            set { recordCount = value; OnPropertyChanged(); }
        }

        public TimeModel()
        {

        }

        public TimeModel(string code)
        {
            try
            {
                string targetText = code[(code.IndexOf("M") + 1)..code.IndexOf("C")];
                Gradient = double.Parse(targetText);

                targetText = code[(code.IndexOf("C") + 1)..code.IndexOf("F")];
                Intercept = double.Parse(targetText);

                targetText = code[(code.IndexOf("F") + 1)..code.IndexOf("N")];
                Floor = int.Parse(targetText);

                targetText = code[(code.IndexOf("N") + 1)..code.IndexOf("R")];
                RecordCount = int.Parse(targetText);

                targetText = code[(code.IndexOf("R") + 1)..code.IndexOf("X")];
                CoefficientOfDetermination = double.Parse(targetText);
            }
            catch
            {
                throw new Exception("Failed to parse code");
            }
        }

        public static TimeModel Default(double diameter, double gradient = 0.5)
        {
            int estimatedByDiameter = RequestsEngine.EstimateCycleTime(diameter);
            return new TimeModel()
            {
                Gradient = gradient,
                Intercept = estimatedByDiameter - 20,
                Floor = estimatedByDiameter,
                CoefficientOfDetermination = 0,
                RecordCount = 0
            };
        }

        public override string ToString()
        {
            // M C F N R X
            return $"M{Gradient:0.000}C{Intercept:0.000}F{Floor:0}N{RecordCount:0}R{CoefficientOfDetermination:0.000}X";
        }

        public string Explain()
        {
            return $"The cycle time increases at a rate of {Gradient:0.00} seconds per millimeter from {Intercept:0} seconds at zero length. A minimum time of {Floor:0} is used. This evaluation is based on {RecordCount:0} records and has a confidence of {CoefficientOfDetermination:P0}.";
        }

        public int At(double length)
        {
            return Math.Max(Floor, Convert.ToInt32(Gradient * length + Intercept));
        }

        public static LineSeries<ObservablePoint> GetSeries(TimeModel model, double toLength, string seriesName)
        {
            ObservableCollection<ObservablePoint> cyclePointsBF = new();

            if (model.Floor <= model.Intercept)
            {
                cyclePointsBF.Add(new(0, model.Intercept));
            }
            else
            {
                cyclePointsBF.Add(new(0, model.Floor));
                cyclePointsBF.Add(new((model.Floor - model.Intercept) / model.Gradient, model.Floor));
            }
            double gradMaxY = model.Gradient * toLength + model.Intercept;

            if (gradMaxY >= model.Floor)
            {
                cyclePointsBF.Add(new(toLength, gradMaxY));
            }

            return new LineSeries<ObservablePoint>
            {
                Values = cyclePointsBF,
                Name = seriesName,
                Fill = null,
                LineSmoothness = 0,
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.DarkBlue),
            };
        }
    }
}
