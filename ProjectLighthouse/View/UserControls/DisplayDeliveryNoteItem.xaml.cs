using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Commands;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayDeliveryNoteItem.xaml
    /// </summary>
    public partial class DisplayDeliveryNoteItem : UserControl
    {


        public EditDeliveryNoteItemCommand EditCommand
        {
            get { return (EditDeliveryNoteItemCommand)GetValue(EditCommandProperty); }
            set { SetValue(EditCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register("EditCommand", typeof(EditDeliveryNoteItemCommand), typeof(DisplayDeliveryNoteItem), new PropertyMetadata(null, SetValues));



        public DeliveryItem Item
        {
            get { return (DeliveryItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(DeliveryItem), typeof(DisplayDeliveryNoteItem), new PropertyMetadata(null, SetValues));

        public DeliveryItem DelItem { get; set; }
        public EditDeliveryNoteItemCommand EditCmd { get; set; }

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayDeliveryNoteItem control)
                return;

            control.DataContext = control;
            if (e.Property.Name == nameof(Item))
            {
                control.DelItem = (DeliveryItem)e.NewValue;
            }
        }

        public DisplayDeliveryNoteItem()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EditCommand?.Execute(DelItem.Id);
        }
    }
}
