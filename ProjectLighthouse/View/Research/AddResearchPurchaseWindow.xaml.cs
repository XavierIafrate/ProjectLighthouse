using ProjectLighthouse.Model.Research;
using System.Windows;

namespace ProjectLighthouse.View.Research
{
    public partial class AddResearchPurchaseWindow : Window
    {
        public ResearchPurchase NewPurchase { get; set; }
        public ResearchProject Project { get; set; }
        public bool SaveExit = false;
        public AddResearchPurchaseWindow(ResearchProject project)
        {
            InitializeComponent();
            NewPurchase = new() { ProjectId = project.Id };
            Project= project;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            NewPurchase.ValidateAll();

            if (NewPurchase.HasErrors)
            {
                return;
            }

            SaveExit = true;
            Close();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ArchetypeComboBox.SelectedValue = null;
        }
    }
}
