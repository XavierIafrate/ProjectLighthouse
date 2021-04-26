using ProjectLighthouse.ViewModel;
using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for OrderView.xaml
    /// </summary>
    public partial class OrderView : UserControl
    {
        OrderViewModel viewModel;
        public OrderView()
        {
            InitializeComponent();
            viewModel = Resources["vm"] as OrderViewModel;
        }

    }
}
