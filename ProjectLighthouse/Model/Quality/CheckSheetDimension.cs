using SQLite;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Quality
{
    public class CheckSheetDimension : IAutoIncrementPrimaryKey
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        [NotNull]
        public int DrawingId { get; set; }
        public string Name { get; set; }
        public bool IsNumeric { get; set; }
        public string StringValue { get; set; }
        public double NumericValue { get; set; }
        public ToleranceType ToleranceType { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string StringFormatter { get; set; } = "0";
        public string UpperLimit => IsNumeric
            ? $"{(NumericValue + Max).ToString(StringFormatter)}"
            : "-";

        public string LowerLimit
        {
            get
            {
                if (!IsNumeric)
                {
                    return "-";
                }

                return ToleranceType switch
                {
                    ToleranceType.None => "-",
                    ToleranceType.Basic => (NumericValue - GetBasicTolerance(StringFormatter)).ToString(StringFormatter),
                    ToleranceType.Min => NumericValue.ToString(StringFormatter),
                    ToleranceType.Max => "-",
                    ToleranceType.Symmetric => $"{(NumericValue - Max).ToString(StringFormatter)}",
                    ToleranceType.Bilateral => $"{(NumericValue - Min).ToString(StringFormatter)}",
                    _ => "-",
                };
            }
        }

        private double GetBasicTolerance(string f)
        {
            return BasicTolerances[f];
        }

        public string DecimalPlacesToStringFormatter(int numPlaces)
        {
            if (numPlaces < 0 || numPlaces > 8)
            {
                throw new ArgumentOutOfRangeException("numPlaces is weird, plz fix");
            }

            if (numPlaces == 0)
            {
                return "0";
            }

            string x = "0.";

            for (int i = 0; i < numPlaces; i++)
            {
                x += "0";
            }

            return x;
        }

        public static Dictionary<string, double> BasicTolerances = new()
        {
            {"0", 0.5 },
            {"0.0", 0.1 },
            {"0.00", 0.02 },
            {"0.000", 0.004 },
            {"0.0000", 0.0008 },
            {"0.00000", 0.00016 },
            {"0.000000", 0.000032 },
            {"0.0000000", 0.0000064 }, // Just in case lol 
        };
    }
    public enum ToleranceType { None, Basic, Min, Max, Symmetric, Bilateral }

}
