using SQLite;

namespace ProjectLighthouse.Model
{
    public class ProductGroup
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string GroupID { get; set; }
        public string MaterialCode { get; set; }

        // User facing
        public string ProductTitle { get; set; }
        public string ProductSubTitle { get; set; }
        public string Breadcrumb { get; set; }
        public string Material { get; set; }
        public string LineDrawingURL { get; set; }
        public string RenderURL { get; set; }
        public string DetailedMaterial { get; set; }
        public string TechnicalNotes { get; set; }
    }
}
