using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserRole { get; set; }
        public bool IsBlocked { get; set; }
        public bool CanApproveRequests { get; set; }
        public bool CanEditLMOs { get; set; }
        public bool CanUpdateLMOs { get; set; }
        public string computerUsername { get; set; }

        public string GetFullName()
        {
            return String.Format("{0} {1}", FirstName, LastName);
        }
    }
}
