

using ProjectLighthouse.Model;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayTechnicalDrawing : UserControl
    {
        public TechnicalDrawing Drawing
        {
            get { return (TechnicalDrawing)GetValue(DrawingProperty); }
            set { SetValue(DrawingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Drawing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawingProperty =
            DependencyProperty.Register("Drawing", typeof(TechnicalDrawing), typeof(DisplayTechnicalDrawing), new PropertyMetadata(null, SetValues));


        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DisplayTechnicalDrawing control)
            {
                if (control.Drawing == null)
                {
                    control.filename.Text = "<null>";
                    control.rev.Text = "n/a";
                    control.openButton.IsEnabled = false;
                    return;
                }

                string filePath = Path.Join(App.ROOT_PATH, control.Drawing.URL);
                
                control.filename.Text = control.Drawing.DrawingName;
                control.rev.Text = $"Revision {control.Drawing.Revision}";

                if (!File.Exists(filePath))
                {
                    control.openButton.Content = "file not found";
                    control.openButton.IsEnabled = false;
                }
                else
                {
                    control.openButton.Content = "Open";
                    control.openButton.IsEnabled = true;
                }
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + Path.Join(App.ROOT_PATH, Drawing.URL) + "\"";
            _ = fileopener.Start();
        }

        public DisplayTechnicalDrawing()
        {
            InitializeComponent();
        }
    }
}
