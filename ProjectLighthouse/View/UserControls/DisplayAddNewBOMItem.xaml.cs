using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
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
            DisplayAddNewBOMItem control = d as DisplayAddNewBOMItem;

            if (control == null)
                return;

            control.DataContext = control.item;
        }

        public DisplayAddNewBOMItem()
        {
            InitializeComponent();
        }
    }
}
