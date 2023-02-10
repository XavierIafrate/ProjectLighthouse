using ProjectLighthouse.Model.Core;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{

    public partial class DisplayNotification : UserControl
    {
        public Notification Notification
        {
            get { return (Notification)GetValue(NotificationProperty); }
            set { SetValue(NotificationProperty, value); }
        }

        public static readonly DependencyProperty NotificationProperty =
            DependencyProperty.Register("Notification", typeof(Notification), typeof(DisplayNotification), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayNotification control)
            {
                return;
            }

            control.DataContext = control.Notification;
            control.SeenIndicator.Visibility = control.Notification.Seen
                ? Visibility.Collapsed
                : Visibility.Visible;
            control.actionButton.IsEnabled = !string.IsNullOrEmpty(control.Notification.ToastAction);
        }

        public DisplayNotification()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.NotificationsManager.ExecuteToastAction(Notification.ToastAction);
            App.NotificationsManager.EnsureMarkedRead(Notification);
        }
    }
}
