using SQLite;
using System.Collections.Generic;

namespace ProjectLighthouse.Model
{
    public class Product
    {
        [PrimaryKey]
        public string Name { get; set; }
        public string Description { get; set; }
        public string WebUrl { get; set; }
        public string ImageUrl { get; set; }
        public string CheckSheetUrl { get; set; }
        [Ignore]
        public List<CheckSheetField> CheckSheetFields { get; set; }

        [Ignore]
        public string RenderPath
        {
            get { return App.ROOT_PATH + ImageUrl; }
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
