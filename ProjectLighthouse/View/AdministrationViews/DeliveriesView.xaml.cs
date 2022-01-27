using ProjectLighthouse.ViewModel;
using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for DeliveriesView.xaml
    /// </summary>
    public partial class DeliveriesView : UserControl
    {
        DeliveriesViewModel viewModel;
        public DeliveriesView()
        {
            InitializeComponent();
            viewModel = Resources["vm"] as DeliveriesViewModel;
            DataContext = viewModel;
        }
    }
}
