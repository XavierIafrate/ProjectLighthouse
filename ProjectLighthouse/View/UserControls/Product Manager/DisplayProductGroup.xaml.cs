using ProjectLighthouse.Model.Products;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayProductGroup : UserControl
    {
        public ProductGroup ProductGroup
        {
            get { return (ProductGroup)GetValue(ProductGroupProperty); }
            set { SetValue(ProductGroupProperty, value); }
        }

        public static readonly DependencyProperty ProductGroupProperty =
            DependencyProperty.Register("ProductGroup", typeof(ProductGroup), typeof(DisplayProductGroup), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayProductGroup control) return;
            control.DataContext = control.ProductGroup;
        }

        public DisplayProductGroup()
        {
            InitializeComponent();
        }
    }
}
