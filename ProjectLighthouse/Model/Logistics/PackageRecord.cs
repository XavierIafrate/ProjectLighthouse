using CsvHelper.Configuration.Attributes;
using System;

namespace ProjectLighthouse.Model.Logistics
{
    public class PackageRecord : IHasFirebaseId
    {
        [Ignore]
        public string FirebaseId { get; set; }
        private DateTime timeStamp;
        public DateTime TimeStamp
        {
            get { return timeStamp; }
            set
            {
                timeStamp = value;
                StrTimeStamp = $"{value:s}";
            }
        }
        [Ignore]
        public string StrTimeStamp { get; set; }
        public string User { get; set; }
        public string User_FirstName { get; set; }
        public string User_LastName { get; set; }
        public string WorkStation { get; set; }
        public string MachineName { get; set; }
        public string DomainUser { get; set; }
        public string OrderReference { get; set; }
        public int NumPackages { get; set; }
    }
}
