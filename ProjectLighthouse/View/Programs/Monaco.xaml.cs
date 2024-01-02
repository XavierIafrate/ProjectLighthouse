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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ProjectLighthouse.Model.Programs.NcProgram;
using Path = System.IO.Path;

namespace ProjectLighthouse.View.Programs
{
    public partial class Monaco : Window, INotifyPropertyChanged
    {

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

        private ObservableCollection<NcProgramCommit> selectedProgramCommits = new();
        public ObservableCollection<NcProgramCommit> SelectedProgramCommits
        {
            get { return selectedProgramCommits; }
            set
            {
                selectedProgramCommits = value;
                OnPropertyChanged();
            }
        }

        private NcProgramCommit? selectedCommit;
        public NcProgramCommit? SelectedCommit
        {
            get { return selectedCommit; }
            set
            {
                selectedCommit = value;
                LoadCommit();
                OnPropertyChanged();
            }
        }

        private bool diffMode;
        public bool DiffMode
        {
            get { return diffMode; }
            set
            {
                diffMode = value;
                ConfigureEditors();
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
                DiffMode = false;

                if (value is not null)
                {
                    NewCommit = new() { ProgramId = SelectedProgram.Id };

                    SelectedProgramCommits.Clear();
                    LumenManager.Commits
                        .Where(x => x.ProgramId == SelectedProgram.Id)
                        .ToList()
                        .ForEach(x => SelectedProgramCommits.Add(x));

                    OnPropertyChanged(nameof(SelectedProgramCommits));

                    if (SelectedProgramCommits.Count == 0)
                    {
                        NewCommit.CommitMessage = "Initial commit";
                    }

                    DisplayedContent = value.ProgramContent;

                }
                OnPropertyChanged();
            }
        }

        private NcProgramCommit newCommit;
        public NcProgramCommit NewCommit
        {
            get { return newCommit; }
            set
            {
                newCommit = value;
                OnPropertyChanged();
            }
        }

        private MonacoProgram displayedContent;
        public MonacoProgram DisplayedContent
        {
            get { return displayedContent; }
            set
            {
                displayedContent = value;
                SetContent();
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
            await SetupEditors();

            ThemeName.ItemsSource = LumenManager.ThemeNames;
            ThemeName.SelectedIndex = LumenManager.ThemeNames.IndexOf(LumenManager.SelectedThemeName);
        }

        void LoadCommit()
        {
            MonacoProgram p;

            if (SelectedCommit is null)
            {
                List<NcProgramCommit> priorCommits = SelectedProgramCommits.ToList()
                    .OrderByDescending(x => x.CommittedAt)
                    .ToList();

                NcProgramCommit? compareAgainst = null;
                if (priorCommits.Count > 0)
                {
                    compareAgainst = priorCommits.First();
                }

                try
                {
                    p = LumenManager.LoadCommit(SelectedProgram.ProgramContent, compareAgainst);
                }
                catch (Exception ex)
                {
                    NotificationManager.NotifyHandledException(ex);
                    return;
                }
            }
            else
            {
                List<NcProgramCommit> priorCommits = SelectedProgramCommits.ToList()
                    .Where(x => x.CommittedAt < SelectedCommit.CommittedAt)
                    .OrderByDescending(x => x.CommittedAt)
                    .ToList();

                NcProgramCommit? compareAgainst = null;
                if (priorCommits.Count > 0)
                {
                    compareAgainst = priorCommits.First();
                }

                try
                {
                    p = LumenManager.LoadCommit(SelectedCommit, compareAgainst);
                }
                catch (Exception ex)
                {
                    NotificationManager.NotifyHandledException(ex);
                    return;
                }
            }


            DisplayedContent = p;
        }

        async void SetContent()
        {
            if (DisplayedContent is null)
            {
                return;
            }


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
            if (DiffMode)
            {
                await LumenManager.ExecuteScriptFunctionAsync(DollarOne, "setContent", DisplayedContent.OriginalDollarOneCode, DisplayedContent.DollarOneCode);
            }
            else
            {
                await LumenManager.ExecuteScriptFunctionAsync(DollarOne, "setContent", DisplayedContent.DollarOneCode);
            }
        }

        async Task SetDollarTwoContent()
        {
            if (DiffMode)
            {
                await LumenManager.ExecuteScriptFunctionAsync(DollarTwo, "setContent", DisplayedContent.OriginalDollarTwoCode, DisplayedContent.DollarTwoCode);
            }
            else
            {
                await LumenManager.ExecuteScriptFunctionAsync(DollarTwo, "setContent", DisplayedContent.DollarTwoCode);
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
            ApplyTheme(LumenManager.SelectedThemeData);

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
            ApplyTheme(LumenManager.SelectedThemeData);

            DollarTwo.Visibility = Visibility.Visible;
        }

        private void ThemeName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LumenManager.SetTheme((string)ThemeName.SelectedValue);
        }

        public void ApplyTheme(string themeData)
        {
            SetLumenTheme(themeData);
            SetTheme(DollarOne, themeData);
            SetTheme(DollarTwo, themeData);
        }

        private async void ConfigureEditors()
        {
            CommitMenu.Visibility = DiffMode
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (DiffMode)
            {
                SetToDiffMode(DollarOne);
                SetToDiffMode(DollarTwo);

                SelectedCommit = null;

                this.DollarOneDiffModeText.Visibility = Visibility.Visible;
                this.DollarTwoDiffModeText.Visibility = Visibility.Visible;

                return;
            }

            this.DollarOne.Source =
                new Uri(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"Monaco\index.html"));
            this.DollarTwo.Source =
                new Uri(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"Monaco\index.html"));

