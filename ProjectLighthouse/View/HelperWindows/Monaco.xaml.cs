using DocumentFormat.OpenXml.Drawing;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Path = System.IO.Path;

namespace View.HelperWindows
{
    public partial class Monaco : Window
    {
        List<string> themes;

        public Monaco()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.dollarOne.Source =
                    new Uri(System.IO.Path.Combine(
                        System.AppDomain.CurrentDomain.BaseDirectory,
                        @"Monaco\index.html"));

            this.dollarTwo.Source =
                    new Uri(System.IO.Path.Combine(
                        System.AppDomain.CurrentDomain.BaseDirectory,
                        @"Monaco\index.html"));

            themes = Directory.GetFiles(Path.Join(System.AppDomain.CurrentDomain.BaseDirectory, @"Monaco\themes")).ToList();

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

            themeName.ItemsSource = fileNames;
            themeName.Text = "Cobalt2";

            dollarOne.CoreWebView2InitializationCompleted += DollarOne_CoreWebView2InitializationCompleted;
            dollarTwo.CoreWebView2InitializationCompleted += DollarTwo_CoreWebView2InitializationCompleted;
        } 

        private void DollarOne_Loaded(object sender, RoutedEventArgs e)
        {
            dollarOne.Visibility = Visibility.Visible;
            SetTheme(dollarOne, "Cobalt2");
        }

        private void DollarTwo_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            dollarTwo.Visibility = Visibility.Visible;
            SetTheme(dollarTwo, "Cobalt2");
        }

        private void DollarOne_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            SetTheme(dollarOne, "Cobalt2");
            dollarOne.Visibility = Visibility.Visible;
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label _lbl = sender as Label;
            DragDrop.DoDragDrop(_lbl, _lbl.Content, DragDropEffects.Move);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //NcProgram prog = GetProgramFromFile(@"C:\Users\xavie\Downloads\127.PRG");
            NcProgram prog = GetProgramFromFile(@"\\groupfile01\Sales\Production\Programs\Citizen\Part Programs\12.PRG");

            await ExecuteScriptFunctionAsync(dollarOne, "setContent", prog.dollarOneCode);
            await ExecuteScriptFunctionAsync(dollarTwo, "setContent", prog.dollarTwoCode);
        }

        private static NcProgram GetProgramFromFile(string path)
        {
            string programData = File.ReadAllText(path);
            if (!programData.Contains("$0") || !programData.Contains("$1") || !programData.Contains("$2"))
            {
                throw new InvalidDataException("spindle missing");
            }

            NcProgram prog = new()
            {
                header = programData[..programData.IndexOf("$1")].Trim(),
                dollarOneCode = programData.Substring(programData.IndexOf("$1") + 2, programData.IndexOf("$2") - programData.IndexOf("$1") - 2).Trim(),
                dollarTwoCode = programData.Substring(programData.IndexOf("$2") + 2, programData.IndexOf("$0") - programData.IndexOf("$2") - 2).Trim(),
                dollarZeroCode = programData.Substring(programData.IndexOf("$0") + 2, programData.Length - programData.IndexOf("$0") - 2).Trim()
            };

            return prog;
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

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await ExecuteScriptFunctionAsync(dollarOne, "pushSnippet", "test_snippet", "this is a programmatic snippet", "inserted ${1:test}");
            await ExecuteScriptFunctionAsync(dollarTwo, "pushSnippet", "test_snippet", "this is a programmatic snippet", "inserted ${1:test}");
        }

        private void SetThemeButton_Click(object sender, RoutedEventArgs e)
        {
            SetTheme(dollarOne, themeName.Text);
            SetTheme(dollarTwo, themeName.Text);
        }

        private async void SetTheme(WebView2 window, string name)
        {
            string path = themes.Find(x => x.Contains(name));

            string data = File.ReadAllText(path);
            dynamic d = JObject.Parse(data);
            await ExecuteScriptFunctionAsync(window, "setTheme", d);
        }

    }
    internal class NcProgram
    {
        public string header { get; set; }
        public string dollarOneCode { get; set; }
        public string dollarTwoCode { get; set; }
        public string dollarZeroCode { get; set; }
    }
}
