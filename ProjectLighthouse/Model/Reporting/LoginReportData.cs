using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.Model.Reporting
{
    public class LoginReportData
    {
        public List<User> Users { get; set; }
        public List<Login> Logins { get; set; }
        public DateTime From { get; set; }  
        public DateTime To { get; set; }    

        public List<UserLoginRecords> UserLogins { get; set;}

        public LoginReportData(List<User> users, List<Login> logins, DateTime fromDate, DateTime toDate)
        {
            From = fromDate;
            To = toDate;
            Users = users;
            Logins = logins.Where(l => l.Time >= fromDate && l.Time <= toDate).ToList();
            UserLogins = new();

            for (int i = 0; i < Users.Count; i++)
            {
                UserLogins.Add(
                    new(
                        user:Users[i],
                        logins:Logins.Where(x => x.User == Users[i].UserName).OrderBy(x => x.Time).ToList()
                        )
                    );
            }
        }
    }


    public class UserLoginRecords
    {
        public User User { get; set; }  
        public List<Login> UserLogins { get; set; }
        public UserLoginRecords(User user, List<Login> logins)
        {
            User = user;
            UserLogins = logins;
        }
    }
}
