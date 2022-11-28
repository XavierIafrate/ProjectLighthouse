using ProjectLighthouse.Model.Research;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;
using System.Windows.Media;

namespace ProjectLighthouse.View.Research
{
    public partial class NewProjectWindow : Window
    {
        public ResearchProject NewProject { get; set; }
        public bool SaveExit = false;

        public NewProjectWindow()
        {
            InitializeComponent();
            NewProject = new();
        }

        private bool DataIsComplete()
        {
            bool result = true;

            Brush transparent = Brushes.Transparent;
            Brush error = (Brush)Application.Current.Resources["Red"];

            if (string.IsNullOrWhiteSpace(NewProject.ProjectName))
            {
                result = false;
                projectNameTextBox.BorderBrush = error;
            }
            else
            {
                projectNameTextBox.BorderBrush = transparent;
            }

            return result;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataIsComplete())
            {
                MessageBox.Show("Some of the information provided is invalid.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            NewProject.ProjectName = NewProject.ProjectName.Trim();

            NewProject.Notes = new();
            NewProject.Archetypes = new();
            NewProject.CreatedAt = DateTime.Now;
            NewProject.CreatedBy = App.CurrentUser.UserName;

            SaveExit = true;
            Close();
        }

        private void projectNameTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateAlphanumeric(e, allowSpace: true); ;
        }
    }
}
