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
            DisplayAssemblyItem control = d as DisplayAssemblyItem;

            if (control == null)
                return;

            control.DataContext = control.item;
        }

        public DisplayAssemblyItem()
        {
            InitializeComponent();
        }
    }
}
