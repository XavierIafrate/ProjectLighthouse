using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayDeliveryNoteItem.xaml
    /// </summary>
    public partial class DisplayDeliveryNoteItem : UserControl
    {
        public DeliveryItem Item
        {
            get { return (DeliveryItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(DeliveryItem), typeof(DisplayDeliveryNoteItem), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayDeliveryNoteItem control = d as DisplayDeliveryNoteItem;

            if (control == null)
                return;

            control.DataContext = control.Item;
        }

        public DisplayDeliveryNoteItem()
        {
            InitializeComponent();
        }
    }
}
