using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayScheduleWarning : UserControl
    {
        public ScheduleWarning Warning
        {
            get { return (ScheduleWarning)GetValue(WarningProperty); }
            set { SetValue(WarningProperty, value); }
        }

        public static readonly DependencyProperty WarningProperty =
            DependencyProperty.Register("Warning", typeof(ScheduleWarning), typeof(DisplayScheduleWarning), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayScheduleWarning control)
            {
                return;
            }

            control.DataContext = control.Warning;
            if (control.Warning.Important)
            {
                control.Background.Background = (Brush)Application.Current.Resources["materialError"];
                control.Background.BorderThickness = new Thickness(2);
                control.WarningText.Foreground = (Brush)Application.Current.Resources["materialOnError"];
                control.title.Foreground = (Brush)Application.Current.Resources["materialOnError"];
                control.icon.Fill = (Brush)Application.Current.Resources["materialOnError"];

            }
            else
            {
                control.Background.Background = (Brush)Application.Current.Resources["materialYellow"];
                control.Background.BorderThickness = new Thickness(2);
                control.WarningText.Foreground = (Brush)Application.Current.Resources["materialOffBlack"];
                control.title.Foreground = (Brush)Application.Current.Resources["materialOffBlack"];
                control.icon.Fill = (Brush)Application.Current.Resources["materialOffBlack"];
            }
        }

        public DisplayScheduleWarning()
        {
            InitializeComponent();
        }
    }
}
