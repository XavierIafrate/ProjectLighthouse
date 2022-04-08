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
using ProjectLighthouse.Model;

namespace ProjectLighthouse.View.AdministrationViews.AnalyticsComponents
{
    public partial class DisplayMachineOperatingBlock : UserControl
    {


        public MachineOperatingBlock Block
        {
            get { return (MachineOperatingBlock)GetValue(BlockProperty); }
            set { SetValue(BlockProperty, value); }
        }

        public static readonly DependencyProperty BlockProperty =
            DependencyProperty.Register("Block", typeof(MachineOperatingBlock), typeof(DisplayMachineOperatingBlock), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayMachineOperatingBlock control)
            {
                return;
            }
            control.DataContext = control.Block;

            if (control.Block.State == "Running")
            {
                control.BackgroundBorder.BorderBrush = (Brush)Application.Current.Resources["Green"];
                control.StateText.Foreground = (Brush)Application.Current.Resources["Green"];
            }
            else if (control.Block.State == "Setting")
            {
                control.BackgroundBorder.BorderBrush = (Brush)Application.Current.Resources["Blue"];
                control.StateText.Foreground = (Brush)Application.Current.Resources["Blue"];
            }
            else
            {
                control.BackgroundBorder.BorderBrush = (Brush)Application.Current.Resources["Red"];
                control.StateText.Foreground = (Brush)Application.Current.Resources["Red"];
            }

            control.QuantityProjectionText.Text = $"{control.Block.GetCalculatedPartsProduced():#,##0} pcs projected";

            control.QuantityProjections.Visibility = control.Block.State == "Running" ? Visibility.Visible : Visibility.Hidden;
            control.runningIcon.Visibility = control.Block.State == "Running" ? Visibility.Visible : Visibility.Collapsed;
            control.settingIcon.Visibility = control.Block.State == "Setting" ? Visibility.Visible : Visibility.Collapsed;
            control.errorIcon.Visibility = (control.runningIcon.Visibility == Visibility.Collapsed && control.settingIcon.Visibility == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
        }

        public DisplayMachineOperatingBlock()
        {
            InitializeComponent();
        }
    }
}
