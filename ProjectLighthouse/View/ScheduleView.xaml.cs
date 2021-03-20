using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel;
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
                viewModel.updateItem(clickedItem.order);
            }

        }
        private void DisplayLMOScheduling_MouseClick_awaiting(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right && (App.currentUser.UserRole == "Scheduling" || App.currentUser.UserRole == "admin"))
            {
                DisplayAwaitingScheduling clickedItem = sender as DisplayAwaitingScheduling;
                viewModel.updateItem(clickedItem.order);
            }

        }
    }
}
