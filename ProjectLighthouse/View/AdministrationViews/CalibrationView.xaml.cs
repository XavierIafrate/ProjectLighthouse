using ProjectLighthouse.ViewModel.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View
{
    public partial class CalibrationView : UserControl
    {
        public CalibrationView()
        {
            InitializeComponent();
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
        }

        private void NumbersOnly(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }
    }
}
