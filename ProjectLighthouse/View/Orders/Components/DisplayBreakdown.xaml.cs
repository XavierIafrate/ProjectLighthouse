using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class DisplayBreakdown : UserControl
    {
        public string CodeText
        {
            get { return (string)GetValue(CodeTextProperty); }
            set { SetValue(CodeTextProperty, value); }
        }

        public static readonly DependencyProperty CodeTextProperty =
            DependencyProperty.Register("CodeText", typeof(string), typeof(DisplayBreakdown), new PropertyMetadata(string.Empty));



        public DisplayBreakdown()
        {
            InitializeComponent();
        }

        public void SetPending(bool pending) 
        {
            Brush fadedBrush = (Brush)App.Current.Resources[pending ? "OrangeFaded" : "RedFaded"];
            Brush solidBrush = (Brush)App.Current.Resources[pending ? "Orange" : "Red"];
            Brush foregroundBrush = (Brush)App.Current.Resources[pending ? "OnOrange" : "OnRed"];

            breakdown_border.Background = fadedBrush;
            breakdown_border.BorderBrush = solidBrush;
            borderPattern.Stroke = solidBrush;
            label.Background= solidBrush;
            labelText.Foreground= foregroundBrush;
        }
    }
}
