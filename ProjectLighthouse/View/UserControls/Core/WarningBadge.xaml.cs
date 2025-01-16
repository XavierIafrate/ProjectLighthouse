using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class WarningBadge : UserControl
    {
        public string WarningText
        {
            get { return (string)GetValue(warningTextProperty); }
            set { SetValue(warningTextProperty, value); }
        }

        public static readonly DependencyProperty warningTextProperty =
            DependencyProperty.Register("warningText", typeof(string), typeof(WarningBadge), new PropertyMetadata(null, SetValues));

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
