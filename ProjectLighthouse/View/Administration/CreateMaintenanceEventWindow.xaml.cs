using ProjectLighthouse;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace View.Administration
{
    public partial class CreateMaintenanceEventWindow : Window
    {
        private Lathe lathe;
        public bool SaveExit;
        public MaintenanceEvent AddedEvent;
        public MaintenanceEvent existingEvent;
        public bool editing;

        public CreateMaintenanceEventWindow(Lathe lathe, MaintenanceEvent? existingEvent = null)
        {
            InitializeComponent();
            forLatheText.Text = $"For {lathe.FullName}";
            StartDate.SelectedDate = DateTime.Today;

            StartDate.DisplayDateStart = new DateTime(year: 2020, month: 1, day: 1);
            StartDate.DisplayDateEnd = DateTime.Now.AddYears(1);

            this.lathe = lathe;
            ActiveCheckBox.Visibility = Visibility.Collapsed;
            RequireDocs.Visibility = Visibility.Collapsed;
            AddServiceRecordControls.Visibility = Visibility.Collapsed;

            if (existingEvent != null)
            {
                editing = true;
                this.existingEvent = existingEvent;
                titleText.Text = $"Editing '{existingEvent.Description}'";
                this.Title= "Editing Maintenance Event";
                descriptionTextBox.IsEnabled = App.CurrentUser.Role == UserRole.Administrator;
                descriptionTextBox.Text = existingEvent.Description;
                intervalText.Text = existingEvent.IntervalMonths.ToString("0");
                StartDate.SelectedDate = existingEvent.StartingDate;
                AddButton.Content = "Update Event";

                RequireDocs.IsChecked = existingEvent.RequireDocumentation;
                RequireDocs.Visibility = Visibility.Visible;


                ActiveCheckBox.IsChecked = existingEvent.Active;
                ActiveCheckBox.Visibility = Visibility.Visible;

                AddServiceRecordControls.Visibility = Visibility.Visible;

                RecordIssueDate.DisplayDateStart = existingEvent.LastCompleted;
                RecordIssueDate.SelectedDate= DateTime.Today;

                LastCompleteText.Text = existingEvent.LastCompleted == DateTime.MinValue ? "This event has never been completed." : $"Last completed {existingEvent.LastCompleted:dd/MM/yyyy}.";
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            descriptionTextBox.BorderBrush = Brushes.Transparent;
            intervalText.BorderBrush = Brushes.Transparent;

            if (!int.TryParse(intervalText.Text, out int interval))
            {
                intervalText.BorderBrush = (Brush)Application.Current.Resources["Red"];
                return;
            }

            if (string.IsNullOrWhiteSpace(descriptionTextBox.Text))
            {
                descriptionTextBox.BorderBrush = (Brush)Application.Current.Resources["Red"];
                return;
            }

            if (!editing)
            {
                MaintenanceEvent newEvent = new()
                {
                    Lathe = lathe.Id,
                    Description = descriptionTextBox.Text.Trim(),
                    CreatedBy = App.CurrentUser.UserName,
                    CreatedAt = DateTime.Now,
                    StartingDate = StartDate.SelectedDate.Value.Date,
                    IntervalMonths = interval,
                    Active = true,
                    RequireDocumentation = true,
                };

                SaveExit = DatabaseHelper.Insert(newEvent);

                if (SaveExit)
                {
                    AddedEvent = newEvent;
                }
            }
            else
            {
                existingEvent.Description = descriptionTextBox.Text.Trim();
                existingEvent.StartingDate = StartDate.SelectedDate.Value.Date;
                existingEvent.IntervalMonths = interval;
                existingEvent.Active = ActiveCheckBox.IsChecked ?? false;
                existingEvent.RequireDocumentation = RequireDocs.IsChecked ?? false;

                SaveExit = DatabaseHelper.Update(existingEvent);

            }

            Close();
        }

        private void descriptionTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            descriptionTextBox.BorderBrush = string.IsNullOrWhiteSpace(descriptionTextBox.Text)
                ? (Brush)Application.Current.Resources["Red"]
                : Brushes.Transparent;
        }

        private void descriptionTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            descriptionTextBox.Text = descriptionTextBox.Text.Trim();

            if (autoFormatCheckBox.IsChecked ?? false)
            {
                if (descriptionTextBox.Text != CultureInfo.CurrentCulture.TextInfo.ToTitleCase(descriptionTextBox.Text))
                {
                    descriptionTextBox.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(descriptionTextBox.Text);
                    autoFormatCheckBox.Visibility = Visibility.Visible;
                }
            }
        }

        private void intervalText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void UploadServiceRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if(existingEvent.RequireDocumentation)
            {
                if (string.IsNullOrWhiteSpace(FilePicker.FilePath))
                {
                    MessageBox.Show("No file has been selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!File.Exists(FilePicker.FilePath))
                {
                    MessageBox.Show("The selected file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(FilePicker.FilePath))
            {
                if (MessageBox.Show($"You are about to mark this as completed on {RecordIssueDate.SelectedDate.Value.Date:dd/MM/yyyy} without uploading any documentation.{Environment.NewLine}" +
                    $"Are you sure?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            if (UpdateStartDateCheckBox.IsChecked ?? false)
            {
                existingEvent.StartingDate = RecordIssueDate.SelectedDate.Value.Date;
                StartDate.SelectedDate = RecordIssueDate.SelectedDate.Value.Date;
            }
            existingEvent.LastCompleted = RecordIssueDate.SelectedDate.Value.Date;
            DatabaseHelper.Update(existingEvent);

            if (!string.IsNullOrWhiteSpace(FilePicker.FilePath))
            {
                Attachment record = new()
                {
                    DocumentReference = $"s{existingEvent.Id}",
                    CreatedAt = DateTime.Now,
                    CreatedBy = App.CurrentUser.UserName,
                };

                record.CopyToStore(FilePicker.FilePath);
                DatabaseHelper.Insert(record);
            }

            SaveExit = true;
            Close();
        }
    }
}
