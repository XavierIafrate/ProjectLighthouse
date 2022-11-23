using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace View.Research
{
    /// <summary>
    /// Interaction logic for ResearchView.xaml
    /// </summary>
    public partial class ResearchView : UserControl
    {
        public ResearchView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            grad.Visibility = scrollViewer.VerticalOffset == 0
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;
            SendButton.IsEnabled = !string.IsNullOrWhiteSpace(textBox.Text);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            searchBox.Text = "";
        }

    }
}
