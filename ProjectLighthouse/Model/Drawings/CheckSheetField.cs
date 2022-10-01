using SQLite;
using System;

namespace ProjectLighthouse.Model.Drawings
{
    public class CheckSheetField
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }

        public string Product { get; set; }
        public string ToolingGroup { get; set; }

        public CheckSheetField()
        {
            AddedAt = DateTime.Now;
            AddedBy = App.CurrentUser.UserName;
        }
    }
}
