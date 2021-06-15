using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayStatsCard : UserControl
    {
        public MachineStatistics statistics
        {
            get { return (MachineStatistics)GetValue(statisticsProperty); }
            set { SetValue(statisticsProperty, value); }
        }

        public static readonly DependencyProperty statisticsProperty =
            DependencyProperty.Register("statistics", typeof(MachineStatistics), typeof(DisplayStatsCard), new PropertyMetadata(null, setValues));

        private static void setValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayStatsCard control = d as DisplayStatsCard;

            if (control == null)
                return;
            control.DataContext = control.statistics;
            string latheID = DatabaseHelper.Read<Lathe>().Where(n => n.FullName == control.statistics.MachineID).FirstOrDefault().Id;

            double percent = (double)control.statistics.PartCountAll / (double)control.statistics.PartCountTarget;
            percent = Math.Round(percent * 100) / 100;
            if (double.IsNaN(percent))
                percent = 0;

            control.progressGrid.ColumnDefinitions[0].Width = new GridLength(percent, GridUnitType.Star);
            control.progressGrid.ColumnDefinitions[1].Width = new GridLength(1 - percent, GridUnitType.Star);
            control.progressText.Text = String.Format("{0:0}%", percent * 100);

            control.progressBar.Visibility = percent == 0 ? Visibility.Hidden : Visibility.Visible;

            control.completionDateText.Text = control.statistics.EstimateCompletionDate();
            control.estimatedTimeRemaining.Text = control.statistics.EstimateCompletionTimeRemaining();
            control.connectionText.Text = control.statistics.Status.ToUpper();

            string statusColour = String.Empty;
            string accentColour = String.Empty;

            switch (control.statistics.Status)
            {
                case "Running":
                    statusColour = "materialPrimaryGreen";
                    accentColour = "materialPrimary";
                    control.connectionText.Text = "ONLINE";
                    break;
                case "Setting":
                    statusColour = "materialPrimaryBlue";
                    accentColour = "materialPrimary";
                    control.connectionText.Text = "MANUAL OPERATION";
                    break;
                case "Breakdown":
                    statusColour = "materialError";
                    accentColour = "materialError";
                    break;
                case "Offline":
                    statusColour = "materialError";
                    accentColour = "materialError";
                    break;
                case "Idle":
                    statusColour = "materialError";
                    accentColour = "materialError";
                    break;
                default:
                    statusColour = "materialPrimary";
                    accentColour = "materialWhite";
                    control.connectionText.Text = "EXCEPTION";
                    break;
            }

            control.controlBackground.Stroke = (SolidColorBrush)Application.Current.Resources[statusColour];
            control.connectionText.Foreground = (SolidColorBrush)Application.Current.Resources[statusColour];
            control.workProgressSubtitle.Foreground = (SolidColorBrush)Application.Current.Resources[accentColour];
            control.progressText.Foreground = (SolidColorBrush)Application.Current.Resources[statusColour];
            control.progressBar.Fill = (SolidColorBrush)Application.Current.Resources[statusColour];
        }

        public DisplayStatsCard()
        {
            InitializeComponent();
        }
    }
}

