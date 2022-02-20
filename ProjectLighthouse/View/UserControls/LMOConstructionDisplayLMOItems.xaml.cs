using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class LMOConstructionDisplayLMOItems : UserControl
    {
        public LatheManufactureOrderItem Item
        {
            get { return (LatheManufactureOrderItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(LatheManufactureOrderItem), typeof(LMOConstructionDisplayLMOItems), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not LMOConstructionDisplayLMOItems control)
            {
                return;
            }

            control.DataContext = control.Item;

            control.RequirementsBox.Visibility = control.Item.RequiredQuantity == 0
                ? Visibility.Collapsed
                : Visibility.Visible;

            control.CycleTimeText.Text = control.Item.CycleTime == 0 ? "Cycle time unknown" : $"Cycle time: {control.Item.CycleTime:0}s";
        }

        public LMOConstructionDisplayLMOItems()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Item.TargetQuantity = string.IsNullOrWhiteSpace(QuantityTextBox.Text) 
                ? Math.Max(Item.RequiredQuantity, 0)
                : Math.Max(int.Parse(QuantityTextBox.Text), Item.RequiredQuantity);
            Item.NotifyEditMade();
        }

        private void QuantityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            QuantityTextBox.Text = Item.TargetQuantity.ToString("0");
        }
    }
}
