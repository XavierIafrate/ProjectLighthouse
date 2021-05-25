using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for ScheduleView.xaml
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        ScheduleViewModel viewModel;

        public ScheduleView()
        {
            InitializeComponent();
            viewModel = Resources["vm"] as ScheduleViewModel;
        }

        private void DisplayLMOScheduling_MouseClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right && (App.currentUser.UserRole == "Scheduling" || App.currentUser.UserRole == "admin"))
            {
                DisplayLMOScheduling clickedItem = sender as DisplayLMOScheduling;
                viewModel.updateItem(clickedItem.orderObject.Order);
            }
        }
    }
}
