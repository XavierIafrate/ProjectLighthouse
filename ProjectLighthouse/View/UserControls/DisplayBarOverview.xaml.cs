using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayBarOverview : UserControl
    {
        public BarStockRequirementOverview Bar
        {
            get { return (BarStockRequirementOverview)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public static readonly DependencyProperty BarProperty =
            DependencyProperty.Register("Bar", typeof(BarStockRequirementOverview), typeof(DisplayBarOverview), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayBarOverview control)
            {
                return;
            }

            control.BarID.Text = control.Bar.BarStock.Id;

            control.WarningFlag.Visibility = control.Bar.Priority == 0 ? Visibility.Visible : Visibility.Collapsed;
            control.WaitingFlag.Visibility = control.Bar.Priority == 1 ? Visibility.Visible : Visibility.Collapsed;
            control.OKFlag.Visibility = control.Bar.Priority >=2 ? Visibility.Visible : Visibility.Collapsed;
        }

        public DisplayBarOverview()
        {
            InitializeComponent();
        }

        private void Copy_Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Bar.BarStock.Id);
        }
    }
}
