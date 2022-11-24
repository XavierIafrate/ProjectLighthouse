using ProjectLighthouse.Model.Core;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Products
{
    public class ProductGroup : IAutoIncrementPrimaryKey    
    {
        [PrimaryKey, AutoIncrement] 
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string BarId { get; set; }
    }
}
