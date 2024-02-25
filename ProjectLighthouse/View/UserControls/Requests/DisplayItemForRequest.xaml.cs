using DocumentFormat.OpenXml.Vml.Spreadsheet;
using ProjectLighthouse.Model.Products;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayItemForRequest : UserControl
    {
        public ICommand AddCommand
        {
            get { return (ICommand)GetValue(AddCommandProperty); }
            set { SetValue(AddCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register("AddCommand", typeof(ICommand), typeof(DisplayItemForRequest), new PropertyMetadata(null, SetValues));


        public TurnedProduct Item
        {
            get { return (TurnedProduct)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(TurnedProduct), typeof(DisplayItemForRequest), new PropertyMetadata(null, SetValues));


        public DisplayItemForRequest()
        {
            InitializeComponent();
        }

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayItemForRequest control) return;

            if (control.Item is null)
            {
                return;
            }

            control.AddButton.IsEnabled = control.AddCommand is not null && !control.Item.Retired;
            control.ItemNameText.Text = control.Item.ProductName;

            if (control.Item.AppendableOrder != null)
            {
                int numberNeeded = -control.Item.FreeStock();
                // TODO sort this out
                if (numberNeeded > 0)
                {
                    control.ActionText.Text = $"Increase quantity on {control.Item.AppendableOrder.Name}: +{numberNeeded:#,##0} pcs";
                }
                else
                {
                    // TODO What is this for?
                    control.ActionText.Text = $"{control.Item.LighthouseGuaranteedQuantity:#,##0}pcs on {control.Item.AppendableOrder.Name} [{numberNeeded} needed]";
                }

                control.ActionText.Foreground = (Brush)App.Current.Resources["Green"];
                control.ActionBackground.Background = (Brush)App.Current.Resources["GreenFaded"];
            }
            else if (control.Item.ZeroSetOrder != null)
            {
                control.ActionText.Text = $"Compatible with {control.Item.ZeroSetOrder.Name}";
                control.ActionText.Foreground = (Brush)App.Current.Resources["Blue"];
                control.ActionBackground.Background = (Brush)App.Current.Resources["BlueFaded"];
            }
            else if (control.Item.FreeStock() < 0)
            {
                control.ActionText.Text = $"{-control.Item.FreeStock():#,##0} Required";
                control.ActionText.Foreground = (Brush)App.Current.Resources["Orange"];
                control.ActionBackground.Background = (Brush)App.Current.Resources["OrangeFaded"];
            }
            else if (control.Item.QuantitySold > 0 && (double)control.Item.FreeStock() / control.Item.QuantitySold < 0.1)
            {
                control.ActionText.Text = $"Low Stock {control.Item.QuantityInStock:#,##0} / {control.Item.QuantitySold:#,##0}";
                control.ActionText.Foreground = (Brush)App.Current.Resources["Orange"];
                control.ActionBackground.Background = (Brush)App.Current.Resources["OrangeFaded"];
            }
            else
            {
                control.ActionText.Visibility = Visibility.Collapsed;
            }

            if (control.Item.Retired)
            {
                control.ActionText.Visibility = Visibility.Collapsed;
            }

            control.StockText.Text = $"{control.Item.QuantityInStock:#,##0} In Stock";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.Item is null)
            {
                return;
            }

            this.AddCommand?.Execute(this.Item);
        }
    }
}
