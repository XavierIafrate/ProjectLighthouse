using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Reporting;
using ProjectLighthouse.View.Administration;
using ProjectLighthouse.ViewModel.Commands.UserManagement;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class ManageUsersViewModel : BaseViewModel
    {
        #region Vars
        public Array Roles { get; set; }
        public List<string> Views { get; set; }
        public List<Login> Logins { get; set; }
        public List<Login> UserLogins { get; set; }

        private List<Permission> allPermissionsGiven;

        private List<EditablePermission> selectedUserPermissions;

        public List<EditablePermission> SelectedUserPermissions
        {
            get { return selectedUserPermissions; }
            set
            {
                selectedUserPermissions = value;
                OnPropertyChanged();
            }
        }

        private EditablePermission selectedPermission;

        public EditablePermission SelectedPermission
        {
            get { return selectedPermission; }
            set { selectedPermission = value; OnPropertyChanged(); }
        }

        private UserRole selectedRole;
        public UserRole SelectedRole
        {
            get { return selectedRole; }
            set
            {
                selectedRole = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private List<User> users;
        public List<User> Users
        {
            get { return users; }
            set
            {
                users = value;
                OnPropertyChanged();
            }
        }

        private User selectedUser;
        public User SelectedUser
        {
            get { return selectedUser; }
            set
            {
                selectedUser = value;
                if(editMode)
                {
                    CancelEdit();
                }
                LoadUserDetails();
                OnPropertyChanged();
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(editMode));
            }
        }

        private Visibility readControlsVis;
        public Visibility ReadControlsVis
        {
            get { return readControlsVis; }
            set
            {
                readControlsVis = value;
                OnPropertyChanged();
            }
        }

        public EditUserCommand EditCmd { get; set; }
        public SaveUserEditCommand SaveCmd { get; set; }
        public CancelEditCommand CancelCmd { get; set; }
        public AddUserCommand AddUserCmd { get; set; }
        public DeleteUserCommand DeleteUserCmd { get; set; }
        public ResetUserPasswordCommand ResetPasswordCmd { get; set; }
        public GrantPermissionCommand AddPermissionCmd { get; set; }
        public RevokePermissionCommand RemovePermissionCmd { get; set; }


        #endregion
        public ManageUsersViewModel()
        {
            Roles = Enum.GetValues(typeof(UserRole));
            Views = new() { "Requests", "Orders", "Schedule" };

            SelectedUserPermissions = new();
            Logins = new();
            Users = new();
            SelectedUser = new();
            UserLogins = new();

            EditControlsVis = Visibility.Collapsed;
            ReadControlsVis = Visibility.Visible;

            EditCmd = new(this);
            SaveCmd = new(this);
            CancelCmd = new(this);
            DeleteUserCmd = new(this);
            ResetPasswordCmd = new(this);
            AddPermissionCmd = new(this);
            RemovePermissionCmd = new(this);
            AddUserCmd = new(this);

            LoadData();

            if (Users.Count > 0)
            {
                SelectedUser = Users.First();
            }
        }

        private void LoadData()
        {
            Users = DatabaseHelper.Read<User>().OrderBy(n => n.UserName).ToList();
            allPermissionsGiven = DatabaseHelper.Read<Permission>().ToList();

            for (int i = 0; i < Users.Count; i++)
            {
                Users[i].UserPermissions = allPermissionsGiven.Where(x => x.UserId == Users[i].Id).ToList();
            }

            Logins = DatabaseHelper.Read<Login>().OrderByDescending(x => x.Time).ToList();
        }

        public void EnableEdit()
        {
            if (SelectedUser is null) return;

            if(SelectedUser.UserName == "sysadmin")
            {
                MessageBox.Show("This account cannot be edited", "Action prevented", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ReadControlsVis = Visibility.Collapsed;
            EditControlsVis = Visibility.Visible;
        }


        internal void CancelEdit()
        {
            ReadControlsVis = Visibility.Visible;
            EditControlsVis = Visibility.Collapsed;
        }

        public void SaveEdit()
        {
            if (!IsValidEmail(SelectedUser.EmailAddress) && !string.IsNullOrEmpty(SelectedUser.EmailAddress))
            {
                MessageBox.Show("Invalid email address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ReadControlsVis = Visibility.Visible;
            EditControlsVis = Visibility.Collapsed;
            SelectedUser.Role = SelectedRole;
            DatabaseHelper.Update(SelectedUser);
            int user = SelectedUser.Id;
            LoadData();

            if (Users.Count == 0)
            {
                return;
            }

            if (Users.Any(x => x.Id == user))
            {
                SelectedUser = Users.Find(x => x.Id == user);
            }
            else
            {
                SelectedUser = Users.First();
            }
        }

        public void DeleteUser()
        {
            MessageBoxResult Result = MessageBox.Show($"Are you sure you want to delete {SelectedUser.GetFullName()}?\nThis cannot be undone.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (Result != MessageBoxResult.Yes)
            {
                return;
            }
            DatabaseHelper.Delete(SelectedUser);
            LoadData();
            if (Users.Count > 0)
            {
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

        void LoadUserDetails()
        {
            if (SelectedUser == null)
            {
                return;
            }

            Array allPermissions = Enum.GetValues(typeof(PermissionType));
            List<EditablePermission> newPermissions = new();

            for (int i = 0; i < allPermissions.Length; i++)
            {
                if (allPermissions.GetValue(i) is not PermissionType p) continue;
                newPermissions.Add(new(p, SelectedUser.HasPermission(p), SelectedUser.PermissionInherited(p)));
            }

            SelectedUserPermissions = newPermissions;

            SelectedRole = SelectedUser.Role;
            SelectedView = Views.Find(n => n == (string.IsNullOrEmpty(SelectedUser.DefaultView) ? "Orders" : SelectedUser.DefaultView));

            UserLogins = Logins.Where(x => x.User == SelectedUser.UserName && x.Time.AddDays(14) > DateTime.Now).ToList() ?? new();
            OnPropertyChanged(nameof(UserLogins));
        }

        public void CreateNewUser()
        {
            NewUserWindow window = new() { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (window.SaveExit)
            {
                LoadData();

                SelectedUser = Users.Find(x => x.UserName == window.username.Text);
            }
        }

        public void AddPermission()
        {
            if (SelectedPermission == null)
            {
                MessageBox.Show("No permission is selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedUser.HasPermission(SelectedPermission.Action))
            {
                MessageBox.Show($"User already has permission to {SelectedPermission.DisplayText}", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Permission newPermission = new()
            {
                UserId = SelectedUser.Id,
                PermittedAction = SelectedPermission.Action
            };

            DatabaseHelper.Insert(newPermission);

            allPermissionsGiven.Add(newPermission);
            SelectedUser.UserPermissions.Add(newPermission);

            LoadUserDetails();
        }

        public void RevokePermission()
        {
            if (SelectedPermission == null)
            {
                MessageBox.Show("No permission is selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!SelectedUser.HasPermission(SelectedPermission.Action))
            {
                MessageBox.Show($"User already unable to '{SelectedPermission.DisplayText}'", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedUser.RoleGrantsPermission(SelectedPermission.Action))
            {
                MessageBox.Show($"The action '{SelectedPermission.DisplayText}' cannot be revoked as it is granted by the user type.", "Cannot revoke permission", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Permission toDelete = SelectedUser.UserPermissions.Find(x => x.PermittedAction == SelectedPermission.Action);

            DatabaseHelper.Delete(toDelete);

            allPermissionsGiven.Remove(toDelete);
            SelectedUser.UserPermissions.Remove(toDelete);

            LoadUserDetails();
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
            List<User> reportUsers = Users.Where(u => u.Role is UserRole.Purchasing or UserRole.Production or UserRole.Scheduling).ToList();
            LoginReportData reportData = new(reportUsers, Logins, DateTime.Today.AddDays(-7), DateTime.Now);
            string path = $@"C:\Users\x.iafrate\Documents\LoginReport_{DateTime.Now:yyMMdd_HHmmss}.pdf";

            reportService.Export(path, reportData);
            reportService.OpenPdf(path);
        }
    }
}
