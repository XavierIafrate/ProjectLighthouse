using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for DisplayProductRangeSnippet.xaml
    /// </summary>
    public partial class DisplayProductRangeSnippet : UserControl
    {
        public List<TurnedProduct> Items
        {
            get { return (List<TurnedProduct>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(List<TurnedProduct>), typeof(DisplayProductRangeSnippet), new PropertyMetadata(null, SetValues));

        public ProductGroup Info
        {
            get { return (ProductGroup)GetValue(InfoProperty); }
            set { SetValue(InfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Info.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InfoProperty =
            DependencyProperty.Register("Info", typeof(ProductGroup), typeof(DisplayProductRangeSnippet), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayProductRangeSnippet control = d as DisplayProductRangeSnippet;

            if (control == null)
                return;

            if (control.Items == null || control.Info == null)
                return;


            control.Visibility = string.IsNullOrEmpty(control.Info.ID) ? Visibility.Collapsed : Visibility.Visible;
            if (string.IsNullOrEmpty(control.Info.ID))
                return;

            //if (control.Info == "M00142")
            //    control.Visibility = Visibility.Collapsed;

            control.ProductGroupName.Text = control.Info.ID;
            control.Breadcrumb.Text = control.Info.Breadcrumb;
            control.ProductTitle.Text = control.Info.ProductTitle;
            control.ProductSubTitle.Text = " - " + control.Info.ProductSubTitle;
            string uri = App.ROOT_PATH + control.Info.LineDrawingURL;
            if (File.Exists(uri))
            {
                control.Receiver.Source = new BitmapImage(new Uri(uri, UriKind.Absolute));
                control.image.Visibility = Visibility.Visible;
            }
            else
            {
                control.image.Visibility = Visibility.Collapsed;
            }

            control.ProductDisplay.ItemsSource = control.Items;

            //control.DataContext = control.Items;
        }

        public DisplayProductRangeSnippet()
        { 
            InitializeComponent();
        }
    }
}
