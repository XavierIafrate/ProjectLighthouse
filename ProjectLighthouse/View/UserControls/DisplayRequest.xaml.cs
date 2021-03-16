using ProjectLighthouse.Model;
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
    /// Interaction logic for DisplayRequest.xaml
    /// </summary>
    public partial class DisplayRequest : UserControl
    {
        public Request Request
        {
            get { return (Request)GetValue(RequestProperty); }
            set { SetValue(RequestProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RequestProperty =
            DependencyProperty.Register("Request", typeof(Request), typeof(DisplayRequest), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayRequest requestControl = d as DisplayRequest;

            if (requestControl != null)
            {
                requestControl.DataContext = requestControl.Request;

                #region colourFormatting
                switch (requestControl.Request.QuantityRequired)
                {
                    case >= 1000:
                        requestControl.qtyTextBlock.Foreground = (Brush)Application.Current.Resources["colGood"];
                        break;

                    case < 100:
                        requestControl.qtyTextBlock.Foreground = (Brush)Application.Current.Resources["colCritical"];
                        break;

                    default:
                        requestControl.qtyTextBlock.Foreground = (Brush)Application.Current.Resources["colNeutral"];
                        break;
                }
                #endregion

                #region flags
                requestControl.acceptedFlag.Visibility = Visibility.Collapsed;
                requestControl.declinedFlag.Visibility = Visibility.Collapsed;
                requestControl.pendingFlag.Visibility = Visibility.Visible;

                if (requestControl.Request.IsAccepted)
                {
                    requestControl.acceptedFlag.Visibility = Visibility.Visible;
                    requestControl.declinedFlag.Visibility = Visibility.Collapsed;
                    requestControl.pendingFlag.Visibility = Visibility.Collapsed;
                }
                if (requestControl.Request.IsDeclined)
                {
                    requestControl.acceptedFlag.Visibility = Visibility.Collapsed;
                    requestControl.declinedFlag.Visibility = Visibility.Visible;
                    requestControl.pendingFlag.Visibility = Visibility.Collapsed;
                }
                #endregion
            }
        }

        public DisplayRequest()
        {
            InitializeComponent();
        }
    }
}
