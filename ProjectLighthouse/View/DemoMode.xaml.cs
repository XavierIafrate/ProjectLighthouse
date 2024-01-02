using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;

namespace ProjectLighthouse
{
    public partial class DemoMode : Window
    {
        public bool failed = true;
        public string rootDirectory;

        public DemoMode()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (App.ValidateRootDirectory(dialog.FileName + "\\", demo: true))
                {
                    failed = false;
                    rootDirectory = dialog.FileName + "\\";
                    Close();
                }
                else
                {
                    MessageBox.Show("The selected directory failed validation checks.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
