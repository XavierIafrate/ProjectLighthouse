using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
{
    public class ManageUsersViewModel : BaseViewModel
    {

        public List<string> roles { get; set; }

        private string selectedRole;
        public string SelectedRole
        {
            get { return selectedRole; }
            set 
            { 
                selectedRole = value;
                if (SelectedUser == null)
                    return;
                if (SelectedUser.UserRole != value.Content)
                    SelectedUser.UserRole = value.Content.ToString();
                OnPropertyChanged("SelectedRole");
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
                if (roles != null && value.UserRole != null)
                    SelectedRole = roles.Where(n => n.ToString() == value.UserRole).Single();
                OnPropertyChanged("SelectedUser");
            }
        }

        private Visibility editControlsVis;
        public Visibility EditControlsVis
        {
            get { return editControlsVis; }
            set 
            { 
                editControlsVis = value;
                OnPropertyChanged("EditControlsVis");
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
        public deleteUserCommand deleteUserCommand { get; set; }

        public ManageUsersViewModel()
        {

            roles = new();
            roles.Add("admin");
            roles.Add("Purchasing");
            roles.Add("Scheduling");
            roles.Add("Production");
            roles.Add("Viewer");

            SelectedRole = new();
            Users = new();
            SelectedUser = new();

            EditControlsVis = Visibility.Collapsed;
            ReadControlsVis = Visibility.Visible;

            editCommand = new(this);
            saveCommand = new(this);
            deleteUserCommand = new(this);

            


            LoadData();

            if (Users.Count > 0)
                SelectedUser = Users.FirstOrDefault();
        }


        private void LoadData()
        {
            Users = DatabaseHelper.Read<User>().OrderBy(n => n.UserName).ToList();
        }

        public void EnableEdit()
        {
            ReadControlsVis = Visibility.Collapsed;
            EditControlsVis = Visibility.Visible;
        }

        public void SaveEdit()
        {
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
    }
}
