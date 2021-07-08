using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayLMOScheduling.xaml
    /// </summary>
    public partial class DisplayLMOScheduling : UserControl
    {
        public CompleteOrder orderObject
        {
            get { return (CompleteOrder)GetValue(orderObjectProperty); }
            set { SetValue(orderObjectProperty, value); }
        }

        public static readonly DependencyProperty orderObjectProperty =
            DependencyProperty.Register("orderObject", typeof(CompleteOrder), typeof(DisplayLMOScheduling), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayLMOScheduling control = d as DisplayLMOScheduling;

            if (control != null)
            {
                control.DataContext = control.orderObject;
                switch (control.orderObject.Order.Status)
                {
                    case "Ready":
                        control.bg.Fill = (Brush)App.Current.Resources["materialPrimaryGreen"];
                        control.statusBadgeText.Fill = (Brush)App.Current.Resources["materialPrimaryGreen"];
                        break;
                    case "Awaiting scheduling":
                        control.bg.Fill = (Brush)App.Current.Resources["materialError"];
                        control.statusBadgeText.Fill = (Brush)App.Current.Resources["materialError"];
                        break;
                    case "Running":
                        control.bg.Fill = (Brush)App.Current.Resources["materialPrimaryBlue"];
                        control.statusBadgeText.Fill = (Brush)App.Current.Resources["materialPrimaryBlue"];
                        break;
                    case "Problem":
                        control.bg.Fill = (Brush)App.Current.Resources["materialError"];
                        control.statusBadgeText.Fill = (Brush)App.Current.Resources["materialError"];
                        break;
                    default:
                        control.bg.Fill = (Brush)App.Current.Resources["materialError"];
                        control.statusBadgeText.Fill = (Brush)App.Current.Resources["materialError"];
                        break;
                }
            }
            control.editButton.Visibility = App.currentUser.UserRole == "Scheduling" || App.currentUser.UserRole == "admin" ? Visibility.Visible : Visibility.Collapsed;
        }

        public DisplayLMOScheduling()
        {
            InitializeComponent();
        }
    }
}
