﻿using DocumentFormat.OpenXml.Drawing.Charts;
using SQLite;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Quality
{
    public class ToleranceDefinition
    {
        [PrimaryKey]
        public string Id { get; set; }
        [NotNull]
        public int StandardId { get; set; }
        public Standard standard;
        public string Name { get; set; }
        public bool IsNumeric { get; set; }
        public int DecimalPlaces { get; set; }
        public string StringValue { get; set; }
        public double NumericValue { get; set; }
        public bool IsDynamic { get; set; }
        public ToleranceType ToleranceType { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string? FitId { get; set; }
        public string StringFormatter => DecimalPlacesToStringFormatter(DecimalPlaces);

        public string? DerivesFrom { get; set; }

        public string Nominal
        {
            get
            {
                if (!IsNumeric)
                {
                    return StringValue;
                }

                return ToleranceType switch
                {
                    ToleranceType.Min => "-",
                    ToleranceType.Max => "-",
                    _ => NumericValue.ToString(StringFormatter),
                };
            }
        }

        public string LowerLimit
        {
            get
            {
                if (IsDynamic)
                {
                    return ToleranceType switch
                    {
                        ToleranceType.None => "-",
                        ToleranceType.Min => Min.ToString(StringFormatter),
                        ToleranceType.Max => "-",
                        ToleranceType.Symmetric => $"-{(Max).ToString(StringFormatter)}",
                        ToleranceType.Bilateral => $"-{(Min).ToString(StringFormatter)}",
                        _ => "-",
                    };
                }

                if (!IsNumeric)
                {
                    return "-";
                }

                return ToleranceType switch
                {
                    ToleranceType.None => "-",
                    ToleranceType.Basic => (NumericValue - GetBasicTolerance(StringFormatter)).ToString(StringFormatter == "0" ? "0.0" : StringFormatter),
                    ToleranceType.Min => NumericValue.ToString(StringFormatter),
                    ToleranceType.Max => "-",
                    ToleranceType.Symmetric => $"{(NumericValue - Max).ToString(StringFormatter)}",
                    ToleranceType.Bilateral => $"{(NumericValue - Min).ToString(StringFormatter)}",
                    _ => "-",
                };
            }
        }

        public string UpperLimit
        {
            get
            {
                if (IsDynamic)
                {
                    return ToleranceType switch
                    {
                        ToleranceType.None => "-",
                        ToleranceType.Min => "-",
                        ToleranceType.Max => $"+{(Max).ToString(StringFormatter)}",
                        ToleranceType.Symmetric => $"+{(Max).ToString(StringFormatter)}",
                        ToleranceType.Bilateral => $"+{(Max).ToString(StringFormatter)}",
                        _ => "-",
                    };
                }

                if (!IsNumeric)
                {
                    return "-";
                }

                return ToleranceType switch
                {
                    ToleranceType.None => "-",
                    ToleranceType.Basic => (NumericValue + GetBasicTolerance(StringFormatter)).ToString(StringFormatter == "0" ? "0.0" : StringFormatter),
                    ToleranceType.Min => "-",
                    ToleranceType.Max => NumericValue.ToString(StringFormatter),
                    ToleranceType.Symmetric => $"{(NumericValue + Max).ToString(StringFormatter)}",
                    ToleranceType.Bilateral => $"{(NumericValue + Max).ToString(StringFormatter)}",
                    _ => "-",
                };
            }
        }

        private double GetBasicTolerance(string f)
        {
            // TODO error handling
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
        public override string ToString()
        {
            return ToleranceType switch
            {
                ToleranceType.None => $"{Nominal} (No Tolerance)",
                ToleranceType.Min => $"{NumericValue.ToString(StringFormatter)} MIN",
                ToleranceType.Max => $"{NumericValue.ToString(StringFormatter)} MAX",
                ToleranceType.Symmetric => $"{NumericValue.ToString(StringFormatter)} ± {Max.ToString(StringFormatter)}",
                ToleranceType.Bilateral => $"{NumericValue.ToString(StringFormatter)} {Max.ToString($" +{StringFormatter}; -{StringFormatter}; +{StringFormatter}")} / {(Min * -1).ToString($" +{StringFormatter}; -{StringFormatter}; -{StringFormatter}")}",
                ToleranceType.Fit => $"{Nominal} {FitId ?? "(FIT NOT SET)"}",
                _ => "ERROR Unknown Tolerance Type",
            };
        }
    }

    public enum ToleranceType { None = 0, Basic = 1, Min = 2, Max = 3, Symmetric = 4, Bilateral = 5, Fit = 6 }
}