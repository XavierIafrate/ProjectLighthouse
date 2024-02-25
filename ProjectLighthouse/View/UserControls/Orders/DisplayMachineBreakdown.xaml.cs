using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.ValueConverters;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayMachineBreakdown : UserControl
    {
        public MachineBreakdown Breakdown
        {
            get { return (MachineBreakdown)GetValue(BreakdownProperty); }
            set { SetValue(BreakdownProperty, value); }
        }

        public static readonly DependencyProperty BreakdownProperty =
            DependencyProperty.Register("Breakdown", typeof(MachineBreakdown), typeof(DisplayMachineBreakdown), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayMachineBreakdown control) return;
            control.BreakdownCodeText.Text = control.Breakdown.BreakdownCode;
            control.BreakdownStartedText.Text = control.Breakdown.BreakdownStarted.ToString("dd/MM HH:mm");
            control.BreakdownEndedText.Text = " To " + control.Breakdown.BreakdownEnded.ToString("dd/MM HH:mm");
            control.TimeElapsedText.Text = new intToTimespanString().Convert(control.Breakdown.TimeElapsed, null, null, null) as string;
            
            if (control.Breakdown.BreakdownMeta is not null)
            {
                control.MetaNameText.Text = control.Breakdown.BreakdownMeta.Name;
            }
        }

        public DisplayMachineBreakdown()
        {
            InitializeComponent();
        }
    }
}
