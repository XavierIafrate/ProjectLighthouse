using ProjectLighthouse.Model.Orders;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class DisplayGeneralOrderRequirement : UserControl
    {
        public GeneralManufactureOrder Order
        {
            get { return (GeneralManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(GeneralManufactureOrder), typeof(DisplayGeneralOrderRequirement), new PropertyMetadata(null));


        public DisplayGeneralOrderRequirement()
        {
            InitializeComponent();
        }
    }
}
