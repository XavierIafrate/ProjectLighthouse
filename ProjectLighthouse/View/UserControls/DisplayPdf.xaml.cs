using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayPdf : UserControl
    {
        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(DisplayPdf), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DisplayPdf control)
            {
                if (string.IsNullOrEmpty(control.FilePath))
                {
                    control.filename.Text = "<null>";
                    control.openButton.IsEnabled = false;
                    return;
                }

                string filePath = Path.Join(App.ROOT_PATH, control.FilePath);

                if (!File.Exists(filePath))
                {
                    
                    control.filename.Text = Path.GetFileName(filePath) + " (not found)";
                    control.openButton.IsEnabled = false;
                }
                else
                {
                    control.filename.Text = Path.GetFileName(filePath);
                    control.openButton.IsEnabled = true;
                }
            }
        }

        public DisplayPdf()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + Path.Join(App.ROOT_PATH, FilePath) + "\"";
            _ = fileopener.Start();
        }
    }
}
