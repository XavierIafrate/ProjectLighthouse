using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for BarStockView.xaml
    /// </summary>
    public partial class BarStockView : UserControl
    {
        public BarStockView()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            searchBox.Clear();
        }
    }
}
