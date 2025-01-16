using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class MachineView : UserControl
    {
        public MachineView()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SearchBox.Text = "";
        }
    }
}
