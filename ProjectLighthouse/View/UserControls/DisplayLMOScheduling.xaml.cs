using ProjectLighthouse.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayLMOScheduling : UserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public CompleteOrder orderObject
        {
            get { return (CompleteOrder)GetValue(orderObjectProperty); }
            set 
            { 
                SetValue(orderObjectProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("orderObject"));
            }
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
                        control.bg.Background = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                        control.statusBadgeText.Fill = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                        break;
                    case "Awaiting scheduling":
                        control.bg.Background = (Brush)Application.Current.Resources["materialError"];
                        control.statusBadgeText.Fill = (Brush)Application.Current.Resources["materialError"];
                        break;
                    case "Running":
                        control.bg.Background = (Brush)Application.Current.Resources["materialPrimaryBlue"];
                        control.statusBadgeText.Fill = (Brush)Application.Current.Resources["materialPrimaryBlue"];
                        break;
                    case "Problem":
                        control.bg.Background = (Brush)Application.Current.Resources["materialError"];
                        control.statusBadgeText.Fill = (Brush)Application.Current.Resources["materialError"];
                        break;
                    default:
                        control.bg.Background = (Brush)Application.Current.Resources["materialError"];
                        control.statusBadgeText.Fill = (Brush)Application.Current.Resources["materialError"];
                        break;
                }
            }
            control.editButton.Visibility = App.CurrentUser.UserRole is "Scheduling" or "admin"
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public DisplayLMOScheduling()
        {
            InitializeComponent();
        }
    }
}