            if (SelectedProgram is not null)
            {
                DisplayedContent = SelectedProgram.ProgramContent;
            }

            this.DollarOneDiffModeText.Visibility = Visibility.Collapsed;
            this.DollarTwoDiffModeText.Visibility = Visibility.Collapsed;

            await DollarOne.EnsureCoreWebView2Async();
            await DollarTwo.EnsureCoreWebView2Async();
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
            LumenManager.DisposeWindow(this);
        }

        #region Themes

        void SetLumenTheme(string themeData)
        {
            if (themeData == "vs" || themeData == "hc-light")
            {
                ForegroundBrush = (SolidColorBrush)App.Current.Resources["OnBackground"];
                BackgroundBrush = (SolidColorBrush)App.Current.Resources["Background"];
                CursorForegroundBrush = (SolidColorBrush)App.Current.Resources["Purple"];
                return;
            }
            else if (themeData == "vs-dark")
            {
                ForegroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffffff");
                BackgroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#1e1e1e");
                CursorForegroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#007acc");
                return;
            }
            else if (themeData == "hc-black")
            {
                ForegroundBrush = (SolidColorBrush)Brushes.White;
                BackgroundBrush = (SolidColorBrush)Brushes.Black;
                CursorForegroundBrush = (SolidColorBrush)App.Current.Resources["PurpleLight"];
                return;
            }

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
            dynamic d;
            if (LumenManager.builtInThemes.ContainsValue(data))
            {
                d = data;
            }
            else
            {
                d = JObject.Parse(data);
            }

            await LumenManager.ExecuteScriptFunctionAsync(window, "setTheme", d);
        }
        #endregion

        private void CloseProgramButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.CommandParameter is not NcProgram prog) return;

            LumenManager.Close(prog);
        }

        private void PopoutProgramButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            LumenManager.OpenInSingleWindow(button.CommandParameter as NcProgram);
        }

        public void HideMenu()
        {
            ProgramsList.Visibility = Visibility.Collapsed;
            ThemesComboBox.Visibility = Visibility.Collapsed;
        }

        public void ShowMenu()
        {
            ProgramsList.Visibility = Visibility.Visible;
            ThemesComboBox.Visibility = Visibility.Visible;
        }

        private void CommitButton_Click(object sender, RoutedEventArgs e)
        {
            NewCommit.ValidateAll();

            if (NewCommit.HasErrors)
            {
                MessageBox.Show("Commit has errors");
                return;
            }

            try
            {
                LumenManager.PostCommit(DisplayedContent, NewCommit);
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
            }

            SelectedProgramCommits.Add(NewCommit);
        }
    }
}
