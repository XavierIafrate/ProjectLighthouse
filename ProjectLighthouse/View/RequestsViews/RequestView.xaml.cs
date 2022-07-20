using System.Windows.Controls;
using System.Windows;

namespace ProjectLighthouse.View
{

    public partial class RequestView : UserControl
    {
        public RequestView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            grad.Visibility = scrollViewer.VerticalOffset == 0
                ? Visibility.Hidden
                : Visibility.Visible;
        }
    }
}
