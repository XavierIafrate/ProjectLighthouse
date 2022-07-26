using ProjectLighthouse.Model;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayAttachment : UserControl
    {
        public Attachment Attachment
        {
            get { return (Attachment)GetValue(AttachmentProperty); }
            set { SetValue(AttachmentProperty, value); }
        }

        public static readonly DependencyProperty AttachmentProperty =
            DependencyProperty.Register("Attachment", typeof(Attachment), typeof(DisplayAttachment), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayAttachment control) return;
            control.FileName.Text = control.Attachment.FileName;
        }

        public DisplayAttachment()
        {
            InitializeComponent();
        }


        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            string tmpPath = Path.Join(Path.GetTempPath(), Attachment.FileName + Attachment.Extension);
            if (!File.Exists(tmpPath))
            {
                File.Copy(Path.Join(App.ROOT_PATH, Attachment.AttachmentStore), tmpPath);
            }
            fileopener.StartInfo.Arguments = "\"" + tmpPath + "\"";
            _ = fileopener.Start();
        }
    }
}
