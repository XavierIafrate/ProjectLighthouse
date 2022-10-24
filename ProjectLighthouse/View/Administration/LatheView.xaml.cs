using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class LatheView : UserControl
    {
        public LatheView()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SearchBox.Text = "";
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
