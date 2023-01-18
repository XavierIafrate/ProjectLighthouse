using SQLite;
using System;

namespace ProjectLighthouse.Model.Administration
{
    public class Machine
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string FullName { get; set; }
        public string SerialNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public bool OutOfService { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}
