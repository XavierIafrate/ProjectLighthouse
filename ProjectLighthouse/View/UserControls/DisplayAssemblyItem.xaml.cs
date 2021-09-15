using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayAssemblyItem.xaml
    /// </summary>
    public partial class DisplayAssemblyItem : UserControl
    {
        public AssemblyItem item
        {
            get { return (AssemblyItem)GetValue(itemProperty); }
            set { SetValue(itemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty itemProperty =
            DependencyProperty.Register("item", typeof(AssemblyItem), typeof(DisplayAssemblyItem), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayAssemblyItem control)
                return;

            control.DataContext = control.item;
        }

        public DisplayAssemblyItem()
        {
            InitializeComponent();
        }
    }
}
