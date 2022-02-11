using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Reporting;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class ManageUsersViewModel : BaseViewModel
    {
        #region Vars
        public List<string> roles { get; set; }
        public List<string> views { get; set; }
        public List<Login> Logins { get; set; }

        public List<Login> UserLogins { get; set; }

        private string selectedRole;
        public string SelectedRole
        {
            get { return selectedRole; }
            set
            {
                selectedRole = value;
                if (SelectedUser == null)
                    return;
                if (SelectedUser.UserRole != value)
                    SelectedUser.UserRole = value.ToString();
                OnPropertyChanged("SelectedRole");
            }
        }

        private string selectedView;
        public string SelectedView
        {
            get { return selectedView; }
            set
            {
                selectedView = value;
                if (SelectedUser == null)
                    return;
                if (SelectedUser.DefaultView != value)
                    SelectedUser.DefaultView = value.ToString();
                OnPropertyChanged("SelectedView");
            }
        }

        private List<User> users;
        public List<User> Users
        {
            get { return users; }
            set
            {
                users = value;
                OnPropertyChanged("Users");
            }
        }

        private User selectedUser;
        public User SelectedUser
        {
            get { return selectedUser; }
            set
            {
                selectedUser = value;
                if (value != null)
                {
                    if (roles != null && value.UserRole != null)
                        SelectedRole = roles.Where(n => n.ToString() == value.UserRole).Single();
                    if (views != null && value.DefaultView != null)
                        SelectedView = views.Where(n => n.ToString() == (string.IsNullOrEmpty(value.DefaultView) ? "Orders" : value.DefaultView)).Single();
                    if (value.UserName != null)
                    {
                        UserLogins = Logins.Where(x => x.User == value.UserName && x.Time.AddDays(14) > DateTime.Now).ToList();
                        OnPropertyChanged(nameof(UserLogins));
                    }
                    
                }

                OnPropertyChanged("SelectedUser");
            }
        }

        public bool editMode { get; set; }

        private Visibility editControlsVis;
        public Visibility EditControlsVis
        {
            get { return editControlsVis; }
            set
            {
                editControlsVis = value;
                editMode = value == Visibility.Visible;
                OnPropertyChanged("EditControlsVis");
                OnPropertyChanged("editMode");
            }
        }

        private Visibility readControlsVis;
        public Visibility ReadControlsVis
        {
            get { return readControlsVis; }
            set
            {
                readControlsVis = value;
                OnPropertyChanged("ReadControlsVis");
            }
        }

        public EditUserCommand editCommand { get; set; }
        public SaveUserEditCommand saveCommand { get; set; }
        public DeleteUserCommand deleteUserCommand { get; set; }
        public ResetUserPasswordCommand resetPasswordCommand { get; set; }
        public GetLoginReportCommand getLoginReportCommand { get; set; }
        #endregion
        public ManageUsersViewModel()
        {
            roles = new() { "admin", "Purchasing", "Scheduling", "Production", "Viewer" };
            views = new() { "View Requests", "Orders", "Schedule", "Assembly Orders" };

            SelectedRole = string.Empty;
            Users = new();
            SelectedUser = new();
            Logins = new();
            UserLogins = new();

            EditControlsVis = Visibility.Collapsed;
            ReadControlsVis = Visibility.Visible;

            editCommand = new(this);
            saveCommand = new(this);
            deleteUserCommand = new(this);
            resetPasswordCommand = new(this);
            getLoginReportCommand = new(this);

            LoadData();

            if (Users.Count > 0)
                SelectedUser = Users.FirstOrDefault();
        }

        private void LoadData()
        {
            Users = DatabaseHelper.Read<User>().OrderBy(n => n.UserName).ToList();
            Logins = DatabaseHelper.Read<Login>().OrderByDescending(x => x.Time).ToList();
        }

        public void EnableEdit()
        {
            ReadControlsVis = Visibility.Collapsed;
            EditControlsVis = Visibility.Visible;
        }

        public void SaveEdit()
        {

            if (!IsValidEmail(SelectedUser.EmailAddress) && !string.IsNullOrEmpty(SelectedUser.EmailAddress))
            {
                Console.WriteLine($"email invalid: '{SelectedUser.EmailAddress}'");
                MessageBox.Show("Invalid email address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ReadControlsVis = Visibility.Visible;
            EditControlsVis = Visibility.Collapsed;

            DatabaseHelper.Update<User>(SelectedUser);
            string username = SelectedUser.UserName;
            LoadData();
            if (Users.Count > 0)
                foreach (User u in Users)
                    if (u.UserName == username)
                        SelectedUser = u;
        }

        public void DeleteUser()
        {
            MessageBoxResult Result = MessageBox.Show($"Are you sure you want to delete {SelectedUser.GetFullName()}?\nThis cannot be undone.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (Result == MessageBoxResult.Yes)
            {
                DatabaseHelper.Delete<User>(SelectedUser);
                LoadData();
                if (Users.Count > 0)
                    SelectedUser = Users.FirstOrDefault();
            }
        }

        public void ResetPassword()
        {
            Random rand = new();
            string newPassword = string.Empty;
            for (int i = 0; i <= 4; i++)
                newPassword += $"{rand.Next(0, 9):N0}";

            SelectedUser.Password = newPassword;
            SaveEdit();

            MessageBox.Show($"{SelectedUser.GetFullName()}'s new password is {newPassword}.\nOnce logged in, the password can be changed in the settings.", "Password Reset", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void CreateNewUser()
        {

        }

        public static bool IsValidEmail(string email)  // Stolen from MS Docs
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                static string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    IdnMapping idn = new();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public void GenerateReport()
        {
            ReportPdf reportService = new();
            List<User> reportUsers = Users.Where(u => u.UserRole is "admin" or "Purchasing" or "Production" or "Scheduling").ToList();
            LoginReportData reportData = new(reportUsers, Logins, DateTime.Today.AddDays(-7), DateTime.Now);
            string path = @"C:\Users\x.iafrate\Documents\LoginReport.pdf";

            reportService.Export(path, reportData);
            reportService.OpenPdf(path);
        }
    }
}
