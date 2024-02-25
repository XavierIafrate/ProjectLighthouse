using ProjectLighthouse.Model.Material;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayMaterialInfo : UserControl
    {
        public MaterialInfo MaterialInfo
        {
            get { return (MaterialInfo)GetValue(MaterialInfoProperty); }
            set { SetValue(MaterialInfoProperty, value); }
        }

        public static readonly DependencyProperty MaterialInfoProperty =
            DependencyProperty.Register("MaterialInfo", typeof(MaterialInfo), typeof(DisplayMaterialInfo), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayMaterialInfo control) return;
            if (control.MaterialInfo is null) return;
            control.DataContext = control.MaterialInfo;
        }

        public DisplayMaterialInfo()
        {
            InitializeComponent();
        }
    }
}
