﻿using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class TechnicalDrawing
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Product { get; set; }
        public int Revision { get; set; }
        public string URL { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public bool IsArchetype { get; set; }
        public string Customer { get; set; }
        
        public bool IsCurrent;
    }
}