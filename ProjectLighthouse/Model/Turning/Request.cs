﻿using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class Request
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

        //public bool isSchedulingApproved { get; set; } = false;
        //public bool isProductionApproved { get; set; } = false;
        public bool IsDeclined { get; set; } = false;
        public bool IsAccepted { get; set; } = false;
        public string AcceptedBy { get; set; }

        public string DeclinedReason { get; set; } = "";
        public string ResultingLMO { get; set; }
        public string Status { get; set; } = "Pending approval";
        public string Likeliness { get; set; }
        public string Notes { get; set; }

        public bool CleanCustomerRequirement { get; set; }
    }
}