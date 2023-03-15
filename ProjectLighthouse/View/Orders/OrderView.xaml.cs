using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders
{
    public partial class OrderView : UserControl
    {
        public OrderView()
        {
            InitializeComponent();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearButton.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
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
            OrderScroller?.ScrollToVerticalOffset(0);
        }

        private void TabItem_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not TabItem tabItem) return;

            if (!tabItem.IsSelected) return;
            if (tabItem.IsEnabled) return;

            for (int i = 0; i < OrderTabControl.Items.Count; i++)
            {
                TabItem x = (TabItem)OrderTabControl.Items[i];
                if (x.IsEnabled)
                {
                    OrderTabControl.SelectedIndex = i;
                    return;
                }
            }
        }
    }
}
