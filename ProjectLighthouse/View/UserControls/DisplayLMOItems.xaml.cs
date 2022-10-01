using ProjectLighthouse.Model.Orders;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayLMOItems : UserControl
    {
        public LatheManufactureOrderItem LatheManufactureOrderItem
        {
            get { return (LatheManufactureOrderItem)GetValue(LatheManufactureOrderItemProperty); }
            set { SetValue(LatheManufactureOrderItemProperty, value); }
        }

        public static readonly DependencyProperty LatheManufactureOrderItemProperty =
            DependencyProperty.Register("LatheManufactureOrderItem", typeof(LatheManufactureOrderItem), typeof(DisplayLMOItems), new PropertyMetadata(null, SetValues));



        public ICommand EditCommand
        {
            get { return (ICommand)GetValue(EditCommandProperty); }
            set { SetValue(EditCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register("EditCommand", typeof(ICommand), typeof(DisplayLMOItems), new PropertyMetadata(null));



        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DisplayLMOItems control)
            {
                control.DataContext = control.LatheManufactureOrderItem;

                control.specialFlag.Visibility = (control.LatheManufactureOrderItem.IsSpecialPart) ? Visibility.Visible : Visibility.Collapsed;
                control.requirements.Visibility = (control.LatheManufactureOrderItem.RequiredQuantity == 0) ? Visibility.Collapsed : Visibility.Visible;

                control.requirements.BorderBrush = (control.LatheManufactureOrderItem.QuantityDelivered >= control.LatheManufactureOrderItem.RequiredQuantity) ? // Customer requirement fulfilled
                    (Brush)Application.Current.Resources["Green"] : (Brush)Application.Current.Resources["Red"];

                control.requirementsBg.Background = (control.LatheManufactureOrderItem.QuantityDelivered >= control.LatheManufactureOrderItem.RequiredQuantity) ? // Customer requirement fulfilled
                    (Brush)Application.Current.Resources["Green"] : (Brush)Application.Current.Resources["Red"];

                control.qtyrequired.Foreground = (control.LatheManufactureOrderItem.QuantityDelivered >= control.LatheManufactureOrderItem.RequiredQuantity) ? // Customer requirement fulfilled
                    (Brush)Application.Current.Resources["Green"] : (Brush)Application.Current.Resources["Red"];

                control.daterequired.Foreground = (control.LatheManufactureOrderItem.QuantityDelivered >= control.LatheManufactureOrderItem.RequiredQuantity) ? // Customer requirement fulfilled
                    (Brush)Application.Current.Resources["Green"] : (Brush)Application.Current.Resources["Red"];

                control.custReqTitle.Foreground = (control.LatheManufactureOrderItem.QuantityDelivered >= control.LatheManufactureOrderItem.RequiredQuantity) ? // Customer requirement fulfilled
                    (Brush)Application.Current.Resources["Green"] : (Brush)Application.Current.Resources["Red"];

                //control.ProgressBar.Foreground = control.LatheManufactureOrderItem.QuantityDelivered >= control.LatheManufactureOrderItem.TargetQuantity
                //    ? (Brush)Application.Current.Resources["Green"]
                //    : (Brush)Application.Current.Resources["Purple"];

                control.EditButton.Visibility = control.LatheManufactureOrderItem.ShowEdit
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                control.cycleTimeIndicator.Visibility = control.LatheManufactureOrderItem.QuantityMade > 0 && control.LatheManufactureOrderItem.CycleTime == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                //control.ProgressBar.Maximum = control.LatheManufactureOrderItem.TargetQuantity;
                //control.ProgressBar.Value = control.LatheManufactureOrderItem.QuantityMade;
            }
        }

        public DisplayLMOItems()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditCommand.Execute(LatheManufactureOrderItem.Id);
        }
    }
}
