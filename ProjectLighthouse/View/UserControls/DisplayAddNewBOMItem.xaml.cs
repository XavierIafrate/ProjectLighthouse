using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayAddNewBOMItem : UserControl
    {
        public AssemblyItem item
        {
            get { return (AssemblyItem)GetValue(itemProperty); }
            set { SetValue(itemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty itemProperty =
            DependencyProperty.Register("item", typeof(AssemblyItem), typeof(DisplayAddNewBOMItem), new PropertyMetadata(null, SetValues));

        public bool addClicked = false;

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayAddNewBOMItem control)
                return;

            control.DataContext = control.item;
        }

        public DisplayAddNewBOMItem()
        {
            InitializeComponent();
        }
    }
}
