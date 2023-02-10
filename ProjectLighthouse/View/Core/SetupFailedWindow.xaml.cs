using System.Windows;

namespace ProjectLighthouse.View.Core
{
    public partial class SetupFailedWindow : Window
    {
        public SetupFailedWindow(string message, string submessage)
        {
            InitializeComponent();
            ErrorMessage.Text = message;
            SubErrorMessage.Text = submessage;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
