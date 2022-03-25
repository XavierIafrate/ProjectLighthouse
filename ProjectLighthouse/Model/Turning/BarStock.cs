using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class BarStock
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Material { get; set; }
        public string Form { get; set; }
        public int Length { get; set; }
        public int Size { get; set; }
        public double InStock { get; set; }
        public double OnOrder { get; set; }
        public int Cost { get; set; }
        public int SuggestedStock { get; set; }

        public double GetUnitMassOfBar()
        {
            double mass = 3.14159 * Math.Pow((double)Size / 2000, 2);
            mass *= (double)Length / 1000;
            mass *= 8050;
            return mass;
        }

        public override string ToString()
        {
            return Id;
        }
    }

}
