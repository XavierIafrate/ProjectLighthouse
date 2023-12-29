using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Orders;
using SQLite;
using System;

namespace ProjectLighthouse.Model.Requests
{
    public class Request : BaseObject, ICloneable, IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string description = "";
        public string Description
        {
            get { return description; }
            set { description = value; OnPropertyChanged(); }
        }

        public int TotalQuantity { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public int? ArchetypeId { get; set; }

        public string RaisedBy { get; set; }
        public DateTime RaisedAt { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public DateTime? DecisionAt { get; set; }
        public string? DecisionBy { get; set; }

        public int? OrderId { get; set; }
        public LatheManufactureOrder? order;


        public object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Request>(serialised);
        }

        public void Mark(bool accepted)
        {
            Status = accepted ? RequestStatus.Accepted : RequestStatus.Declined;

            ModifiedAt = DateTime.Now;
            ModifiedBy = App.CurrentUser.GetFullName();

            DecisionAt = DateTime.Now;
            DecisionBy = App.CurrentUser.GetFullName();
        }

        public enum RequestStatus
        {
            Draft = -2,
            Declined = -1,
            Pending = 0,
            Accepted = 1,
        }
    }
}
