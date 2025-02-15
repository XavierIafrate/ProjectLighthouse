﻿using ProjectLighthouse.Model.Orders;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{

    public partial class DisplayOrderStateBadge : UserControl
    {
        public OrderState OrderState
        {
            get { return (OrderState)GetValue(OrderStateProperty); }
            set { SetValue(OrderStateProperty, value); }
        }

        public static readonly DependencyProperty OrderStateProperty =
            DependencyProperty.Register("OrderState", typeof(OrderState), typeof(DisplayOrderStateBadge), new PropertyMetadata(OrderState.Problem));

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(DisplayOrderStateBadge), new PropertyMetadata(14.0, SetFontSize));

        private static void SetFontSize(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayOrderStateBadge control) return;
            control.stateText.Margin = new(control.FontSize * .667, control.FontSize * .1, control.FontSize * .667, control.FontSize * .1);
        }

        public DisplayOrderStateBadge()
        {
            InitializeComponent();
        }
    }
}
