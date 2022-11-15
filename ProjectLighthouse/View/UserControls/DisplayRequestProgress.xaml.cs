using ProjectLighthouse.Model.Requests;
using System.Windows;
using System.Windows.Controls;

namespace View.UserControls
{
    public partial class DisplayRequestProgress : UserControl
    {


        public Request TheRequest
        {
            get { return (Request)GetValue(TheRequestProperty); }
            set { SetValue(TheRequestProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TheRequest.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TheRequestProperty =
            DependencyProperty.Register("TheRequest", typeof(Request), typeof(DisplayRequestProgress), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayRequestProgress control) return;


        }

        public DisplayRequestProgress()
        {
            InitializeComponent();
        }
    }
}
