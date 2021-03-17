using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static User currentUser = new User
        {
            FirstName = "Xavier",
            LastName = "Iafrate",
            UserName = "xav",
            Password = "123456",
            UserRole = "admin",
            CanApproveRequests = true,
            IsBlocked = false,
            CanEditLMOs = true
        };
    }
}
