using SQLite;
using System;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Deliveries
{
    public class DeliveryNote : BaseObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique]
        public string Name { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveredBy { get; set; }

        private bool verified;
        public bool Verified
        {
            get { return verified; }
            set { verified = value; OnPropertyChanged(); }
        }


        private List<string> errors = new();

        [Ignore]
        public new List<string> Errors
        {
            get { return errors; }
            set { errors = value; OnPropertyChanged(); }
        }
    }
}
