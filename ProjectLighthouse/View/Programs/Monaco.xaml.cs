using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Path = System.IO.Path;

namespace ProjectLighthouse.View.Programs
{
    public partial class Monaco : Window, INotifyPropertyChanged
    {
        List<string> themes;
        Uri path;
        MonacoProgram Program;


        private SolidColorBrush backgroundBrush;
        public SolidColorBrush BackgroundBrush
        {
            get { return backgroundBrush; }
            set
            {
                backgroundBrush = value;
                OnPropertyChanged();
            }
        }

        private SolidColorBrush foregroundBrush;
        public SolidColorBrush ForegroundBrush
        {
            get { return foregroundBrush; }
            set
            {
                foregroundBrush = value;
                OnPropertyChanged();
            }
        }

        private SolidColorBrush cursorForegroundBrush;
        public SolidColorBrush CursorForegroundBrush
        {
            get { return cursorForegroundBrush; }
            set
            {
                cursorForegroundBrush = value;
                OnPropertyChanged();
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Monaco(Uri path)
        {
            InitializeComponent();
            //this.path = path;

            this.path = new Uri(Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        @"View\Programs\12.PRG"));

            Program = GetProgramFromFile(this.path.LocalPath);

            DataContext = this;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DollarOne.Source =
                    new Uri(Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        @"Monaco\index.html"));

            this.DollarTwo.Source =
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

            await DollarOne.EnsureCoreWebView2Async();
            await DollarTwo.EnsureCoreWebView2Async();
            ItemsSource = fileNames;
            ThemeName.SelectedIndex = fileNames.IndexOf("idleFingers");
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label _lbl = sender as Label;
            DragDrop.DoDragDrop(_lbl, _lbl.Content, DragDropEffects.Move);
        }

        private static MonacoProgram GetProgramFromFile(string path)
        {
            string programData = File.ReadAllText(path);
            if (!programData.Contains("$0") || !programData.Contains("$1") || !programData.Contains("$2"))
            {
                throw new InvalidDataException("spindle missing");
            }

            MonacoProgram prog = new()
            {
                Header = programData[..programData.IndexOf("$1")].Trim(),
                DollarOneCode = programData.Substring(programData.IndexOf("$1") + 2, programData.IndexOf("$2") - programData.IndexOf("$1") - 2).Trim(),
                DollarTwoCode = programData.Substring(programData.IndexOf("$2") + 2, programData.IndexOf("$0") - programData.IndexOf("$2") - 2).Trim(),
                DollarZeroCode = programData.Substring(programData.IndexOf("$0") + 2, programData.Length - programData.IndexOf("$0") - 2).Trim()
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
            await ExecuteScriptFunctionAsync(DollarOne, "pushSnippet", "test_snippet", "this is a programmatic snippet", "inserted ${1:test}");
            await ExecuteScriptFunctionAsync(DollarTwo, "pushSnippet", "test_snippet", "this is a programmatic snippet", "inserted ${1:test}");
        }

        private void SetThemeButton_Click(object sender, RoutedEventArgs e)
        {
            string themeData = LoadThemeData(ThemeName.Text);
            SetLumenTheme(themeData);
            SetTheme(DollarOne, themeData);
            SetTheme(DollarTwo, themeData);
        }

        string LoadThemeData(string name)
        {
            string path = themes.Find(x => x.Contains(name));
            string data = File.ReadAllText(path);
            return data;
        }

        void SetLumenTheme(string themeData)
        {
            string? targeting = null;

            string? foreground = null;
            string? background = null;
            string? cursorForeground = null;

            JsonTextReader reader = new(new StringReader(themeData));
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    if (targeting is null)
                    {
                        // Nothing
                    }
                    else if (targeting == "editor.background")
                    {
                        background = reader.Value.ToString();
                    }
                    else if (targeting == "editor.foreground")
                    {
                        foreground = reader.Value.ToString();
                    }
                    else if (targeting == "editorCursor.foreground")
                    {
                        cursorForeground = reader.Value.ToString();
                    }

                    if (reader.Value.ToString() == "editor.background")
                    {
                        targeting = "editor.background";
                    }
                    else if (reader.Value.ToString() == "editor.foreground")
                    {
                        targeting = "editor.foreground";
                    }
                    else if (reader.Value.ToString() == "editorCursor.foreground")
                    {
                        targeting = "editorCursor.foreground";
                    }
                    else
                    {
                        targeting = null;
                    }
                }
            }

            if (foreground is null || background is null || cursorForeground is null)
            {
                throw new Exception("Invalid theme");
            }

            ForegroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(foreground);
            BackgroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(background);
            CursorForegroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(cursorForeground);

        }

        private class ThemeData
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private async void SetTheme(WebView2 window, string data)
        {
            dynamic d = JObject.Parse(data);
            await ExecuteScriptFunctionAsync(window, "setTheme", d);
        }

        private async void DollarOne_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            await ExecuteScriptFunctionAsync(DollarOne, "setContent", Program.DollarOneCode);
            string themeData = LoadThemeData(ThemeName.Text);

            SetTheme(DollarOne, themeData);
            DollarOne.Visibility = Visibility.Visible;
        }


        private async void DollarTwo_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            await ExecuteScriptFunctionAsync(DollarTwo, "setContent", Program.DollarTwoCode);
            string themeData = LoadThemeData(ThemeName.Text);

            SetTheme(DollarTwo, themeData);
            DollarTwo.Visibility = Visibility.Visible;
        }

        private void ThemeName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string themeData = LoadThemeData((string)ThemeName.SelectedValue);
            SetLumenTheme(themeData);
            SetTheme(DollarOne, themeData);
            SetTheme(DollarTwo, themeData);
        }
    }
    internal class MonacoProgram
    {
        public string Header { get; set; }
        public string DollarOneCode { get; set; }
        public string DollarTwoCode { get; set; }
        public string DollarZeroCode { get; set; }
    }
}
