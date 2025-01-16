using ProjectLighthouse.Model.Material;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayBarStock : UserControl
    {
        public BarStock Bar
        {
            get { return (BarStock)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public static readonly DependencyProperty BarProperty =
            DependencyProperty.Register("Bar", typeof(BarStock), typeof(DisplayBarStock), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayBarStock control) return;
            if (control.Bar is null) return;
        }

        public DisplayBarStock()
        {
            InitializeComponent();
        }
    }
}
