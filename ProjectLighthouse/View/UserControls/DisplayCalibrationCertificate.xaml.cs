using ProjectLighthouse.Model.Administration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayCalibrationCertificate : UserControl
    {
        public CalibrationCertificate Certificate
        {
            get { return (CalibrationCertificate)GetValue(CertificateProperty); }
            set { SetValue(CertificateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Certificate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CertificateProperty =
            DependencyProperty.Register("Certificate", typeof(CalibrationCertificate), typeof(DisplayCalibrationCertificate), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayCalibrationCertificate control)
            {
                return;
            }

            control.DataContext = control.Certificate;

            string filePath = Path.Join(App.ROOT_PATH, control.Certificate.Url);

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

            if (control.Certificate.IsPass)
            {
                control.fail.Visibility = Visibility.Collapsed;
                control.pass.Visibility = Visibility.Visible;
            }
            else
            {
                control.fail.Visibility = Visibility.Visible;
                control.pass.Visibility = Visibility.Collapsed;
            }

            if (control.Certificate.UKAS)
            {
                control.ukasBadge.Visibility = Visibility.Visible;
                control.ukasText.Visibility = Visibility.Visible;
            }
            else
            {
                control.ukasBadge.Visibility = Visibility.Collapsed;
                control.ukasText.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + Path.Join(App.ROOT_PATH, Certificate.Url) + "\"";
            _ = fileopener.Start();
        }

        public DisplayCalibrationCertificate()
        {
            InitializeComponent();
        }
    }
}
