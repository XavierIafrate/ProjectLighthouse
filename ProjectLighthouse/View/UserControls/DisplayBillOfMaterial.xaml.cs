using ProjectLighthouse.Model.Assembly;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayBillOfMaterial.xaml
    /// </summary>
    public partial class DisplayBillOfMaterial : UserControl
    {
        public BillOfMaterials BOM
        {
            get { return (BillOfMaterials)GetValue(BOMProperty); }
            set { SetValue(BOMProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BOM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BOMProperty =
            DependencyProperty.Register("BOM", typeof(BillOfMaterials), typeof(DisplayBillOfMaterial), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayBillOfMaterial control = d as DisplayBillOfMaterial;

            if (control == null)
                return;

            control.DataContext = control.BOM;
        }

        public DisplayBillOfMaterial()
        {
            InitializeComponent();
        }
    }
}
