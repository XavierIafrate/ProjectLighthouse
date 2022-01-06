using ProjectLighthouse.Model;
using ProjectLighthouse.View.ScheduleViews;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayMachineService : UserControl
    {
        public MachineService Service
        {
            get { return (MachineService)GetValue(ServiceProperty); }
            set { SetValue(ServiceProperty, value); }
        }

        public static readonly DependencyProperty ServiceProperty =
            DependencyProperty.Register("Service", typeof(MachineService), typeof(DisplayMachineService), new PropertyMetadata(null, SetValues));

        public DisplayMachineService()
        {
            InitializeComponent();
        }

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayMachineService control)
            {
                return;
            }

            control.DataContext = control.Service;
            control.Edit_Button.Visibility = App.CurrentUser.UserRole is "admin" or "Scheduling"
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            CreateService editWindow = new(Service, null);
            editWindow.ShowDialog();

            if (editWindow.Saved)
            {
                Service.NotifyEditMade();
            }
        }
    }
}
