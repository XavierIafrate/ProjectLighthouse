using ProjectLighthouse.Model.Research;
using ProjectLighthouse.ViewModel.Helpers;
using System.Windows;

namespace ProjectLighthouse.View.Research
{
    public partial class AddNewArchetypeWindow : Window
    {
        public ResearchArchetype NewArchetype { get; set; }
        public bool SaveExit = false;

        public AddNewArchetypeWindow(ResearchProject project)
        {
            InitializeComponent();

            NewArchetype = new()
            {
                ProjectId = project.Id,
            };
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            NewArchetype.ValidateAll();

            if (NewArchetype.HasErrors)
            {
                return;
            }

            SaveExit = true;
            Close();
        }

        private void NameTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateAlphanumeric(e, true);
        }
    }
}
