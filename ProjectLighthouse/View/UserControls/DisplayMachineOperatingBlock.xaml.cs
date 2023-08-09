using ProjectLighthouse.Model.Analytics;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayMachineOperatingBlock : UserControl
    {
        public MachineOperatingBlock Block
        {
            get { return (MachineOperatingBlock)GetValue(BlockProperty); }
            set { SetValue(BlockProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Block.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlockProperty =
            DependencyProperty.Register("Block", typeof(MachineOperatingBlock), typeof(DisplayMachineOperatingBlock), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayMachineOperatingBlock control) return;

            if (control.Block is null) return;
            string colour = control.Block.State switch
            {
                "Running" => "Green",
                "Setting" => "Blue",
                "Breakdown" => "Red",
                "Offline" => "OnBackground",
                "Idle" => "Purple",
                "No Data" => "Orange",
                _ => "Yellow"
            };

            if (control.Block.State == "Running")
            {
                control.ProducedIndicator.Text = $"Theoretical Production: {control.Block.SecondsElapsed / control.Block.CycleTime:#,##0} pcs";
            }

            control.StatusIndicator.Background = (Brush)Application.Current.Resources[colour];

            List<string> errs = control.Block.GetListOfErrors();
            if (errs.Count == 0)
            {
                control.ErrorsList.Visibility = Visibility.Collapsed;
            }
            else
            {
                control.ErrorsList.ItemsSource = errs;
            }


    }

    public DisplayMachineOperatingBlock()
        {
            InitializeComponent();
        }
    }
}
