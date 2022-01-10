using ProjectLighthouse.Model.Administration;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayCalibratedEquipment : UserControl
    {
        public CalibratedEquipment Equipment
        {
            get { return (CalibratedEquipment)GetValue(EquipmentProperty); }
            set { SetValue(EquipmentProperty, value); }
        }

        public static readonly DependencyProperty EquipmentProperty =
            DependencyProperty.Register("Equipment", typeof(CalibratedEquipment), typeof(DisplayCalibratedEquipment), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayCalibratedEquipment control)
            {
                return;
            }

            control.DataContext = control.Equipment;
        }

        public DisplayCalibratedEquipment()
        {
            InitializeComponent();
        }
    }
}
