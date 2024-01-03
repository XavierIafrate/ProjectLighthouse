using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Programs
{
    public partial class ProgamManager : UserControl
    {
        public ProgamManager()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            searchBox.Clear();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;

            grad.Visibility = scrollViewer.VerticalOffset == 0
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void TagButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button tagButton) return;
            searchBox.Text = (string)tagButton.Tag;
        }
    }
}
