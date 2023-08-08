using ProjectLighthouse.Model.Core;
using SQLite;

namespace ProjectLighthouse.Model.Drawings
{
    public class ToleranceDefinition : IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [NotNull, Unique]
        public string Address { get; set; }

        public string Value { get; set; }
        public ToleranceType Type { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public string Source { get; set; }


        public enum ToleranceType
        {
            None = 0,
            Bilateral = 10,
            Symmetric = 20,
            Min = 30,
            Max = 40,
        }

    }
}
