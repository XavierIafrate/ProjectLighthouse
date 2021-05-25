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
    /// Interaction logic for DisplaySubAssembly.xaml
    /// </summary>
    public partial class DisplaySubAssembly : UserControl
    {
        public CompleteAssemblyProduct Assembly
        {
            get { return (CompleteAssemblyProduct)GetValue(AssemblyProperty); }
            set { SetValue(AssemblyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Assembly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AssemblyProperty =
            DependencyProperty.Register("Assembly", typeof(CompleteAssemblyProduct), typeof(DisplaySubAssembly), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplaySubAssembly control = d as DisplaySubAssembly;

            if (control == null)
                return;

            control.DataContext = control.Assembly;
        }

        public DisplaySubAssembly()
        {
            InitializeComponent();
        }
    }
}
