using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
