using ProjectLighthouse.Model.Assembly;
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

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayBillOfMaterials.xaml
    /// </summary>
    public partial class DisplayBillOfMaterials : UserControl
    {
        public BillOfMaterialsItem BillOfMaterials
        {
            get { return (BillOfMaterialsItem)GetValue(BillOfMaterialsProperty); }
            set { SetValue(BillOfMaterialsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BillOfMaterials.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BillOfMaterialsProperty =
            DependencyProperty.Register("BillOfMaterials", typeof(BillOfMaterialsItem), typeof(DisplayBillOfMaterials), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayBillOfMaterials control = d as DisplayBillOfMaterials;

            if (control == null)
                return;

            control.DataContext = control.BillOfMaterials;
        }

        public DisplayBillOfMaterials()
        {
            InitializeComponent();
        }
    }
}
