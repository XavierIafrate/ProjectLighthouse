using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Material;
using System.Linq;
using SQLite;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Products
{
    public class ProductGroup : IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }


        public int? ProductId { get; set; }
        public double? MinBarSize { get; set; }
        public double MajorDiameter { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public double GetRequiredBarSize()
        {
            if (MinBarSize is null)
            {
                return MajorDiameter;
            }

            return Math.Max(MajorDiameter, (double)MinBarSize);
        }

        public BarStock? GetRequiredBarStock(List<BarStock> bars, int materialId)
        {
            bars = bars.Where(x => x.MaterialId == materialId && x.Size >= GetRequiredBarSize())
                        .OrderBy(x => x.Size)
                        .ToList();

            if (bars.Count == 0)
            {
                return null;
            }

            return bars.First();
        }
    }
}
