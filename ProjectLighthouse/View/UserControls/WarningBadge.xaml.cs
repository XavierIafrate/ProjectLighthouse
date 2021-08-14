using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for WarningBadge.xaml
    /// </summary>
    public partial class WarningBadge : UserControl
    {
        public string WarningText
        {
            get { return (string)GetValue(_warningTextProperty); }
            set { SetValue(_warningTextProperty, value); }
        }

        public static readonly DependencyProperty _warningTextProperty =
            DependencyProperty.Register("_warningText", typeof(string), typeof(WarningBadge), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not WarningBadge control)
            {
                return;
            }

            if (!string.IsNullOrEmpty(control.WarningText))
            {
                control.message.Text = control.WarningText;
            }
        }

        public WarningBadge()
        {
            InitializeComponent();
        }
    }
}
