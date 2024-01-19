using System;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.View.Core
{
    public partial class HelpWindow : Window
    {
        string TargetSource;
        public HelpWindow(string? url)
        {
            InitializeComponent();
            TargetSource = url ?? $"{App.ROOT_PATH}docs\\index.html";

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await SetupBrowser();
        }

        async Task SetupBrowser()
        {
            Browser.Source = new Uri(TargetSource);
            await Browser.EnsureCoreWebView2Async();
        }
    }
}
