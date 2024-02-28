using ProjectLighthouse.Model.Deliveries;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayDeliveryNoteItem : UserControl
    {
        public DeliveryItem Item
        {
            get { return (DeliveryItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(DeliveryItem), typeof(DisplayDeliveryNoteItem), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayDeliveryNoteItem control)
                return;

            // TODO: Edit Delivery Notes
            //control.EditButton.Visibility = Visibility.Collapsed; //App.CurrentUser.Role == UserRole.Administrator ? Visibility.Visible : Visibility.Collapsed;
        }

        public DisplayDeliveryNoteItem()
        {
            InitializeComponent();
        }
    }
}
