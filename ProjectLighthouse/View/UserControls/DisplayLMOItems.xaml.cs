using ProjectLighthouse.Model.Orders;
using System;
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

                control.EditButton.Visibility = control.LatheManufactureOrderItem.ShowEdit
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                control.cycleTimeIndicator.Visibility = control.LatheManufactureOrderItem.QuantityMade > 0 || control.LatheManufactureOrderItem.CycleTime > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                control.CycleTimeText.Text = control.LatheManufactureOrderItem.CycleTime == 0
                    ? "?"
                    : $"{Math.Floor((double)control.LatheManufactureOrderItem.CycleTime / 60):0}m {control.LatheManufactureOrderItem.CycleTime % 60}s";

                if (control.cycleTimeIndicator.Visibility == Visibility.Visible && control.LatheManufactureOrderItem.CycleTime == 0)
                {
                    control.cycleTimeIndicator.Background = (Brush)Application.Current.Resources["RedFaded"];
                    control.cycleTimeIndicatorIcon.Fill = (Brush)Application.Current.Resources["Red"];
                    control.CycleTimeText.Foreground = (Brush)Application.Current.Resources["Red"];
                }
                else
                {
                    control.cycleTimeIndicator.Background = (Brush)Application.Current.Resources["TealFaded"];
                    control.cycleTimeIndicatorIcon.Fill = (Brush)Application.Current.Resources["Teal"];
                    control.CycleTimeText.Foreground = (Brush)Application.Current.Resources["Teal"];
                }

                if (control.LatheManufactureOrderItem.PreviousCycleTime is null)
                {
                    control.historicalCycleTimeIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    control.historicalCycleTimeText.Text = $"{Math.Floor((double)control.LatheManufactureOrderItem.PreviousCycleTime! / 60):0}m {control.LatheManufactureOrderItem.PreviousCycleTime % 60}s";

                }

                if (control.LatheManufactureOrderItem.ModelledCycleTime is null)
                {
                    control.modelledCycleTimeIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    control.modelledCycleTimeText.Text = $"{Math.Floor((double)control.LatheManufactureOrderItem.ModelledCycleTime! / 60):0}m {control.LatheManufactureOrderItem.ModelledCycleTime % 60}s";

                }

                if (control.LatheManufactureOrderItem.PreviousCycleTime is null)
                {
                    control.deltaIndicator.Visibility = Visibility.Collapsed;
                }
                else if (control.LatheManufactureOrderItem.CycleTime > 0 && control.LatheManufactureOrderItem.PreviousCycleTime != control.LatheManufactureOrderItem.CycleTime)
                {
                    int diff = control.LatheManufactureOrderItem.CycleTime - (int)control.LatheManufactureOrderItem.PreviousCycleTime;
                    string f;
                    string b;
                    string pd;

                    if (diff <= 0)
                    {
                        f = "Teal";
                        b = "TealFaded";
                        pd = "M10,4H14V13L17.5,9.5L19.92,11.92L12,19.84L4.08,11.92L6.5,9.5L10,13V4Z";
                    }
                    else
                    {
                        f = "Red";
                        b = "RedFaded";
                        pd = "M14,20H10V11L6.5,14.5L4.08,12.08L12,4.16L19.92,12.08L17.5,14.5L14,11V20Z";
                    }

                    control.deltaIndicator.Background = (Brush)Application.Current.Resources[b];
                    control.deltaSymbol.Fill = (Brush)Application.Current.Resources[f];
                    control.deltaSymbol.Data = Geometry.Parse(pd);
                    control.deltaIndicatorText.Foreground = (Brush)Application.Current.Resources[f];

                    control.deltaIndicatorText.Text = Math.Abs(diff).ToString("0");
                }
                else
                {
                    control.deltaIndicator.Visibility = Visibility.Collapsed;
                }
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
