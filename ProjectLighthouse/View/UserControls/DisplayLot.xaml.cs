using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProjectLighthouse.Model;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayLot.xaml
    /// </summary>
    public partial class DisplayLot : UserControl
    {


        public Lot Lot
        {
            get { return (Lot)GetValue(LotProperty); }
            set { SetValue(LotProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Lot.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty LotProperty =
            DependencyProperty.Register("Lot", typeof(Lot), typeof(DisplayLot), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayLot control = d as DisplayLot;

            if (control == null)
                return;

            control.DataContext = control.Lot;

            if (control.Lot.IsReject)
            {
                control.HelperText.Text = "Rejected";
            }
            else if (control.Lot.IsDelivered)
            {
                control.HelperText.Text = "Delivered";
            }
            else
            {
                control.HelperText.Text = "Not Delivered";
            }
        }

        public DisplayLot()
        {
            InitializeComponent();
        }
    }
}
