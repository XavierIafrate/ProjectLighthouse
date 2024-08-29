using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class DisplayLot : UserControl
    {
        public Lot Lot
        {
            get { return (Lot)GetValue(LotProperty); }
            set { SetValue(LotProperty, value); }
        }

        public static readonly DependencyProperty LotProperty =
            DependencyProperty.Register("Lot", typeof(Lot), typeof(DisplayLot), new PropertyMetadata(null));

        public DisplayLot()
        {
            InitializeComponent();
        }

        private void PrintLabelButton_Click(object sender, RoutedEventArgs e)
        {
            LabelPrintingHelper.PrintLot(Lot);
        }

        private void parentControl_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not DisplayLot control) return;
            PrintLabelButton.Visibility = control.IsMouseCaptureWithin ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
