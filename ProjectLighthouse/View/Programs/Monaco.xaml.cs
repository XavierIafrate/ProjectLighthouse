using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.ViewModel.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Path = System.IO.Path;

namespace ProjectLighthouse.View.Programs
{
    public partial class Monaco : Window, INotifyPropertyChanged
    {
        List<string> themes;

        private ObservableCollection<NcProgram> programs = new();
        public ObservableCollection<NcProgram> Programs
        {
            get { return programs; }
            set
            {
                programs = value;
                OnPropertyChanged();
            }
        }

        private NcProgram selectedProgram;
        public NcProgram SelectedProgram
        {
            get { return selectedProgram; }
            set
            {
                selectedProgram = value;
                SetContent();
                OnPropertyChanged();
            }
        }

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

        public Monaco()
        {
            InitializeComponent();
            DataContext = this;
        }

        async Task SetupEditors()
        {
            this.DollarOne.Source =
                   new Uri(Path.Combine(
                       AppDomain.CurrentDomain.BaseDirectory,
                       @"Monaco\index.html"));
            this.DollarTwo.Source =
                   new Uri(Path.Combine(
                       AppDomain.CurrentDomain.BaseDirectory,
                       @"Monaco\index.html"));

            await DollarOne.EnsureCoreWebView2Async();
            await DollarTwo.EnsureCoreWebView2Async();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

            await SetupEditors();

            ThemeName.ItemsSource = fileNames;
            ThemeName.SelectedIndex = fileNames.IndexOf("Oceanic Next");
        }

        async void SetContent()
        {
            if (SelectedProgram is null) return;
            await SetDollarOneContent();
            await SetDollarTwoContent();
        }

        private void DollarOne_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            OnDollarOneNavigationCompleted();
        }

        private void DollarTwo_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            OnDollarTwoNavigationCompleted();
        }

        async Task SetDollarOneContent()
        {
            if (DollarOne.Source.LocalPath.Contains("-diff"))
            {
                await LumenManager.ExecuteScriptFunctionAsync(DollarOne, "setContent", SelectedProgram.ProgramContent.OriginalDollarOneCode, SelectedProgram.ProgramContent.DollarOneCode);
            }
            else
            {
                await LumenManager.ExecuteScriptFunctionAsync(DollarOne, "setContent", SelectedProgram.ProgramContent.DollarOneCode);
            }
        }

        async Task SetDollarTwoContent()
        {
            if (DollarTwo.Source.LocalPath.Contains("-diff"))
            {
                await LumenManager.ExecuteScriptFunctionAsync(DollarTwo, "setContent", SelectedProgram.ProgramContent.OriginalDollarTwoCode, SelectedProgram.ProgramContent.DollarTwoCode);
            }
            else
            {
                await LumenManager.ExecuteScriptFunctionAsync(DollarTwo, "setContent", SelectedProgram.ProgramContent.DollarTwoCode);
            }
        }

        async void OnDollarOneNavigationCompleted()
        {
            if (SelectedProgram is null)
            {
                if (Programs.Count > 0)
                {
                    SelectedProgram = Programs.First();
                }
                else
                {
                    return;
                }
            }
            
            DollarOne.Visibility = Visibility.Hidden;
            await SetDollarOneContent();
            ApplyTheme();

            DollarOne.Visibility = Visibility.Visible;
        }

        async void OnDollarTwoNavigationCompleted()
        {
            if (SelectedProgram is null)
            {
                if (Programs.Count > 0)
                {
                    SelectedProgram = Programs.First();
                }
                else
                {
                    return;
                }
            }
            
            DollarTwo.Visibility = Visibility.Hidden;
            await SetDollarTwoContent();
            ApplyTheme();

            DollarTwo.Visibility = Visibility.Visible;
        }



        private void ThemeName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyTheme();
        }

        void ApplyTheme()
        {
            string themeData;

            try
            {
                themeData = LoadThemeData((string)ThemeName.SelectedValue);
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
                return;
            }

            SetLumenTheme(themeData);
            SetTheme(DollarOne, themeData);
            SetTheme(DollarTwo, themeData);
        }

        private async void DiffButton_Click(object sender, RoutedEventArgs e)
        {
            if (DollarOne.Source.LocalPath.Contains("-diff"))
            {
                this.DollarOne.Source =
                   new Uri(Path.Combine(
                       AppDomain.CurrentDomain.BaseDirectory,
                       @"Monaco\index.html"));
                this.DollarOneDiffModeText.Visibility = Visibility.Collapsed;

                await DollarOne.EnsureCoreWebView2Async();
            }
            else
            {
                string jsFunc = "editor.getValue()";
                string content = await DollarOne.CoreWebView2.ExecuteScriptAsync(jsFunc);
                string textContent = Regex.Unescape(content.ToString());
                textContent = textContent[1..^1];
                SelectedProgram.ProgramContent.DollarOneCode = textContent;
                SetToDiffMode(DollarOne);

                this.DollarOneDiffModeText.Visibility = Visibility.Visible;
            }


            if (DollarTwo.Source.LocalPath.Contains("-diff"))
            {
                this.DollarTwo.Source =
                   new Uri(Path.Combine(
                       AppDomain.CurrentDomain.BaseDirectory,
                       @"Monaco\index.html"));
                this.DollarTwoDiffModeText.Visibility = Visibility.Collapsed;

                await DollarTwo.EnsureCoreWebView2Async();
            }
            else
            {
                string jsFunc = "editor.getValue()";
                string content = await DollarTwo.CoreWebView2.ExecuteScriptAsync(jsFunc);
                string textContent = Regex.Unescape(content.ToString());
                textContent = textContent[1..^1];
                SelectedProgram.ProgramContent.DollarTwoCode = textContent;
                SetToDiffMode(DollarTwo);

                this.DollarTwoDiffModeText.Visibility = Visibility.Visible;
            }
        }

        static async void SetToDiffMode(WebView2 webView)
        {
            webView.Source =
                       new Uri(Path.Combine(
                           AppDomain.CurrentDomain.BaseDirectory,
                           @"Monaco\index-diff.html"));
            await webView.EnsureCoreWebView2Async();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            LumenManager.DisposeWindow();

        }

        #region Themes
        string LoadThemeData(string name)
        {
           string path = themes.Find(x => Path.GetFileNameWithoutExtension(x) == name) 
                ?? throw new ArgumentException($"Could not find theme: '{name}'");

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

        private static async void SetTheme(WebView2 window, string data)
        {
            dynamic d = JObject.Parse(data);
            await LumenManager.ExecuteScriptFunctionAsync(window, "setTheme", d);
        }
        #endregion

        private void CloseProgramButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.CommandParameter is not NcProgram prog) return;

            LumenManager.Close(prog);
        }
    }
}
