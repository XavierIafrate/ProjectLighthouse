using System;

namespace ProjectLighthouse.Model.Scheduling
{
    public class TimeModel : BaseObject
    {
        private double gradient;
        public double Gradient
        {
            get { return gradient; }
            set { if (gradient == value) return; gradient = value; OnPropertyChanged(); }
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
                RecordCount= int.Parse(targetText);

                targetText = code[(code.IndexOf("R") + 1)..code.IndexOf("X")];
                CoefficientOfDetermination = double.Parse(targetText);
            }
            catch
            {
                throw new Exception("Failed to parse code");
            }
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
    }
}
