﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model.Assembly
{
    public class BillOfMaterials
    {
        public string ID { get; set; }
        public string ToMake { get; set; }
        public List<BillOfMaterialsItem> Items { get; set; }
    }
}