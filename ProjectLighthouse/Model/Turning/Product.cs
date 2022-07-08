using SQLite;
using System.Collections.Generic;

namespace ProjectLighthouse.Model
{
    public class Product
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique, NotNull]
        public string Name { get; set; }
        [NotNull]
        public string Description { get; set; }
        [NotNull]
        public string WebUrl { get; set; }
        [NotNull]
        public string ImageUrl { get; set; }
        [NotNull]
        public string WebImageUrl { get; set; }

        [Ignore]
        public string LocalRenderPath
        {
            get { return $@"{App.AppDataDirectory}lib\renders\{ImageUrl}"; }
        }


        [Ignore]
        public List<ToolingGroup> ToolingGroups { get; set; }
        public class ToolingGroup
        {
            [PrimaryKey]
            public string Name { get; set; }
            public string MemberOf { get; set; }
            public string CheckSheetUrl { get; set; }
            [Ignore]
            public List<TurnedProduct> Products { get; set; }

            [Ignore]
            public List<CheckSheetField> CheckSheetFields { get; set; }

            public override string ToString()
            {
                return $"{MemberOf}>{Name}";
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
