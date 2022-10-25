using System;
using System.Diagnostics;
using System.Windows;

namespace ProjectLighthouse.View
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void githubButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo() { FileName = "https://github.com/XavierIafrate/ProjectLighthouse", UseShellExecute = true });
        }
    }
}
