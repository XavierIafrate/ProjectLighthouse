using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class AssemblyManufactureOrder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Name { get; set; }
        public string POReference { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }

        public string Status { get; set; }
        public string Notes { get; set; }

        public AssemblyManufactureOrder Clone()
        {
            return new AssemblyManufactureOrder()
            {
                Id = Id,
                Name = Name,
                POReference = POReference,
                CreatedAt = CreatedAt,
                CreatedBy = CreatedBy,
                ModifiedAt = ModifiedAt,
                ModifiedBy = ModifiedBy,
                Status = Status,
                Notes = Notes
            };
        }
    }
}
