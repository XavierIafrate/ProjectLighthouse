using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.View.Scheduling;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayResearch : UserControl
    {
        public ResearchTime Research
        {
            get { return (ResearchTime)GetValue(ResearchProperty); }
            set { SetValue(ResearchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Research.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResearchProperty =
            DependencyProperty.Register("Research", typeof(ResearchTime), typeof(DisplayResearch), new PropertyMetadata(null, SetValues));

        public DisplayResearch()
        {
            InitializeComponent();
        }

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayResearch control)
            {
                return;
            }
            control.Edit_Button.Visibility = App.CurrentUser.Role >= UserRole.Scheduling
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            CreateResearch editWindow = new(Research, null);
            editWindow.ShowDialog();

            if (editWindow.Saved)
            {
                Research.NotifyEditMade();
            }
        }
    }
}
