using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string content = File.ReadAllText(@"\\groupfile01\Sales\Production\Programs\Citizen\Part Programs\12.PRG");
            //string content = "test";

            await ExecuteScriptFunctionAsync(webView21, "setContent", content.ToString());
        }

        public static async Task<string> ExecuteScriptFunctionAsync(WebView2 webView2, string functionName, params object[] parameters)
        {
            string script = functionName + "(";
            for (int i = 0; i < parameters.Length; i++)
            {
                script += JsonConvert.SerializeObject(parameters[i]);
                if (i < parameters.Length - 1)
                {
                    script += ", ";
                }
            }
            script += ");";
            return await webView2.ExecuteScriptAsync(script);
        }
    }
}
