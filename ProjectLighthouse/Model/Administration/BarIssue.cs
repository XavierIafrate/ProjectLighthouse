using SQLite;
using System;

namespace ProjectLighthouse.Model.Administration
{
    public class BarIssue
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string IssuedBy { get; set; }
        public string BarId { get; set; }
        public string OrderId { get; set; }
        public string MaterialBatch { get; set; }
        public int Quantity { get; set; }
        public string MaterialInfo { get; set; }
    }
}
