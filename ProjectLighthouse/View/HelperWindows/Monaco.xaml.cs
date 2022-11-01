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
using System.Windows.Shapes;

namespace View.HelperWindows
{
    /// <summary>
    /// Interaction logic for Monaco.xaml
    /// </summary>
    public partial class Monaco : Window
    {
        public Monaco()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.webView21.Source =
                    new Uri(System.IO.Path.Combine(
                        System.AppDomain.CurrentDomain.BaseDirectory,
                        @"Monaco\index.html"));
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label _lbl = sender as Label;
            DragDrop.DoDragDrop(_lbl, _lbl.Content, DragDropEffects.Move);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.webView21.CoreWebView2.ExecuteScriptAsync("test()");
        }
    }
}
