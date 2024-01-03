using ProjectLighthouse.Model.Drawings;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayDrawingTimelineItem.xaml
    /// </summary>
    public partial class DisplayDrawingTimelineItem : UserControl
    {


        public TechnicalDrawing Drawing
        {
            get { return (TechnicalDrawing)GetValue(DrawingProperty); }
            set { SetValue(DrawingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Drawing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawingProperty =
            DependencyProperty.Register("Drawing", typeof(TechnicalDrawing), typeof(DisplayDrawingTimelineItem), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayDrawingTimelineItem control)
            {
                return;
            }
            string indicatorColour;

            if (!control.Drawing.IsApproved && !control.Drawing.IsRejected)
            {
                control.NameText.Text = "Release Candidate";
                indicatorColour = "Purple";
            }
            else if (control.Drawing.IsRejected)
            {
                control.NameText.Text = "Rejected Candidate";
                indicatorColour = "Red";
            }
            else
            {
                control.NameText.Text = $"Rev.{control.Drawing.Revision}{control.Drawing.AmendmentType}";
                indicatorColour = control.Drawing.IsCurrent ? "Green" : "OnBackground";
            }

            control.currentFlag.Visibility = control.Drawing.IsCurrent ? Visibility.Visible : Visibility.Collapsed;
            control.withdrawnFlag.Visibility = control.Drawing.IsWithdrawn ? Visibility.Visible : Visibility.Collapsed;
            control.researchFlag.Visibility = control.Drawing.DrawingType == TechnicalDrawing.Type.Research ? Visibility.Visible : Visibility.Collapsed;

            control.Indicator.Background = (Brush)Application.Current.Resources[indicatorColour];
        }

        public DisplayDrawingTimelineItem()
        {
            InitializeComponent();
        }
    }
}
