using DocumentFormat.OpenXml.Drawing;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Requests;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayRequestProgress : UserControl
    {


        public Request TheRequest
        {
            get { return (Request)GetValue(TheRequestProperty); }
            set { SetValue(TheRequestProperty, value); }
        }

        public static readonly DependencyProperty TheRequestProperty =
            DependencyProperty.Register("TheRequest", typeof(Request), typeof(DisplayRequestProgress), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayRequestProgress control) return;
            if (control.TheRequest is null) return;
            if (control.TheRequest.SubsequentOrder is null) return;

            LatheManufactureOrder order = control.TheRequest.SubsequentOrder;
            LatheManufactureOrderItem? requestedItem = order.OrderItems.Find(x => x.ProductName == control.TheRequest.Product);

            Brush bRed = (Brush)Application.Current.Resources["Red"];
            Brush bGreen = (Brush)Application.Current.Resources["Green"];
            Brush bGrey = (Brush)Application.Current.Resources["DisabledElement"];

            control.Stage1.Background = bGreen;

            control.Stage2.Background = bGrey;
            control.Stage3.Background = bGrey;
            control.Stage4.Background = bGrey;

            control.Link1.Background = bGrey;
            control.Link2.Background = bGrey;
            control.Link3.Background = bGrey;

            if (requestedItem == null || order.State == OrderState.Cancelled)
            {
                control.Stage1.Background = bRed;
                return;
            }


            if (order.StartDate.Date != DateTime.MinValue)
            {
                control.Link1.Background = bGreen;
                control.Stage2.Background = bGreen;
            }

            if (order.State == OrderState.Running)
            {
                control.Link2.Background = bGreen;
                control.Stage3.Background = bGreen;
            }

            if (requestedItem.QuantityDelivered > requestedItem.RequiredQuantity)
            {
                control.Link3.Background = bGreen;
                control.Stage4.Background = bGreen;
            }

            control.redAlert.Visibility = Visibility.Collapsed;
            control.amberAlert.Visibility = Visibility.Collapsed;
            control.greenAlert.Visibility = Visibility.Collapsed;
            //Awaiting scheduling too 
            DateTime estimatedDelivery = order.StartDate.AddSeconds(requestedItem.GetTimeToMakeRequired());
            estimatedDelivery = estimatedDelivery.Date.AddDays(1);


        }

        public DisplayRequestProgress()
        {
            InitializeComponent();
        }
    }
}
