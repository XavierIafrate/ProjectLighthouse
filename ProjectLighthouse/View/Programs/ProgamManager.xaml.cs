using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            //if (sender is not TextBox textBox) return;
            //SendButton.IsEnabled = !string.IsNullOrWhiteSpace(textBox.Text);
        }



        List<string> themes;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.dollarOne.Source =
                    new Uri(Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        @"Monaco\index.html"));

            this.dollarTwo.Source =
                    new Uri(Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        @"Monaco\index.html"));


            themes = Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, @"Monaco\themes")).ToList();

            List<string> fileNames = new();

            for (int i = 0; i < themes.Count; i++)
            {
                string f = Path.GetFileNameWithoutExtension(themes[i]);
                if (f == "themelist")
                {
                    continue;
                }
                fileNames.Add(f);
            }

            dollarOne.CoreWebView2InitializationCompleted += DollarOne_CoreWebView2InitializationCompleted;
            dollarTwo.CoreWebView2InitializationCompleted += DollarTwo_CoreWebView2InitializationCompleted;
        }

        private void DollarTwo_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            SetTheme(dollarTwo, "Cobalt2");

        }

        private void DollarOne_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            SetTheme(dollarOne, "Cobalt2");

        }
        private async void SetTheme(WebView2 window, string name)
        {
            string path = themes.Find(x => x.Contains(name));
            if (path is null)
            {
                MessageBox.Show($"Could not find theme: '{name}'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string data = File.ReadAllText(path);
            dynamic d = JObject.Parse(data);
            await ExecuteScriptFunctionAsync(window, "setTheme", d);
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
