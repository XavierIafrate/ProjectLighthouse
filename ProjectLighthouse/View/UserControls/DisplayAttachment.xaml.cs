using ProjectLighthouse.Model.Core;
using System;
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
            string fullPath = Path.Join(App.ROOT_PATH, control.Attachment.AttachmentStore);
            if (File.Exists(fullPath))
            {
                control.FileSize.Text = SizeSuffix(new FileInfo(fullPath).Length);
            }
            else
            {
                control.OpenButton.IsEnabled = false;
                control.OpenButton.Content = "not found";
            }
        }

        public DisplayAttachment()
        {
            InitializeComponent();
        }


        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            string tmpPath = Path.Join(Path.GetTempPath(), $"{Attachment.FileName}_{Attachment.Id}" + Attachment.Extension);
            if (!File.Exists(tmpPath))
            {
                File.Copy(Path.Join(App.ROOT_PATH, Attachment.AttachmentStore), tmpPath);
            }
            fileopener.StartInfo.Arguments = "\"" + tmpPath + "\"";
            _ = fileopener.Start();
        }

        static readonly string[] SizeSuffixes =
                   { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} B", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}
