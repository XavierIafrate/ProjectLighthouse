using ProjectLighthouse.Model.Material;
using System.Windows;
using System.Windows.Controls;

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

            control.WarningFlag.Visibility = control.Bar.Status == BarStockRequirementOverview.StockStatus.OrderNow ? Visibility.Visible : Visibility.Collapsed;
            control.LowStockFlag.Visibility = control.Bar.Status == BarStockRequirementOverview.StockStatus.LowStock ? Visibility.Visible : Visibility.Collapsed;
            control.WaitingFlag.Visibility = control.Bar.Status == BarStockRequirementOverview.StockStatus.OnOrder ? Visibility.Visible : Visibility.Collapsed;
            control.OKFlag.Visibility = control.Bar.Status == BarStockRequirementOverview.StockStatus.StockOk ? Visibility.Visible : Visibility.Collapsed;

            control.AlarmFlag.Visibility = control.Bar.UrgentProblem ? Visibility.Visible : Visibility.Collapsed;
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
