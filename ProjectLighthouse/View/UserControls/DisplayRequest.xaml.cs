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
            DisplayRequest requestControl = d as DisplayRequest;
            if (requestControl == null)
                return;
            requestControl.DataContext = requestControl.Request;

            #region colourFormatting
            switch (requestControl.Request.QuantityRequired)
            {
                case >= 1000:
                    requestControl.qtyTextBlock.Foreground = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                    break;

                case < 100:
                    requestControl.qtyTextBlock.Foreground = (Brush)Application.Current.Resources["materialError"];
                    break;

                default:
                    requestControl.qtyTextBlock.Foreground = (Brush)Application.Current.Resources["materialOnBackground"];
                    break;
            }
            #endregion

            #region flags
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
            #endregion
        }

        public DisplayRequest()
        {
            InitializeComponent();
        }
    }
}
