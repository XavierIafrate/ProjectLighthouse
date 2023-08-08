using System;

namespace ProjectLighthouse.Model.Scheduling
{
    public class TimeModel : BaseObject
    {
        private double gradient;
        public double Gradient
        {
            get { return gradient; }
            set { gradient = value; OnPropertyChanged(); }
        }

        private double intercept;
        public double Intercept
        {
            get { return intercept; }
            set { intercept = value; OnPropertyChanged(); }
        }

        private int floor;
        public int Floor
        {
            get { return floor; }
            set { floor = value; OnPropertyChanged(); }
        }

        public double cDetermination;
        public int recordCount;

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

                targetText = code[(code.IndexOf("F") + 1)..];
                Floor = int.Parse(targetText);
            }
            catch
            {
                throw new Exception("Failed to parse code");
            }
        }

        public override string ToString()
        {
            return $"M{Gradient:0.000}C{Intercept:0.000}F{Floor:0}";
        }

        public int At(double length)
        {
            return Math.Max(Floor, Convert.ToInt32(Gradient * length + Intercept));
        }
    }
}
