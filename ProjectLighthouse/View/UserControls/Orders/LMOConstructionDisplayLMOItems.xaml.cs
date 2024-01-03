using ProjectLighthouse.Model.Orders;
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



            if (control.Item.PreviousCycleTime is null)
            {
                control.historicalCycleTimeIndicator.Visibility = Visibility.Collapsed;
            }
            else
            {
                control.historicalCycleTimeText.Text = $"{Math.Floor((double)control.Item.PreviousCycleTime! / 60):0}m {control.Item.PreviousCycleTime % 60}s";
            }

            if (control.Item.ModelledCycleTime is null)
            {
                control.modelledCycleTimeIndicator.Visibility = Visibility.Collapsed;
            }
            else
            {
                control.modelledCycleTimeText.Text = $"{Math.Floor((double)control.Item.ModelledCycleTime! / 60):0}m {control.Item.ModelledCycleTime % 60}s";
            }

            control.StockText.Text = $"{control.Item.QuantityInStock:#,##0} in stock";
        }

        public LMOConstructionDisplayLMOItems()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }
    }
}
