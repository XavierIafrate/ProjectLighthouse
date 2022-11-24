using ProjectLighthouse.Model.Material;
using ProjectLighthouse.ViewModel.Helpers;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.View.Administration
{
    public partial class NewBarWindow : Window
    {
        public BarStock NewBar { get; set; }

        public NewBarWindow()
        {
            InitializeComponent();
        }

        private void ValidateProductNumber(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressForProductName(e);
        }

        private void ValidateInt(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }
    }
}
