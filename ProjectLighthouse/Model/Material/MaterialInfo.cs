using ProjectLighthouse.Model.Core;
using SQLite;

namespace ProjectLighthouse.Model.Material
{
    public class MaterialInfo : IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, Unique]
        public string MaterialCode { get; set; }

        [NotNull]
        public string MaterialText { get; set; }

        [NotNull]
        public string GradeText { get; set; }

        public double Density { get; set; }
        public int? Cost { get; set; }

        public override string ToString()
        {
            return $"{MaterialText}, Grade {GradeText}";
        }
    }
}
