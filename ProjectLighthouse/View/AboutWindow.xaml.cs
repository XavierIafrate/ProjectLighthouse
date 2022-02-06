using System;
using System.Windows;

namespace ProjectLighthouse.View
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            CopyRightText.Text = $"Copyright Wixroyd Group {DateTime.Now:yyyy}";
        }
    }
}
