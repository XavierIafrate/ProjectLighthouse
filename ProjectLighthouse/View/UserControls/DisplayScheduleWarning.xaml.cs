using ProjectLighthouse.Model.Scheduling;
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
            string baseColour = control.Warning.Important ? "Red" : "Orange";


            control.elementBackground.Background = (Brush)Application.Current.Resources[baseColour];
            control.elementBorder.BorderBrush = (Brush)Application.Current.Resources[baseColour];
            control.WarningText.Foreground = (Brush)Application.Current.Resources[baseColour];
            control.title.Foreground = (Brush)Application.Current.Resources[baseColour];
            control.icon.Fill = (Brush)Application.Current.Resources[baseColour];
        }

        public DisplayScheduleWarning()
        {
            InitializeComponent();
        }
    }
}
