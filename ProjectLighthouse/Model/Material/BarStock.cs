using SQLite;
using System;

namespace ProjectLighthouse.Model.Material
{
    public class BarStock
    {
        [PrimaryKey]
        public string Id { get; set; }
        public int Length { get; set; }
        public double Size { get; set; }
        public double InStock { get; set; }
        public double OnOrder { get; set; }
        public int Cost { get; set; }
        public int SuggestedStock { get; set; }

        [Ignore]
        public MaterialInfo MaterialData { get; set; }
        public int MaterialId { get; set; }

        public double GetUnitMassOfBar()
        {
            double mass = 3.14159 * Math.Pow(Size / 2000, 2);
            mass *= (double)Length / 1000;
            mass *= MaterialData.Density;
            return mass;
        }

        public override string ToString()
        {
            return Id;
        }
    }

}
