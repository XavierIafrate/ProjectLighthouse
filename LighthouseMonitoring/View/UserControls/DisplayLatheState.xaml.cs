using LighthouseMonitoring.Model;
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

namespace LighthouseMonitoring.View.UserControls
{
    public partial class DisplayLatheState : UserControl
    {
        public LatheState Lathe
        {
            get { return (LatheState)GetValue(LatheProperty); }
            set { SetValue(LatheProperty, value); }
        }

        public static readonly DependencyProperty LatheProperty =
            DependencyProperty.Register("Lathe", typeof(LatheState), typeof(DisplayLatheState), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayLatheState) return;


        }

        public DisplayLatheState()
        {
            InitializeComponent();
        }
    }
}
