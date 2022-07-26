using ProjectLighthouse.Model;
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
    /// Interaction logic for DisplayQualityCheck.xaml
    /// </summary>
    public partial class DisplayQualityCheck : UserControl
    {


        public QualityCheck Check
        {
            get { return (QualityCheck)GetValue(CheckProperty); }
            set { SetValue(CheckProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Check.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckProperty =
            DependencyProperty.Register("Check", typeof(QualityCheck), typeof(DisplayQualityCheck), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayQualityCheck control) return;
            control.productText.Text = control.Check.Product;
        }

        public DisplayQualityCheck()
        {
            InitializeComponent();
        }
    }
}
