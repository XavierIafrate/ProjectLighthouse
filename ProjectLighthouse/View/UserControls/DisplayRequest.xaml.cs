using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayRequest : UserControl
    {
        public Request Request
        {
            get { return (Request)GetValue(RequestProperty); }
            set { SetValue(RequestProperty, value); }
        }

        public static readonly DependencyProperty RequestProperty =
            DependencyProperty.Register("Request", typeof(Request), typeof(DisplayRequest), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayRequest requestControl)
                return;

            requestControl.DataContext = requestControl.Request;

            requestControl.qtyTextBlock.Foreground = requestControl.Request.QuantityRequired switch
            {
                >= 1000 => (Brush)Application.Current.Resources["materialPrimaryGreen"],
                < 100 => (Brush)Application.Current.Resources["materialError"],
                _ => (Brush)Application.Current.Resources["materialOnBackground"],
            };

            if (requestControl.Request.IsAccepted)
            {
                requestControl.statusBadge.Fill = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                requestControl.statusText.Text = "Accepted";
            }
            else if (requestControl.Request.IsDeclined)
            {
                requestControl.statusBadge.Fill = (Brush)Application.Current.Resources["materialError"];
                requestControl.statusText.Text = "Declined";
            }
            else
            {
                requestControl.statusBadge.Fill = (Brush)Application.Current.Resources["materialPrimaryBlue"];
                requestControl.statusText.Text = "Pending";
            }
        }

        public DisplayRequest()
        {
            InitializeComponent();
        }
    }
}
