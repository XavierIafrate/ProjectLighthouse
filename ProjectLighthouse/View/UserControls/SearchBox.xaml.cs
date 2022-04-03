using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class SearchBox : UserControl
    {
        public string Ghost
        {
            get { return (string)GetValue(GhostProperty); }
            set { SetValue(GhostProperty, value); }
        }

        public static readonly DependencyProperty GhostProperty =
            DependencyProperty.Register("Ghost", typeof(string), typeof(SearchBox), new PropertyMetadata("search...", SetGhost));
        private static void SetGhost(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SearchBox control)
            {
                return;
            }

            //control.ghostText.Text = GhostProperty;
        }

        public string SearchTerm
        {
            get { return (string)GetValue(SearchTermProperty); }
            set { SetValue(SearchTermProperty, value); }
        }

        public static readonly DependencyProperty SearchTermProperty =
            DependencyProperty.Register("SearchTerm", typeof(string), typeof(SearchBox), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SearchBox control)
            {
                return;
            }

            control.DataContext = control.SearchTerm;
        }

        public SearchBox()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTerm = null;
        }
    }
}
