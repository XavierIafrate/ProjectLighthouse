using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayDeliveryNote.xaml
    /// </summary>
    public partial class DisplayDeliveryNote : UserControl
    {
        public DeliveryNote deliveryNote
        {
            get { return (DeliveryNote)GetValue(deliveryNoteProperty); }
            set { SetValue(deliveryNoteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty deliveryNoteProperty =
            DependencyProperty.Register("deliveryNote", typeof(DeliveryNote), typeof(DisplayDeliveryNote), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayDeliveryNote control = d as DisplayDeliveryNote;
            if (control != null)
            {

            }
        }

        public DisplayDeliveryNote()
        {
            InitializeComponent();
        }
    }
}
