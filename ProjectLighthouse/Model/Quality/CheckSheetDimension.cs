using SQLite;

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
    }
    public enum ToleranceType { None, Basic, Min, Max, Symmetric, Bilateral }
}
