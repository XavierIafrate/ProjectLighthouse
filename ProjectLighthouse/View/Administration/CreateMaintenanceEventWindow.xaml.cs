using ProjectLighthouse;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace View.Administration
{
    public partial class CreateMaintenanceEventWindow : Window
    {
        private Lathe lathe;
        public bool SaveExit;
        public CreateMaintenanceEventWindow(Lathe lathe, bool edit = false)
        {
            InitializeComponent();
            forLatheText.Text = $"For {lathe.FullName}";
            StartDate.SelectedDate = DateTime.Today;

            this.lathe = lathe;

            if (edit)
            {
                descriptionTextBox.IsEnabled = App.CurrentUser.Role == UserRole.Administrator;
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

            MaintenanceEvent newEvent = new()
            {
                Lathe = lathe.Id,
                Description = descriptionTextBox.Text.Trim(),
                CreatedBy = App.CurrentUser.UserName,
                CreatedAt = DateTime.Now,
                StartingDate = StartDate.SelectedDate.Value.Date,
                IntervalMonths = interval,
                Active = true,
            };

            SaveExit = DatabaseHelper.Insert(newEvent);
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

            if ((bool)autoFormatCheckBox.IsChecked)
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
    }
}
