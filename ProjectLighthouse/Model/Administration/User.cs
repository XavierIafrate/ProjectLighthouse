using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string UserRole { get; set; }
        public string computerUsername { get; set; }

        private DateTime lastLogin;

        public DateTime LastLogin
        {
            get { return lastLogin; }
            set
            {
                lastLogin = value;
                LastLoginText = value.ToString("s");
            }
        }

        public string LastLoginText { get; set; }

        public bool IsBlocked { get; set; }
        public bool CanApproveRequests { get; set; }
        public bool CanEditLMOs { get; set; }
        public bool CanUpdateLMOs { get; set; }
        public bool CanRaiseDelivery { get; set; }
        public bool CanCreateAssemblyProducts { get; set; }
        public bool CanRaiseRequest { get; set; }
        public bool CanCreateSpecial { get; set; }

        public string DefaultView { get; set; }

        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
