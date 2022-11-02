using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.dollarOne.Source =
                    new Uri(System.IO.Path.Combine(
                        System.AppDomain.CurrentDomain.BaseDirectory,
                        @"Monaco\index.html"));

            this.dollarTwo.Source =
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
            //string content = File.ReadAllText(@"\\groupfile01\Sales\Production\Programs\Citizen\Part Programs\12.PRG");
            NcProgram prog = GetProgramFromFile(@"C:\Users\xavie\Downloads\127.PRG");
            //string content = "test";

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
                header = programData.Substring(0, programData.IndexOf("$1")).Trim(),
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
    }

    internal class NcProgram
    {
        public string header { get; set; }
        public string dollarOneCode { get; set; }
        public string dollarTwoCode { get; set; }
        public string dollarZeroCode { get; set; }
    }
}
