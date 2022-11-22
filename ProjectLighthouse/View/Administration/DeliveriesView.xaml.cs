using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class DeliveriesView : UserControl
    {
        public DeliveriesView()
        {
            InitializeComponent();
        }

        private void clearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            searchBox.Text = "";
        }
    }
}
