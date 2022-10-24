using ProjectLighthouse.Model.Administration;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayMaintenanceEvent : UserControl
    {
        public MaintenanceEvent MaintenanceEvent
        {
            get { return (MaintenanceEvent)GetValue(MaintenanceEventProperty); }
            set { SetValue(MaintenanceEventProperty, value); }
        }

        public static readonly DependencyProperty MaintenanceEventProperty =
            DependencyProperty.Register("MaintenanceEvent", typeof(MaintenanceEvent), typeof(DisplayMaintenanceEvent), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayMaintenanceEvent control)
            {
                return;
            }

            if(control.MaintenanceEvent.LastCompleted == DateTime.MinValue)
            {
                control.LastCompletedText.Text = "Has never been done";
                control.NextDueText.Text = "Due now";
                control.NextDueText.Foreground = (Brush)Application.Current.Resources["Red"];
            }
            else
            {
                control.LastCompletedText.Text = $"Last completed {control.MaintenanceEvent.LastCompleted:dd/MM/yyyy}";
                control.NextDueText.Text = $"Next due {control.MaintenanceEvent.NextDue:dd/MM/yyyy}";

                string colour;
                if (control.MaintenanceEvent.NextDue < DateTime.Now)
                {
                    colour = "Red";
                }
                else if (control.MaintenanceEvent.NextDue < DateTime.Today.AddDays(30))
                {
                    colour = "Orange";
                }
                else
                {
                    colour = "Green";
                }

                control.NextDueText.Foreground = (Brush)Application.Current.Resources[colour];
            }

            if (!control.MaintenanceEvent.Active)
            {
                control.NextDueText.Foreground = (Brush)Application.Current.Resources["OnBackground"];
                control.NextDueText.Text = "Event Inactive";
            }
        }

        public DisplayMaintenanceEvent()
        {
            InitializeComponent();
        }
    }
}
