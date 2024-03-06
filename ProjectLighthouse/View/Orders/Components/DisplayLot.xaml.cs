using ProjectLighthouse.Model.Orders;
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
    }
}
