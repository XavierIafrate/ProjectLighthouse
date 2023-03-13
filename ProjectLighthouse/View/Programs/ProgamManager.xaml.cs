using System;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Programs
{
    public partial class ProgamManager : UserControl
    {
        public ProgamManager()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            searchBox.Clear();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;

            grad.Visibility = scrollViewer.VerticalOffset == 0
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;
            SendButton.IsEnabled = !string.IsNullOrWhiteSpace(textBox.Text);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //this.dollarOne.Source =
            //        new Uri(System.IO.Path.Combine(
            //            System.AppDomain.CurrentDomain.BaseDirectory,
            //            @"Monaco\index.html"));

            //this.dollarTwo.Source =
            //        new Uri(System.IO.Path.Combine(
            //            System.AppDomain.CurrentDomain.BaseDirectory,
            //            @"Monaco\index.html"));
        }
    }
}
