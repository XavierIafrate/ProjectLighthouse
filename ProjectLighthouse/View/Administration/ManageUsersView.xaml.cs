using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class ManageUsersView : UserControl
    {
        public ManageUsersView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;
            grad.Visibility = scrollViewer.VerticalOffset == 0
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            contentScrollViewer.ScrollToTop();
        }
    }
}
