using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.View.Quality
{
    public partial class RequestNewQualityCheckWindow : Window
    {

        public bool RequestAdded = false;

        public RequestNewQualityCheckWindow()
        {
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(productNameTextBox.Text.Trim()))
            {
                MessageBox.Show("Product Name cannot be blank.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(specDetailsTextBox.Text.Trim()) && string.IsNullOrWhiteSpace(FilePicker.FilePath))
            {
                MessageBox.Show("You must provide a specification through one of the two methods.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            QualityCheck newCheck = new()
            {
                Product = productNameTextBox.Text.Trim().ToUpperInvariant(),
                RaisedAt = DateTime.Now,
                RaisedBy = App.CurrentUser.UserName,
                SpecificationDetails = specDetailsTextBox.Text.Trim(),
                RequiredBy = (DateTime)RequiredDate.SelectedDate,
            };

            if (!string.IsNullOrEmpty(FilePicker.FilePath))
            {
                string newPath = $@"{App.ROOT_PATH}lib\{System.IO.Path.GetFileName(FilePicker.FilePath)}";
                try
                {
                    System.IO.File.Copy(FilePicker.FilePath, newPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
                newCheck.SpecificationDocument = $@"lib\{System.IO.Path.GetFileName(FilePicker.FilePath)}";
            }

            if (DatabaseHelper.Insert(newCheck))
            {
                NotifyPeople();
                this.RequestAdded = true;
                Close();
            }
        }

        void NotifyPeople()
        {
            List<string> otherUsers = new();
            otherUsers.AddRange(App.NotificationsManager.users.Where(x => x.HasQualityNotifications).Select(x => x.UserName));

            otherUsers = otherUsers.Where(x => x != App.CurrentUser.UserName).Distinct().ToList();
            for (int i = 0; i < otherUsers.Count; i++)
            {
                Notification newNotification = new(otherUsers[i], App.CurrentUser.UserName, "New QC Request", $"{App.CurrentUser.FirstName} has requested a quality check on {productNameTextBox.Text.ToUpper().Trim()}");
                _ = DatabaseHelper.Insert(newNotification);
            }
        }
    }
}
