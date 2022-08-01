using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.ValueConverters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayQualityCheck : UserControl
    {
        public QualityCheck Check
        {
            get { return (QualityCheck)GetValue(CheckProperty); }
            set { SetValue(CheckProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Check.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckProperty =
            DependencyProperty.Register("Check", typeof(QualityCheck), typeof(DisplayQualityCheck), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayQualityCheck control) return;
            control.productText.Text = control.Check.Product;
            control.StatusText.Text = control.Check.Status;
            Brush flagBrush;
            if (control.Check.Status == "Accepted")
            {
                flagBrush  = (Brush)Application.Current.Resources["Green"];
            }
            else if (control.Check.Status == "Rejected")
            {
                flagBrush =(Brush)Application.Current.Resources["Red"];
            }
            else
            {
                flagBrush = (Brush)Application.Current.Resources["Blue"];
            }

            control.StatusText.Foreground = flagBrush;
            control.borderAccent.BorderBrush = flagBrush;
            control.borderBackground.Background = flagBrush;
        }

        public DisplayQualityCheck()
        {
            InitializeComponent();
        }
    }
}
