using ProjectLighthouse.Model.Drawings;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayDrawingGroup : UserControl
    {
        public TechnicalDrawingGroup Group
        {
            get { return (TechnicalDrawingGroup)GetValue(GroupProperty); }
            set { SetValue(GroupProperty, value); }
        }

        public static readonly DependencyProperty GroupProperty =
            DependencyProperty.Register("Group", typeof(TechnicalDrawingGroup), typeof(DisplayDrawingGroup), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayDrawingGroup control)
            {
                return;
            }

            if (control.Group == null)
            {
                return;
            }

            control.ActionRequiredBadge.Visibility = control.Group.Drawings.Any(x => x.PendingApproval())
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.WithdrawnBadge.Visibility = control.Group.AllDrawingsWithdrawn
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.ArchetypeBadge.Visibility = control.Group.IsArchetypeGroup
                ? Visibility.Visible
                : Visibility.Collapsed;

            control.HasCheckSheetBadge.Visibility = control.Group.HasCheckSheet
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public DisplayDrawingGroup()
        {
            InitializeComponent();
        }
    }
}
