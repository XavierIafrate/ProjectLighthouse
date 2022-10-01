using ProjectLighthouse.Model.Quality;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            if (control.Equipment == null)
            {
                return;
            }
            control.DataContext = control.Equipment;

            Geometry g;
            Brush b;

            if (control.Equipment.IsOutOfService) // Failed Cal
            {
                g = Geometry.Parse("M8.27,3L3,8.27V15.73L8.27,21H15.73L21,15.73V8.27L15.73,3M8.41,7L12,10.59L15.59,7L17,8.41L13.41,12L17,15.59L15.59,17L12,13.41L8.41,17L7,15.59L10.59,12L7,8.41");
                b = (Brush)Application.Current.Resources["Red"];
            }
            else if (!control.Equipment.RequiresCalibration) // Indication Only
            {
                g = Geometry.Parse("M12,1L3,5V11C3,16.55 6.84,21.74 12,23C17.16,21.74 21,16.55 21,11V5M11,7H13V13H11M11,15H13V17H11");
                b = (Brush)Application.Current.Resources["Purple"];
            }
            else if (control.Equipment.CalibrationHasLapsed()) // Calibration Lapsed
            {
                g = Geometry.Parse("M8.27,3L3,8.27V15.73L8.27,21H15.73L21,15.73V8.27L15.73,3M8.41,7L12,10.59L15.59,7L17,8.41L13.41,12L17,15.59L15.59,17L12,13.41L8.41,17L7,15.59L10.59,12L7,8.41");
                b = (Brush)Application.Current.Resources["Red"];
            }
            else // All ok
            {
                g = Geometry.Parse("M9,20.42L2.79,14.21L5.62,11.38L9,14.77L18.88,4.88L21.71,7.71L9,20.42Z");
                b = (Brush)Application.Current.Resources["Green"];
            }

            control.indicator.Data = g;
            control.indicator.Fill = b;
        }

        public DisplayCalibratedEquipment()
        {
            InitializeComponent();
        }
    }
}
