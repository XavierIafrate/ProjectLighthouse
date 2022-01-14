using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class Request : ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Product { get; set; }
        public string POReference { get; set; }
        public int QuantityRequired { get; set; }
        public DateTime DateRequired { get; set; }

        public string RaisedBy { get; set; }
        public DateTime DateRaised { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsDeclined { get; set; } = false;
        public bool IsAccepted { get; set; } = false;
        public string AcceptedBy { get; set; }

        public string DeclinedReason { get; set; } = "";
        public string ResultingLMO { get; set; }
        public string Status { get; set; } = "Pending approval";
        public string Likeliness { get; set; }
        public string Notes { get; set; }

        public bool CleanCustomerRequirement { get; set; }

        public object Clone()
        {
            return new Request
            {
                Id = Id,
                Product = Product,
                POReference = POReference,
                QuantityRequired = QuantityRequired,   
                DateRequired = DateRequired,
                RaisedBy = RaisedBy,
                DateRaised = DateRaised,
                ModifiedBy = ModifiedBy,
                LastModified = LastModified,
                IsDeclined = IsDeclined,
                IsAccepted = IsAccepted,
                AcceptedBy = AcceptedBy,
                DeclinedReason = DeclinedReason,
                ResultingLMO = ResultingLMO,
                Status = Status,
                Likeliness = Likeliness,
                Notes = Notes,
                CleanCustomerRequirement = CleanCustomerRequirement,
            };
        }

        public void MarkAsAccepted()
        {
            IsAccepted = true;
            IsDeclined = false;
            LastModified = DateTime.Now;
            ModifiedBy = App.CurrentUser.GetFullName();
            AcceptedBy = App.CurrentUser.FirstName;
            Status = $"Accepted by {AcceptedBy} - {ResultingLMO}";
        }

        public void MarkAsDeclined()
        {
            IsAccepted = false;
            IsDeclined = true;
            LastModified = DateTime.Now;
            ModifiedBy = App.CurrentUser.GetFullName();
            Status = $"Declined - {DeclinedReason}";
        }

    }
}
