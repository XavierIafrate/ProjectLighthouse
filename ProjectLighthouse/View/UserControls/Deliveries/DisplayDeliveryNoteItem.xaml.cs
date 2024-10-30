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
        }

        public DisplayDeliveryNoteItem()
        {
            InitializeComponent();
        }

        private void CopyExportCodeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Item.ExportProductName);
            }
            catch
            {
                MessageBox.Show("Failed to set clipboard", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyProductNameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Item.Product);
            }
            catch
            {
                MessageBox.Show("Failed to set clipboard", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
