using ProjectLighthouse.Model.Administration;
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
        public string computerUsername { get; set; }
        public string UserRole { get; set; }    

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
        public bool CanModifyCalibration { get; set; }
        public bool CanAddCalibrationCertificates { get; set; }
        public bool EnableDataSync { get; set; }
        public string DefaultView { get; set; }
        public bool ReceivesNotifications { get; set; }
        public bool CanApproveDrawings { get; set; }
        public bool HasQualityNotifications { get; set; }

        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }

        public UserRole Role
        {
            get
            {
                return UserRole switch
                {
                    "Viewer" => Model.UserRole.Viewer,
                    "Purchasing" => Model.UserRole.Purchasing,
                    "Production" => Model.UserRole.Production,
                    "Scheduling" => Model.UserRole.Scheduling,
                    "admin" => Model.UserRole.Administrator,
                    _ => Model.UserRole.Viewer,
                };
            }
            set { }
        }
    }
}
